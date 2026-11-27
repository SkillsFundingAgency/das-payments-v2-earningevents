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
using CollectionPeriod = SFA.DAS.Payments.Model.Core.CollectionPeriod;
using CourseType = SFA.DAS.Payments.EarningEvents.Messages.External.CourseType;
using EarningPeriod = SFA.DAS.Payments.EarningEvents.Messages.External.EarningPeriod;
using EarningType = SFA.DAS.Payments.EarningEvents.Messages.External.EarningType;
using EmployerType = SFA.DAS.Payments.EarningEvents.Messages.External.EmployerType;
using Learner = SFA.DAS.Payments.EarningEvents.Messages.External.Learner;
using LearningType = SFA.DAS.Payments.EarningEvents.Messages.External.LearningType;
using TrainingStatus = SFA.DAS.Payments.EarningEvents.Messages.External.TrainingStatus;

namespace SFA.DAS.Payments.EarningEvents.Specs.StepDefinitions
{
    [Binding]
    [Scope(Tag = "withholdEarnings")]
    public class WithholdPaymentsForLearningStepDefinitions
    {
        private readonly ScenarioContext scenarioContext;
        private readonly GrowthAndSkillsMapper mapper = new();
        private TestSession testSession;
        private CollectionPeriod currentCollectionPeriod;
        private readonly List<Guid> createdEarningIds = new();
        private readonly List<CollectionPeriodModel> openCollectionPeriods = new();
        private HashSet<DasEarningsReceivedEvent> existingEvents;

        public WithholdPaymentsForLearningStepDefinitions(ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
        }

        [BeforeScenario("withholdEarnings")]
        public async Task BeforeScenario()
        {
            testSession = new TestSession();
            currentCollectionPeriod = new CollectionPeriodBuilder().WithDate(DateTime.Today).Build();
            existingEvents = DasEarningsReceivedEventHandler.ReceivedEvents.ToHashSet();
            await testSession.DataContext.ClearCollectionPeriodsData();
        }

        [AfterScenario("withholdEarnings")]
        public async Task AfterScenario()
        {
            var processing = await testSession.DataContext.GrowthAndSkillsEarningsProcessing
                .Where(record => createdEarningIds.Contains(record.GrowthAndSkillsEarningId))
                .ToListAsync();
            testSession.DataContext.GrowthAndSkillsEarningsProcessing.RemoveRange(processing);

            var pricePeriods = await testSession.DataContext.GrowthAndSkillsEarningPricePeriods
                .Where(period => createdEarningIds.Contains(period.GrowthAndSkillsEarningsId))
                .ToListAsync();
            testSession.DataContext.GrowthAndSkillsEarningPricePeriods.RemoveRange(pricePeriods);

            var earnings = await testSession.DataContext.GrowthAndSkillsEarnings
                .Where(earning => createdEarningIds.Contains(earning.EarningsId))
                .ToListAsync();
            testSession.DataContext.GrowthAndSkillsEarnings.RemoveRange(earnings);

            testSession.DataContext.CollectionPeriods.RemoveRange(openCollectionPeriods);
            await testSession.DataContext.SaveChangesAsync();
            await testSession.DataContext.DisposeAsync();
        }

        [Given("there is at least one open collection period")]
        public async Task GivenThereIsAtLeastOneOpenCollectionPeriod()
        {
            var periods = new[]
            {
                (AcademicYear: currentCollectionPeriod.AcademicYear,
                    Period: currentCollectionPeriod.Period),
                (AcademicYear: (short)(currentCollectionPeriod.AcademicYear - 1),
                    Period: (byte)12)
            };

            foreach (var (academicYear, periodNumber) in periods)
            {
                var period = new CollectionPeriodModel
                {
                    AcademicYear = academicYear,
                    Period = periodNumber,
                    StartDateTime = DateTime.Today,
                    CompletionDate = DateTime.Today,
                    Status = CollectionPeriodStatus.Open
                };

                openCollectionPeriods.Add(period);
                testSession.DataContext.CollectionPeriods.Add(period);
            }

            await testSession.DataContext.SaveChangesAsync();
        }

        [Given("there are no open collection periods")]
        public async Task GivenThereAreNoOpenCollectionPeriods()
        {
            Assert.That(await testSession.DataContext.CollectionPeriods.AsNoTracking()
                .AnyAsync(period => period.Status == CollectionPeriodStatus.Open), Is.False);
        }

        [Given("previously received earnings have been marked as Invalidated")]
        [Given("the previous earnings for the training have been invalidated")]
        public async Task GivenPreviouslyReceivedEarningsHaveBeenMarkedAsInvalidated()
        {
            var earning = CreateEarning(
                earningsId: Uuid.NewDatabaseFriendly(Database.SqlServer),
                learningKey: Guid.NewGuid());
            scenarioContext.Set(earning, "PreviouslyReceivedEarning");
            await StoreEarning(earning, isInvalidated: true);
        }

