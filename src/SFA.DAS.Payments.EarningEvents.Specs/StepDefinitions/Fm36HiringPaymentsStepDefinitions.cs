using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Reqnroll;
using SFA.DAS.Payments.EarningEvents.Data;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.Internal.Commands;
using SFA.DAS.Payments.EarningEvents.Model;
using SFA.DAS.Payments.EarningEvents.Specs.Handlers;
using SFA.DAS.Payments.Model.Core.Incentives;
using UUIDNext;
using EarningType = SFA.DAS.Payments.EarningEvents.Model.EarningType;

namespace SFA.DAS.Payments.EarningEvents.Specs.StepDefinitions
{
    [Binding]
    public class Fm36HiringPaymentsStepDefinitions
    {
        private const short CollectionYear = 2627;
        private const int CollectionPeriod = 3;

        private TestSession testSession;
        private EarningsDataContext earningsDataContext;
        private DateTime startDate;
        private int ageAtStart;
        private Guid storedEarningsId;
        private EmployerType employerType;
        private CourseType courseType;

        [BeforeScenario("fm36HiringPayments")]
        public void BeforeScenario()
        {
            testSession = new TestSession();
            var connectionString = TestRunBindings.Config["ConnectionStrings:PaymentsConnectionString"];
            earningsDataContext = new EarningsDataContext(connectionString);
            storedEarningsId = Guid.Empty;
            employerType = EmployerType.NonLevy;
            courseType = CourseType.Apprenticeship;
        }

