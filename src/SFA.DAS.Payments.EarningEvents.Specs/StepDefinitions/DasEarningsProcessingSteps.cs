using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Reqnroll;
using SFA.DAS.Payments.EarningEvents.Messages;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Specs.Handlers;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Model.Core.OnProgramme;
using UUIDNext;
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
    public class DasEarningsProcessingStepDefinitions
    {
        private const string CalculateGrowthAndSkillsPaymentsKey = nameof(CalculateGrowthAndSkillsPayments);
        private readonly ScenarioContext scenarioContext;
        private TestSession testSession;
        private short currentAcademicYear;

        public DasEarningsProcessingStepDefinitions(ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
        }

        private void SetCurrentCollectionYear()
        {
            currentAcademicYear = new CollectionPeriodBuilder().WithDate(DateTime.Today).Build().AcademicYear;
        }

        [BeforeScenario]
        public async Task BeforeScenario()
        {
            testSession = new TestSession();
            await testSession.DataContext.ClearCollectionPeriodsData();
            await testSession.DataContext.ClearGrowthAndSkillsEarningsData();
            SetCurrentCollectionYear();
            Console.WriteLine($"UKPRN : {testSession.Provider.Ukprn}, ULN: {testSession.Learner.Uln}, collection year: {currentAcademicYear}");
        }

        [AfterScenario]
        public void AfterScenario()
        {}

        [Given("that the collection period has opened for R10")]
        public async Task GivenThatTheCollectionPeriodHasOpenedRecently()
        {
            var currentPeriod = new CollectionPeriodBuilder().WithDate(DateTime.Today).Build();
            currentPeriod.Period = 10;
            testSession.DataContext.CollectionPeriods.Add(new CollectionPeriodModel
            {
                AcademicYear = currentPeriod.AcademicYear,
                CalendarMonth = (byte)DateTime.Today.Month,
                CalendarYear = (short)DateTime.Today.Year,
                CompletionDate = DateTime.Today,
                EndDateTime = null,
                Period = 10,
                ReferenceDataValidationDate = null,
                StartDateTime = DateTime.Today,
                Status = CollectionPeriodStatus.Open
            });
            await testSession.DataContext.SaveChangesAsync();
        }

        [Given("DAS Earnings Bridge receives the following earnings for the current collection period R10:")]
        public void GivenDASEarningsBridgeReceivesTheFollowingEarningsForTheCurrentCollectionPeriodR10(Table table)
        {
            var earningsList = new List<Earnings>();

            var earnings = new CalculateGrowthAndSkillsPayments
            {
                EarningsId = Uuid.NewDatabaseFriendly(Database.SqlServer),
                UKPRN = testSession.Provider.Ukprn,
                EmployerContribution = 1,
                Learner = new Learner
                {
                    ULN = testSession.Learner.Uln,
                    LearnerKey = testSession.Learner.LearnerIdentifier,
                    Reference = testSession.Learner.LearnRefNumber,
                },
                Training = new Training
                {
                    AgeAtStartOfTraining = 21,
                    CourseCode = "ZSC00001",
                    CourseReference = "ZSC00001",
                    CourseType = CourseType.ShortCourse,
                    LearningType = LearningType.ApprenticeshipUnit,
                    PlannedEndDate = DateTime.Today.AddMonths(1),
                    StartDate = DateTime.Today,
                    TrainingStatus = TrainingStatus.Continuing,
                    LearningKey = Uuid.NewDatabaseFriendly(Database.SqlServer)
                },
                Earnings = earningsList
            };

            foreach (var row in table.Rows)
            {
                if (!byte.TryParse(row["DeliveryPeriod"], out var deliveryPeriod))
                {
                    throw new FormatException($"Invalid DeliveryPeriod value: '{row["DeliveryPeriod"]}'.");
                }

                if (!Enum.TryParse<EarningType>(row["EarningType"], true, out var earningType))
                {
                    throw new FormatException($"Invalid EarningType value: '{row["EarningType"]}'.");
                }

                if (!decimal.TryParse(row["Amount"], out var amount))
                {
                    throw new FormatException($"Invalid Amount value: '{row["Amount"]}'.");
                }

                earningsList.Add(new Earnings
                {
                    AcademicYear = currentAcademicYear,
                    PricePeriods = new List<PricePeriod>
                    {
                        new PricePeriod
                        {
                            StartDate = DateTime.Today,
                            EndDate = DateTime.Today.AddMonths(1),
                            Price = 1000,
                            CompletionAmount = 700,
                            InstalmentAmount = 300,
                            NumberOfInstalments = 1,
                            Periods = new List<EarningPeriod>
                            {
                                new EarningPeriod
                                {
                                    DeliveryPeriod = deliveryPeriod,
                                    EarningType = earningType,
                                    Amount = amount,
                                    Employer = new Employer
                                    {
                                        AccountId = 123456,
                                        EmployerType = EmployerType.Levy,
                                        FundingAccountId = 123456
                                    },
                                    LearningId = 12345
                                }
                            }
                        }
                    }
                });
            }
            scenarioContext[CalculateGrowthAndSkillsPaymentsKey] = earnings;
        }

        [When("the earnings are processed")]
        public async Task WhenTheEarningsAreProcessed()
        {
            var earnings = scenarioContext.Get<CalculateGrowthAndSkillsPayments>(CalculateGrowthAndSkillsPaymentsKey);
            await testSession.DASMessageContext.Send(earnings);
        }

        [Then("the earnings are written to the Earnings Cache with ProcessedOn as null")]
        public async Task ThenEarningsAreCachedWithoutProcessedDate()
        {
            await testSession.WaitForIt(async () =>
            {
                var earnings = await testSession.DataContext.GrowthAndSkillsEarnings
                    .Include(x => x.PricePeriods)
                    .ToListAsync();

                if (earnings.Count != 1)
                    return false;

                var earning = earnings.Single();
                if (earning.PricePeriods.Count != 1)
                    return false;

                var pricePeriod = earning.PricePeriods.Single();
                return pricePeriod.EarningType == Model.EarningType.Milestone1 && pricePeriod.ProcessedOn == null;
            }, "Milestone1 payment earning was not written to the cache");
        }

        [Then("the Milestone1 payment earning for the current collection period is written to the Earnings Cache with ProcessedOn populated")]
        public async Task TheMilestone1PaymentsEarningForTheCurrentCollectionPeriodIsWrittenToTheEarningsCacheWithProcessedOnPopulated()
        {
            await testSession.WaitForIt(async () =>
            {
                var earnings = await testSession.DataContext.GrowthAndSkillsEarnings
                    .Include(x => x.PricePeriods)
                    .ToListAsync();

                if (earnings.Count != 1)
                    return false;

                var earning = earnings.Single();
                if (earning.PricePeriods.Count != 2)
                    return false;

                var milestone1PaymentPricePeriod = earning.PricePeriods
                    .SingleOrDefault(x => x.EarningType == Model.EarningType.Milestone1);

                return milestone1PaymentPricePeriod != null && milestone1PaymentPricePeriod.ProcessedOn != null;
            }, "Milestone1 payment earning was not written to the cache");
        }

        [Then("no further processing is carried out")]
        public void ThenNoFurtherProcessingIsCarriedOut()
        {
        }

        [Then("no outbound message is published")]
        public async Task ThenNoOutboundMessagePublished()
        {
            var earnings = scenarioContext.Get<CalculateGrowthAndSkillsPayments>(CalculateGrowthAndSkillsPaymentsKey);

            await testSession.WaitForItAndFail(() => GSLShortCourseEarningsEventHandler.GetEvents(testSession.Learner)
                    .Any(x => x.ExternalEarningsId == earnings.EarningsId),
                "short course earning event is not expected");

            await testSession.WaitForItAndFail(() => DasEarningsReceivedEventHandler.ReceivedEvents.Any(x => x.EarningsId == earnings.EarningsId),
                "DAS Earnings Received Event is not expected");
        }

        [Then("the Milestone1 payment GSO Earning Event is generated and published")]
        public async Task ThenMilestoneEventIsPublished()
        {
            await testSession.WaitForIt(() => GSLShortCourseEarningsEventHandler.GetEvents(testSession.Learner)
                    .Any(x => x.Earnings.Any(e => e.Type == ShortCourseEarningType.Milestone1)),
               "short course earning event is not generated");
        }

        [Then("the Milestone1 payment DAS Earnings Received Event is generated and published")]
        public async Task ThenTheMilestonePaymentDASEarningsReceivedEventIsGeneratedAndPublished()
        {
            var earnings = scenarioContext.Get<CalculateGrowthAndSkillsPayments>(CalculateGrowthAndSkillsPaymentsKey);

            await testSession.WaitForIt(() => DasEarningsReceivedEventHandler.ReceivedEvents.Any(x => x.EarningsId == earnings.EarningsId),
                "das course earning event for milestone1 earning is not generated");

            DasEarningsReceivedEventHandler.ReceivedEvents
                .Should()
                .ContainSingle(x => x.EarningsId == earnings.EarningsId);
        }

        [Then("the Completion payment earning for the current collection period is written to the Earnings Cache with ProcessedOn as null")]
        public async Task ThenTheCompletionPaymentEarningForTheCurrentCollectionPeriodIsWrittenToTheEarningsCacheWithProcessedOnAsNull()
        {
            await testSession.WaitForIt(async () =>
            {
                var earnings = await testSession.DataContext.GrowthAndSkillsEarnings
                    .Include(x => x.PricePeriods)
                    .ToListAsync();

                if (earnings.Count != 1)
                    return false;

                var earning = earnings.Single();
                if (earning.PricePeriods.Count != 2)
                    return false;

                var completionPaymentPricePeriod = earning.PricePeriods
                    .SingleOrDefault(x => x.EarningType == Model.EarningType.Completion);

                return completionPaymentPricePeriod != null && completionPaymentPricePeriod.ProcessedOn == null;
            }, "Completion payment earning was not written to the cache with ProcessedOn as null");
        }

        [Then("the Completion payment GSO Earning Event is not generated")]
        public async Task ThenCompletionEventIsNotPublished()
        {
            await testSession.WaitForItAndFail(() => GSLShortCourseEarningsEventHandler.GetEvents(testSession.Learner)
                    .Any(x => x.Earnings.Any(e => e.Type == ShortCourseEarningType.Completion)),
                "short course earning event is not expected");
        }

        [Then("the Completion payment DAS Earnings Received Event is not generated")]
        public async Task ThenTheCompletionPaymentDASEarningsReceivedEventIsNotGenerated()
        {
            var earnings = scenarioContext.Get<CalculateGrowthAndSkillsPayments>(CalculateGrowthAndSkillsPaymentsKey);

            await testSession.WaitForItAndFail(() => DasEarningsReceivedEventHandler.ReceivedEvents.Any(x => x.EarningsId == earnings.EarningsId),
                "DAS Earnings Received Event is not expected");
        }

    }
}