        [Given("previously received earnings are stored")]
        public async Task GivenPreviouslyReceivedEarningsAreStored()
        {
            var earning = CreateEarning(
                earningsId: Uuid.NewDatabaseFriendly(Database.SqlServer),
                learningKey: Guid.NewGuid());
            scenarioContext.Set(earning, "PreviouslyReceivedEarning");
            await StoreEarning(earning, isInvalidated: null);
        }

        [Given("non-matching earnings are stored")]
        public async Task GivenNonMatchingEarningsAreStored()
        {
            var nonMatchingEarning = CreateEarning(
                earningsId: Uuid.NewDatabaseFriendly(Database.SqlServer),
                learningKey: Guid.NewGuid());

            scenarioContext.Set(nonMatchingEarning.EarningsId, "NonMatchingEarningId");
            await StoreEarning(nonMatchingEarning, isInvalidated: null);
        }

        [Given("valid earnings are awaiting reprocessing")]
        public async Task GivenValidEarningsAreAwaitingReprocessing()
        {
            var validEarning = CreateEarning(
                earningsId: Uuid.NewDatabaseFriendly(Database.SqlServer),
                learningKey: Guid.NewGuid());

            scenarioContext.Set(validEarning.EarningsId, "ValidPendingEarningId");
            await StoreEarning(validEarning, isInvalidated: null);
        }

        [Given("new approved earnings are available for the training")]
        public void GivenNewApprovedEarningsAreAvailableForTheTraining()
        {
            var previouslyReceivedEarning = scenarioContext.Get<CalculateGrowthAndSkillsPayments>("PreviouslyReceivedEarning");
            var approvedEarningsMessage = CreateEarning(
                earningsId: Uuid.NewDatabaseFriendly(Database.SqlServer),
                learningKey: previouslyReceivedEarning.Training.LearningKey);

            scenarioContext.Set(approvedEarningsMessage.EarningsId, "ApprovedEarningId");
            scenarioContext.Set(approvedEarningsMessage, "ApprovedEarningsMessage");
            createdEarningIds.Add(approvedEarningsMessage.EarningsId);
        }

        [When("Payments receives a WithholdPaymentsForLearningEvent")]
        public async Task WhenPaymentsReceivesAWithholdPaymentsForLearningEvent()
        {
            var previouslyReceivedEarning = scenarioContext.Get<CalculateGrowthAndSkillsPayments>("PreviouslyReceivedEarning");
            var withholdingEvent = new WithholdPaymentsForLearningEvent
            {
                LearningKey = previouslyReceivedEarning.Training.LearningKey,
                UKPRN = previouslyReceivedEarning.UKPRN,
                ULN = previouslyReceivedEarning.Learner.ULN,
                CourseCode = previouslyReceivedEarning.Training.CourseCode,
                CourseType = previouslyReceivedEarning.Training.CourseType,
                LearningType = previouslyReceivedEarning.Training.LearningType,
                LearningAimReference = previouslyReceivedEarning.Training.CourseReference
            };

            scenarioContext.Set(DateTime.UtcNow, "WithholdingSentAt");
            await testSession.DASMessageContext.Send(withholdingEvent);
        }

        [When("the earnings are reprocessed")]
        public async Task WhenTheEarningsAreReprocessed()
        {
            var baseUrl = TestRunBindings.Config["DASEarningsBridgeFunctionBaseUrl"];
            Assert.That(baseUrl, Is.Not.Null.And.Not.Empty, "DASEarningsBridgeFunctionBaseUrl is not configured.");

            var functionKey = TestRunBindings.Config["DASEarningsBridgeFunctionKey"] ?? string.Empty;
            var url = $"{baseUrl.TrimEnd('/')}/ReprocessPendingEarnings?code={Uri.EscapeDataString(functionKey)}";

            using var client = new HttpClient();
            using var response = await client.PostAsync(url, null);
            Assert.That(response.IsSuccessStatusCode, Is.True, $"Reprocessing returned {response.StatusCode}.");
        }

        [When("the new approved earnings are received")]
        public async Task WhenTheNewApprovedEarningsAreReceived()
        {
            var approvedEarningsMessage = scenarioContext.Get<CalculateGrowthAndSkillsPayments>("ApprovedEarningsMessage");
            await testSession.DASMessageContext.Send(approvedEarningsMessage);
        }

        [Then("the matching received earnings are marked as Invalidated")]
        public async Task ThenTheMatchingReceivedEarningsAreMarkedAsInvalidated()
        {
            var previouslyReceivedEarning = scenarioContext.Get<CalculateGrowthAndSkillsPayments>("PreviouslyReceivedEarning");
            await testSession.WaitForIt(
                () => testSession.DataContext.GrowthAndSkillsEarnings.AsNoTracking()
                    .AnyAsync(earning => earning.EarningsId == previouslyReceivedEarning.EarningsId
                        && earning.IsInvalidated == true),
                "The received earnings were not invalidated.");
        }

