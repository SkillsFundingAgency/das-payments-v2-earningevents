using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Reqnroll;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Mapping;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Model;
using SFA.DAS.Payments.EarningEvents.Specs.Handlers;
using SFA.DAS.Payments.Model.Core.Entities;
using UUIDNext;
using UUIDNext.Tools;
using ExternalCourseType = SFA.DAS.Payments.EarningEvents.Messages.External.CourseType;
using EarningPeriod = SFA.DAS.Payments.EarningEvents.Messages.External.EarningPeriod;
using EarningType = SFA.DAS.Payments.EarningEvents.Messages.External.EarningType;
using EmployerType = SFA.DAS.Payments.EarningEvents.Messages.External.EmployerType;
using Learner = SFA.DAS.Payments.EarningEvents.Messages.External.Learner;
using LearningType = SFA.DAS.Payments.EarningEvents.Messages.External.LearningType;
using ModelCourseType = SFA.DAS.Payments.EarningEvents.Model.CourseType;
using ModelLearningType = SFA.DAS.Payments.EarningEvents.Model.LearningType;
using TrainingStatus = SFA.DAS.Payments.EarningEvents.Messages.External.TrainingStatus;

namespace SFA.DAS.Payments.EarningEvents.Specs.StepDefinitions
{
    [Binding]
    public class WithholdPaymentsForLearningStepDefinitions
    {
        private readonly GrowthAndSkillsMapper mapper = new();
        private List<Guid> earningIds = null!;
        private List<Guid> unrelatedEarningIds = null!;
        private TestSession testSession = null!;
        private CollectionPeriodModel collectionPeriod = null!;
        private CalculateGrowthAndSkillsPayments originalEarnings = null!;
        private DateTime withholdingSentAt;
        private HashSet<Guid> existingNotificationIds = null!;

        [BeforeScenario("withholdEarnings")]
        public void BeforeScenario()
        {
            testSession = new TestSession();
            earningIds = new List<Guid>();
            unrelatedEarningIds = new List<Guid>();
            existingNotificationIds = new HashSet<Guid>();
        }

        [AfterScenario("withholdEarnings")]
        public async Task AfterScenario()
        {
            if (testSession == null)
            {
                return;
            }

            var processingRecords = await testSession.DataContext.GrowthAndSkillsEarningsProcessing
                .Where(x => earningIds.Contains(x.GrowthAndSkillsEarningId)).ToListAsync();
            testSession.DataContext.GrowthAndSkillsEarningsProcessing.RemoveRange(processingRecords);

            var pricePeriods = await testSession.DataContext.GrowthAndSkillsEarningPricePeriods
                .Where(x => earningIds.Contains(x.GrowthAndSkillsEarningsId)).ToListAsync();
            testSession.DataContext.GrowthAndSkillsEarningPricePeriods.RemoveRange(pricePeriods);

            var earnings = await testSession.DataContext.GrowthAndSkillsEarnings
                .Where(x => earningIds.Contains(x.EarningsId)).ToListAsync();
            testSession.DataContext.GrowthAndSkillsEarnings.RemoveRange(earnings);

            if (collectionPeriod != null)
            {
                testSession.DataContext.CollectionPeriods.Remove(collectionPeriod);
            }

            await testSession.DataContext.SaveChangesAsync();
            await testSession.DataContext.DisposeAsync();
        }

        [Given("a collection period is open for withholding")]
        public async Task GivenACollectionPeriodIsOpenForWithholding()
        {
            var period = new CollectionPeriodBuilder().WithDate(DateTime.Today).Build();
            collectionPeriod = new CollectionPeriodModel
            {
                AcademicYear = period.AcademicYear,
                Period = period.Period,
                StartDateTime = DateTime.Today,
                CompletionDate = DateTime.Today,
                Status = CollectionPeriodStatus.Open
            };
            testSession.DataContext.CollectionPeriods.Add(collectionPeriod);
            await testSession.DataContext.SaveChangesAsync();
        }