        [AfterScenario("fm36HiringPayments")]
        public async Task AfterScenario()
        {
            var ukprn = testSession.Provider.Ukprn;

            await earningsDataContext.Database.ExecuteSqlInterpolatedAsync($@"
                DELETE FROM Payments2.GrowthAndSkillsEarningPricePeriod
                WHERE GrowthAndSkillsEarningsId IN (SELECT EarningsId FROM Payments2.GrowthAndSkillsEarning WHERE UKPRN = {ukprn})");
            await earningsDataContext.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM Payments2.GrowthAndSkillsEarning WHERE UKPRN = {ukprn}");
        }

        [Given("an FM36 apprenticeship learner who started on (.*) aged (.*) at the start of learning")]
        public void GivenAnFm36ApprenticeshipLearner(DateTime learnerStartDate, int learnerAge)
        {
            startDate = learnerStartDate;
            ageAtStart = learnerAge;
        }

        [Given("the stored earnings are for a (.*) employer")]
        public void GivenTheStoredEarningsAreForAnEmployerType(string storedEmployerType)
        {
            employerType = Enum.Parse<EmployerType>(storedEmployerType);
        }

        [Given("the stored earnings have the course type (.*)")]
        public void GivenTheStoredEarningsHaveTheCourseType(string storedCourseType)
        {
            courseType = Enum.Parse<CourseType>(storedCourseType);
        }

        [Given("the following hiring payment earnings are stored for the learner")]
        public async Task GivenTheFollowingHiringPaymentEarningsAreStored(Table table)
        {
            storedEarningsId = Uuid.NewDatabaseFriendly(Database.SqlServer);
            var stored = new GrowthAndSkillsEarningModel
            {
                EarningsId = storedEarningsId,
                UKPRN = testSession.Provider.Ukprn,
                LearnerUln = testSession.Learner.Uln,
                LearnerKey = testSession.Learner.LearnerIdentifier,
                LearnerReference = testSession.Learner.LearnRefNumber,
                CourseCode = testSession.Learner.Course.StandardCode.ToString(),
                CourseReference = testSession.Learner.Course.LearnAimRef,
                StartDate = startDate,
                PlannedEndDate = startDate.AddYears(1),
                AgeAtStartOfTraining = (byte)ageAtStart,
                CourseType = courseType,
                PricePeriods = table.Rows.Select(row => new GrowthAndSkillsEarningPricePeriodModel
                {
                    GrowthAndSkillsEarningsId = storedEarningsId,
                    Price = 0,
                    StartDate = startDate,
                    EndDate = startDate.AddYears(1),
                    DeliveryPeriod = byte.Parse(row["Delivery Period"]),
                    AcademicYear = CollectionYear,
                    EarningType = Enum.Parse<EarningType>(row["Earning Type"]),
                    Amount = decimal.Parse(row["Amount"]),
                    EmployerAccountId = 310074,
                    FundingAccountId = 310074,
                    ApprenticeshipId = 589568,
                    EmployerType = employerType
                }).ToList()
            };

            earningsDataContext.GrowthAndSkillsEarnings.Add(stored);
            await earningsDataContext.SaveChangesAsync();
        }

        [Given("no hiring payment earnings are stored for the learner")]
        public void GivenNoHiringPaymentEarningsAreStoredForTheLearner()
        {
        }

        [When("the learner is processed by Earning Events")]
        public async Task WhenTheLearnerIsProcessedByEarningEvents()
        {
            var command = new ProcessLearnerCommand
            {
                Ukprn = testSession.Provider.Ukprn,
                CollectionYear = CollectionYear,
                CollectionPeriod = CollectionPeriod,
                JobId = testSession.JobId,
                RequestTime = DateTime.UtcNow,
                SubmissionDate = DateTime.UtcNow,
                IlrSubmissionDateTime = DateTime.UtcNow,
                Learner = BuildLearner()
            };

            await testSession.Pv2MessageContext.SendTo(TestRunBindings.Config["ProcessLearnerEndpoint"], command);
        }

        [Then("the ACT1 Earnings Event includes the stored hiring payments")]
        public void ThenTheAct1EventIncludesTheStoredHiringPayments(Table table)
        {
            var act1Event = ApprenticeshipContractType1EarningEventHandler.GetEvents(testSession.Learner).Single();

            foreach (var row in table.Rows)
            {
                var incentiveType = Enum.Parse<IncentiveEarningType>(row["Earning Type"]);
                var deliveryPeriod = byte.Parse(row["Delivery Period"]);
                var amount = decimal.Parse(row["Amount"]);

                var incentive = act1Event.IncentiveEarnings.SingleOrDefault(x => x.Type == incentiveType);
                Assert.That(incentive, Is.Not.Null, $"Expected {incentiveType} incentive earnings on the ACT1 event");
                Assert.That(incentive.Periods.Any(p => p.Period == deliveryPeriod && p.Amount == amount), Is.True,
                    $"Expected a {incentiveType} earning of {amount} for delivery period {deliveryPeriod}");
            }
        }

        [Then("the ACT1 Earnings Event includes the learner's on-programme earnings")]
        public void ThenTheAct1EventIncludesTheOnProgrammeEarnings()
        {
            var act1Event = ApprenticeshipContractType1EarningEventHandler.GetEvents(testSession.Learner).Single();

            Assert.That(act1Event.OnProgrammeEarnings, Is.Not.Empty);
            Assert.That(act1Event.StartDate, Is.EqualTo(startDate));
        }

        [Then("the ACT1 Earnings Event does not include hiring payments for delivery period (.*)")]
        public void ThenTheAct1EventDoesNotIncludeHiringPaymentsForDeliveryPeriod(byte deliveryPeriod)
        {
            var act1Event = ApprenticeshipContractType1EarningEventHandler.GetEvents(testSession.Learner).Single();

            Assert.That(HiringIncentives(act1Event).SelectMany(x => x.Periods).Any(p => p.Period == deliveryPeriod), Is.False,
                $"Expected no hiring payment earnings for delivery period {deliveryPeriod}");
        }

        [Then("the ACT1 Earnings Event does not include any hiring payments")]
        public void ThenTheAct1EventDoesNotIncludeAnyHiringPayments()
        {
            var act1Event = ApprenticeshipContractType1EarningEventHandler.GetEvents(testSession.Learner).Single();

            Assert.That(HiringIncentives(act1Event), Is.Empty, "Expected no hiring payment earnings on the ACT1 event");
        }

        [Then("an ACT1 Earnings Event is published for the learner")]
        public async Task ThenAnAct1EarningsEventIsPublished()
        {
            await testSession.WaitForIt(
                () => ApprenticeshipContractType1EarningEventHandler.GetEvents(testSession.Learner).Any(),
                "Timed out waiting for the ACT1 Earnings Event");
        }

        [Then("no GSL Apprenticeship Earnings Event is published for the learner")]
        public void ThenNoGslApprenticeshipEarningsEventIsPublished()
        {
            Assert.That(GSLApprenticeshipEarningsEventHandler.GetEvents(testSession.Learner), Is.Empty,
                "Expected no GSL Apprenticeship Earnings Event to be published");
        }

        private static IEnumerable<IncentiveEarning> HiringIncentives(ApprenticeshipContractType1EarningEvent act1Event)
        {
            return (act1Event.IncentiveEarnings ?? new List<IncentiveEarning>())
                .Where(x => x.Type == IncentiveEarningType.FirstEmployerHiringPayment || x.Type == IncentiveEarningType.SecondEmployerHiringPayment);
        }

        private FM36Learner BuildLearner()
        {
            var course = testSession.Learner.Course;
            const int aimSequenceNumber = 1;

            return new FM36Learner
            {
                LearnRefNumber = testSession.Learner.LearnRefNumber,
                ULN = testSession.Learner.Uln,
                PriceEpisodes = new List<PriceEpisode>
                {
                    new PriceEpisode
                    {
                        PriceEpisodeIdentifier = $"{course.StandardCode}-{startDate:dd/MM/yyyy}",
                        PriceEpisodeValues = new PriceEpisodeValues
                        {
                            PriceEpisodeContractType = "Levy Contract",
                            EpisodeStartDate = startDate,
                            PriceEpisodeAimSeqNumber = aimSequenceNumber,
                            PriceEpisodeFundLineType = course.FundingLineType,
                            PriceEpisodeActualEndDate = null,
                            PriceEpisodePlannedEndDate = startDate.AddYears(1),
                            TNP1 = 15000,
                            TNP2 = 0,
                            TNP3 = 0,
                            TNP4 = 0
                        },
                        PriceEpisodePeriodisedValues = new List<PriceEpisodePeriodisedValues>
                        {
                            Periodised("PriceEpisodeOnProgPayment", 500m),
                            Periodised("PriceEpisodeCompletionPayment", 0m),
                            Periodised("PriceEpisodeBalancePayment", 0m)
                        }
                    }
                },
                LearningDeliveries = new List<LearningDelivery>
                {
                    new LearningDelivery
                    {
                        AimSeqNumber = aimSequenceNumber,
                        LearningDeliveryValues = new LearningDeliveryValues
                        {
                            LearnAimRef = course.LearnAimRef,
                            LearnStartDate = startDate,
                            AgeAtProgStart = ageAtStart,
                            StdCode = course.StandardCode,
                            ProgType = 25,
                            FworkCode = null,
                            PwayCode = null
                        }
                    }
                }
            };
        }

        private static PriceEpisodePeriodisedValues Periodised(string attributeName, decimal amountInPeriod)
        {
            return new PriceEpisodePeriodisedValues
            {
                AttributeName = attributeName,
                Period1 = 0m,
                Period2 = 0m,
                Period3 = amountInPeriod,
                Period4 = 0m,
                Period5 = 0m,
                Period6 = 0m,
                Period7 = 0m,
                Period8 = 0m,
                Period9 = 0m,
                Period10 = 0m,
                Period11 = 0m,
                Period12 = 0m
            };
        }
    }
}