        [Then("non-matching earnings remain unaffected")]
        public async Task ThenNonMatchingEarningsRemainUnaffected()
        {
            var nonMatchingEarningId = scenarioContext.Get<Guid>("NonMatchingEarningId");
            var earning = await testSession.DataContext.GrowthAndSkillsEarnings.AsNoTracking()
                .SingleAsync(record => record.EarningsId == nonMatchingEarningId);

            Assert.That(earning.IsInvalidated, Is.Not.True);
        }

        [Then("a DasEarningsReceivedEvent is published for each open collection period")]
        public async Task ThenADasEarningsReceivedEventIsPublishedForEachOpenCollectionPeriod()
        {
            var previouslyReceivedEarning = scenarioContext.Get<CalculateGrowthAndSkillsPayments>("PreviouslyReceivedEarning");
            await testSession.WaitForIt(
                () => GetDasEarningsReceivedEventsForLearner().Count >= openCollectionPeriods.Count,
                "Timed out waiting for the DasEarningsReceivedEvents.");

            var publishedEvents = GetDasEarningsReceivedEventsForLearner();
            var expectedPeriods = openCollectionPeriods.Select(period => (period.AcademicYear, period.Period));
            var actualPeriods = publishedEvents.Select(earningsEvent =>
                ((short)earningsEvent.CollectionPeriod.AcademicYear, (byte)earningsEvent.CollectionPeriod.Period));

            Assert.That(actualPeriods, Is.EquivalentTo(expectedPeriods));
            Assert.That(publishedEvents.All(earningsEvent =>
                earningsEvent.CourseCode == previouslyReceivedEarning.Training.CourseCode
                && earningsEvent.LearningAimReference == previouslyReceivedEarning.Training.CourseReference), Is.True);
        }

        [Then("each event contains the same temporary EarningsId based on the current time")]
        public void ThenEachEventContainsTheSameTemporaryEarningsIdBasedOnTheCurrentTime()
        {
            var withholdingSentAt = scenarioContext.Get<DateTime>("WithholdingSentAt");
            var earningsIds = GetDasEarningsReceivedEventsForLearner().Select(earningsEvent => earningsEvent.EarningsId).Distinct().ToList();
            Assert.That(earningsIds, Has.Count.EqualTo(1));

            var temporaryEarningsId = earningsIds.Single();
            Assert.That(temporaryEarningsId, Is.Not.EqualTo(Guid.Empty));
            Assert.That(createdEarningIds, Does.Not.Contain(temporaryEarningsId));
            Assert.That(UuidDecoder.TryDecodeTimestamp(temporaryEarningsId, out var timestamp), Is.True);
            Assert.That(timestamp.ToUniversalTime(),
                Is.InRange(withholdingSentAt.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1)));
        }

        [Then("an earnings event is published for the valid earnings")]
        public async Task ThenAnEarningsEventIsPublishedForTheValidEarnings()
        {
            var validPendingEarningId = scenarioContext.Get<Guid>("ValidPendingEarningId");
            await testSession.WaitForIt(
                () => GSLApprenticeshipEarningsEventHandler.ReceivedEvents.Any(earningsEvent =>
                    earningsEvent.ExternalEarningsId == validPendingEarningId
                    && earningsEvent.Ukprn == testSession.Provider.Ukprn
                    && earningsEvent.Learner.Uln == testSession.Learner.Uln),
                "The valid earning was not processed during reprocessing.");
        }

        [Then("the invalidated earnings remain invalidated")]
        public async Task ThenTheInvalidatedEarningsRemainInvalidated()
        {
            var previouslyReceivedEarning = scenarioContext.Get<CalculateGrowthAndSkillsPayments>("PreviouslyReceivedEarning");
            var earning = await testSession.DataContext.GrowthAndSkillsEarnings.AsNoTracking()
                .SingleAsync(record => record.EarningsId == previouslyReceivedEarning.EarningsId);

            Assert.That(earning.IsInvalidated, Is.True);
        }

        [Then("the invalidated earnings remain ignored")]
        public async Task ThenTheInvalidatedEarningsRemainIgnored()
        {
            var previouslyReceivedEarning = scenarioContext.Get<CalculateGrowthAndSkillsPayments>("PreviouslyReceivedEarning");
            var earning = await testSession.DataContext.GrowthAndSkillsEarnings.AsNoTracking()
                .SingleAsync(record => record.EarningsId == previouslyReceivedEarning.EarningsId);

            Assert.That(earning.IsInvalidated, Is.True);

            await AssertNoEventsPublished(
                () => HasEarningsEvent(previouslyReceivedEarning.EarningsId),
                "An earnings event was published for the previously invalidated earning.");
        }