        [Given("received {string} earnings are stored")]
        public async Task GivenReceivedEarningsAreStored(string courseType)
        {
            var isApprenticeship = courseType == "apprenticeship";
            originalEarnings = new CalculateGrowthAndSkillsPayments
            {
                EarningsId = Uuid.NewDatabaseFriendly(Database.SqlServer),
                UKPRN = testSession.Provider.Ukprn,
                EmployerContribution = 0m,
                Learner = new Learner
                {
                    ULN = testSession.Learner.Uln,
                    LearnerKey = testSession.Learner.LearnerIdentifier,
                    Reference = testSession.Learner.LearnRefNumber
                },
                Training = new Training
                {
                    LearningKey = Guid.NewGuid(),
                    CourseCode = isApprenticeship ? "APPR001" : "ZSC00001",
                    CourseReference = isApprenticeship ? "ZPROG001" : "ZC0001",
                    CourseType = isApprenticeship ? ExternalCourseType.Apprenticeship : ExternalCourseType.ShortCourse,
                    LearningType = isApprenticeship ? LearningType.Apprenticeship : LearningType.ApprenticeshipUnit,
                    AgeAtStartOfTraining = 25,
                    StartDate = DateTime.Today.AddMonths(-1),
                    PlannedEndDate = DateTime.Today.AddMonths(11),
                    TrainingStatus = TrainingStatus.Continuing
                },
                Earnings = new List<Earnings>
            {
                new()
                {
                    AcademicYear = collectionPeriod.AcademicYear,
                    PricePeriods = new List<PricePeriod>
                    {
                        new()
                        {
                            StartDate = DateTime.Today.AddMonths(-1),
                            Price = 1000m,
                            Periods = new List<EarningPeriod>
                            {
                                new()
                                {
                                    DeliveryPeriod = 1,
                                    EarningType = isApprenticeship ? EarningType.Learning : EarningType.Milestone1,
                                    Amount = 300m,
                                    Employer = new Employer
                                    {
                                        AccountId = 123456,
                                        FundingAccountId = 123456,
                                        EmployerType = EmployerType.Levy
                                    },
                                    LearningId = 12345
                                }
                            }
                        }
                    }
                }
            }
            };

            var earning = mapper.MapToGrowthAndSkillsEarningModel(originalEarnings);
            earningIds.Add(earning.EarningsId);
            testSession.DataContext.GrowthAndSkillsEarnings.Add(earning);
            await testSession.DataContext.SaveChangesAsync();
        }

        [Given("other received earnings differ by each identifying field")]
        public async Task GivenOtherReceivedEarningsDifferByEachIdentifyingField()
        {
            var changes = new Action<GrowthAndSkillsEarningModel>[]
            {
                earning => earning.LearningKey = Guid.NewGuid(),
                earning => earning.UKPRN++,
                earning => earning.LearnerUln++,
                earning => earning.CourseCode = "OTHER",
                earning => earning.CourseType = earning.CourseType == ModelCourseType.Apprenticeship ? ModelCourseType.ShortCourse : ModelCourseType.Apprenticeship,
                earning => earning.LearningType = ModelLearningType.MathsAndEnglish
            };

            foreach (var change in changes)
            {
                var earning = mapper.MapToGrowthAndSkillsEarningModel(originalEarnings);
                earning.EarningsId = Uuid.NewDatabaseFriendly(Database.SqlServer);
                foreach (var pricePeriod in earning.PricePeriods)
                {
                    pricePeriod.GrowthAndSkillsEarningsId = earning.EarningsId;
                }
                change(earning);
                earningIds.Add(earning.EarningsId);
                unrelatedEarningIds.Add(earning.EarningsId);
                testSession.DataContext.GrowthAndSkillsEarnings.Add(earning);
            }

            await testSession.DataContext.SaveChangesAsync();
        }

        [Given("Payments receives a WithholdPaymentsForLearningEvent")]
        public async Task GivenPaymentsReceivesAWithholdPaymentsForLearningEvent()
        {
            await SendWithholdingEvent();
        }

        [Given("a withholding event has invalidated the earnings")]
        public async Task GivenAWithholdingEventHasInvalidatedTheEarnings()
        {
            await SendWithholdingEvent();
            await WaitForInvalidation();
        }

        [When("the Earnings Events Bridge finds matching received earnings using the details on the event")]
        public async Task WhenTheBridgeFindsMatchingReceivedEarnings()
        {
            await WaitForInvalidation();
            await HasDASEarningEventBeenPublished();
        }

        [When("the Earnings Events Bridge publishes a DAS earnings received event for invalidated earnings")]
        public async Task WhenTheBridgePublishesAnEarningsReceivedEventForInvalidatedEarnings()
        {
            await HasDASEarningEventBeenPublished();
        }

        [When("the invalidated earnings are reprocessed")]
        public async Task WhenTheInvalidatedEarningsAreReprocessed()
        {
            var baseUrl = TestRunBindings.Config["DASEarningsBridgeFunctionBaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                Assert.Fail("DASEarningsBridgeFunctionBaseUrl is not configured - set it in appSettings.development.json to the deployed DASEarningsBridge Function app's base URL.");
            }

            var functionKey = TestRunBindings.Config["DASEarningsBridgeFunctionKey"];
            using var httpClient = new HttpClient();
            using var response = await httpClient.PostAsync($"{baseUrl!.TrimEnd('/')}/ReprocessPendingEarnings?code={functionKey}", null);
            Assert.That(response.IsSuccessStatusCode, Is.True, $"Reprocessing returned {(int)response.StatusCode} {response.StatusCode}.");
        }

        [When("new approved earnings are received for the training")]
        public async Task WhenNewApprovedEarningsAreReceivedForTheTraining()
        {
            var approvedEarningsId = Uuid.NewDatabaseFriendly(Database.SqlServer);
            earningIds.Add(approvedEarningsId);
            var approvedEarnings = new CalculateGrowthAndSkillsPayments
            {
                EarningsId = approvedEarningsId,
                UKPRN = originalEarnings.UKPRN,
                EmployerContribution = originalEarnings.EmployerContribution,
                Learner = originalEarnings.Learner,
                Training = originalEarnings.Training,
                Earnings = originalEarnings.Earnings
            };
            await testSession.DASMessageContext.Send(approvedEarnings);
            await testSession.WaitForIt(() => testSession.DataContext.GrowthAndSkillsEarnings.AsNoTracking()
                .AnyAsync(x => x.EarningsId == approvedEarningsId), "The approved earnings were not received by the bridge.");
        }

        [Then("the matching received earnings are marked as Invalidated")]
        public async Task ThenOnlyTheMatchingReceivedEarningsAreStoredAsInvalidated()
        {
            var earnings = await testSession.DataContext.GrowthAndSkillsEarnings.AsNoTracking()
                .Where(x => earningIds.Contains(x.EarningsId)).ToListAsync();

            Assert.That(earnings.Count, Is.EqualTo(earningIds.Count));
            Assert.That(earnings.Single(x => x.EarningsId == originalEarnings.EarningsId).IsInvalidated, Is.True);
            Assert.That(earnings.Where(x => unrelatedEarningIds.Contains(x.EarningsId)).All(x => x.IsInvalidated != true), Is.True);
        }

        [Then("the invalidated state is stored against the received earnings")]
        public async Task ThenTheInvalidatedStateIsStoredAgainstTheReceivedEarnings()
        {
            var storedEarning = await testSession.DataContext.GrowthAndSkillsEarnings.AsNoTracking()
                .SingleAsync(x => x.EarningsId == originalEarnings.EarningsId);
            Assert.That(storedEarning.IsInvalidated, Is.True);
        }

        [Then("the original received earnings remain invalidated")]
        public async Task ThenTheOriginalReceivedEarningsRemainInvalidated()
        {
            await WaitForInvalidation();
        }

        [Then("the invalidated earnings are ignored")]
        public void ThenTheInvalidatedEarningsAreIgnored()
        {
            Assert.That(HasEarningsEvent(originalEarnings.EarningsId), Is.False);
        }