        [Then("no earnings events are published for the invalidated earnings")]
        public async Task ThenNoEarningsEventsArePublishedForTheInvalidatedEarnings()
        {
            var previouslyReceivedEarning = scenarioContext.Get<CalculateGrowthAndSkillsPayments>("PreviouslyReceivedEarning");
            await AssertNoEventsPublished(
                () => HasEarningsEvent(previouslyReceivedEarning.EarningsId),
                "An earnings event was published for the invalidated earning.");
        }

        [Then("an earnings event is published for the new approved earnings")]
        public async Task ThenAnEarningsEventIsPublishedForTheNewApprovedEarnings()
        {
            var approvedEarningId = scenarioContext.Get<Guid>("ApprovedEarningId");
            await testSession.WaitForIt(
                () => GSLApprenticeshipEarningsEventHandler.ReceivedEvents.Any(earningsEvent =>
                    earningsEvent.ExternalEarningsId == approvedEarningId
                    && earningsEvent.Ukprn == testSession.Provider.Ukprn
                    && earningsEvent.Learner.Uln == testSession.Learner.Uln),
                "No apprenticeship earnings event was published for the approved earnings.");
        }

        [Then("no DasEarningsReceivedEvent is published")]
        public async Task ThenNoDasEarningsReceivedEventIsPublished()
        {
            await AssertNoEventsPublished(
                () => GetDasEarningsReceivedEventsForLearner().Any(),
                "A DasEarningsReceivedEvent was published with no open collection periods.");
        }

        private CalculateGrowthAndSkillsPayments CreateEarning(Guid earningsId, Guid learningKey)
        {
            var startDate = DateTime.Today.AddMonths(-1);
            return new CalculateGrowthAndSkillsPayments
            {
                EarningsId = earningsId,
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
                    LearningKey = learningKey,
                    CourseCode = "APPR001",
                    CourseReference = testSession.Learner.Course.LearnAimRef,
                    CourseType = CourseType.Apprenticeship,
                    LearningType = LearningType.Apprenticeship,
                    AgeAtStartOfTraining = 25,
                    StartDate = startDate,
                    PlannedEndDate = startDate.AddMonths(12),
                    TrainingStatus = TrainingStatus.Continuing
                },
                Earnings = new List<Earnings>
                {
                    new Earnings
                    {
                        AcademicYear = currentCollectionPeriod.AcademicYear,
                        PricePeriods = new List<PricePeriod>
                        {
                            new PricePeriod
                            {
                                StartDate = startDate,
                                Price = testSession.Learner.Course.AgreedPrice,
                                Periods = new List<EarningPeriod>
                                {
                                    new EarningPeriod
                                    {
                                        DeliveryPeriod = 1,
                                        EarningType = EarningType.Learning,
                                        Amount = 300m,
                                        Employer = new Employer
                                        {
                                            AccountId = testSession.Provider.Ukprn,
                                            FundingAccountId = testSession.Provider.Ukprn,
                                            EmployerType = EmployerType.Levy
                                        },
                                        LearningId = testSession.JobId
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private async Task StoreEarning(CalculateGrowthAndSkillsPayments earningsMessage, bool? isInvalidated)
        {
            var earning = mapper.MapToGrowthAndSkillsEarningModel(earningsMessage);
            earning.IsInvalidated = isInvalidated;
            createdEarningIds.Add(earning.EarningsId);
            testSession.DataContext.GrowthAndSkillsEarnings.Add(earning);
            await testSession.DataContext.SaveChangesAsync();
        }

        private List<DasEarningsReceivedEvent> GetDasEarningsReceivedEventsForLearner()
        {
            return DasEarningsReceivedEventHandler.ReceivedEvents.Where(earningsEvent =>
                !existingEvents.Contains(earningsEvent)
                && earningsEvent.UKPRN == testSession.Provider.Ukprn
                && earningsEvent.ULN == testSession.Learner.Uln).ToList();
        }

        private bool HasEarningsEvent(Guid earningsId)
        {
            return GSLApprenticeshipEarningsEventHandler.ReceivedEvents.Any(earningsEvent =>
                       earningsEvent.ExternalEarningsId == earningsId)
                   || GSLShortCourseEarningsEventHandler.ReceivedEvents.Any(earningsEvent =>
                       earningsEvent.ExternalEarningsId == earningsId);
        }

        private static async Task AssertNoEventsPublished(Func<bool> hasEvent, string failureMessage)
        {
            Assert.That(hasEvent(), Is.False, failureMessage);
            await Task.Delay(TimeSpan.FromSeconds(10));
            Assert.That(hasEvent(), Is.False, failureMessage);
        }
    }
}