        [Then("no further payments are generated from those earnings")]
        public async Task ThenNoFurtherPaymentsAreGeneratedFromThoseEarnings()
        {
            var processed = await testSession.DataContext.GrowthAndSkillsEarningsProcessing.AsNoTracking()
                .AnyAsync(x => x.GrowthAndSkillsEarningId == originalEarnings.EarningsId && x.ProcessedOn != null);
            Assert.That(processed, Is.False);
        }

        [Then("a DasEarningsReceivedEvent is published with a temporary time-based EarningsId")]
        public async Task ThenAnEarningsReceivedEventIsPublishedWithATemporaryTimeBasedEarningsId()
        {
            var notification = await HasDASEarningEventBeenPublished();
            Assert.That(notification.EarningsId, Is.Not.EqualTo(Guid.Empty).And.Not.EqualTo(originalEarnings.EarningsId));
            Assert.That(UuidDecoder.TryDecodeTimestamp(notification.EarningsId, out var timestamp), Is.True);
            Assert.That(timestamp.ToUniversalTime(), Is.InRange(withholdingSentAt.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1)));
            Assert.That(notification.UKPRN, Is.EqualTo(originalEarnings.UKPRN));
            Assert.That(notification.ULN, Is.EqualTo(originalEarnings.Learner.ULN));
            Assert.That(notification.CourseCode, Is.EqualTo(originalEarnings.Training.CourseCode));
            Assert.That(notification.LearningAimReference, Is.EqualTo(originalEarnings.Training.CourseReference));
            Assert.That(notification.CollectionPeriod.AcademicYear, Is.EqualTo(collectionPeriod.AcademicYear));
            Assert.That(notification.CollectionPeriod.Period, Is.EqualTo(collectionPeriod.Period));
        }

        private async Task SendWithholdingEvent()
        {
            var message = new WithholdPaymentsForLearningEvent
            {
                LearningKey = originalEarnings.Training.LearningKey,
                UKPRN = originalEarnings.UKPRN,
                ULN = originalEarnings.Learner.ULN,
                CourseCode = originalEarnings.Training.CourseCode,
                LearningAimReference = originalEarnings.Training.CourseReference,
                CourseType = originalEarnings.Training.CourseType,
                LearningType = originalEarnings.Training.LearningType
            };
            existingNotificationIds = DasEarningsReceivedEventHandler.ReceivedEvents
                .Select(x => x.EarningsId)
                .ToHashSet();
            withholdingSentAt = DateTime.UtcNow;
            await testSession.DASMessageContext.Send(message);
        }

        private async Task WaitForInvalidation()
        {
            var originalEarningsId = earningIds[0];
            await testSession.WaitForIt(() => testSession.DataContext.GrowthAndSkillsEarnings.AsNoTracking()
                .AnyAsync(x => x.EarningsId == originalEarningsId && x.IsInvalidated == true), "The original earnings were not invalidated.");
        }

        private async Task<DasEarningsReceivedEvent> HasDASEarningEventBeenPublished()
        {
            await testSession.WaitForIt(() => FindDASEarningEvent() != null, "The earnings received notification was not published.");
            return FindDASEarningEvent()!;
        }

        private DasEarningsReceivedEvent? FindDASEarningEvent()
        {
            return DasEarningsReceivedEventHandler.ReceivedEvents.FirstOrDefault(x =>
                !existingNotificationIds.Contains(x.EarningsId)
                && x.UKPRN == originalEarnings.UKPRN
                && x.ULN == originalEarnings.Learner.ULN
                && x.CourseCode == originalEarnings.Training.CourseCode
                && x.LearningAimReference == originalEarnings.Training.CourseReference
                && x.CollectionPeriod.AcademicYear == collectionPeriod.AcademicYear
                && x.CollectionPeriod.Period == collectionPeriod.Period);
        }

        private bool HasEarningsEvent(Guid earningsId)
        {
            return GSLApprenticeshipEarningsEventHandler.GetEvents(testSession.Learner)
                       .Any(x => x.ExternalEarningsId == earningsId)
                   || GSLShortCourseEarningsEventHandler.GetEvents(testSession.Learner)
                       .Any(x => x.ExternalEarningsId == earningsId);
        }
    }
}
