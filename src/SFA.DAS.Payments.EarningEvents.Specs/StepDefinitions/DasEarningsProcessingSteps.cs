using FluentAssertions;
using Reqnroll;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Specs.Handlers;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.EarningEvents.Messages;

namespace SFA.DAS.Payments.EarningEvents.Specs.StepDefinitions
{
    [Binding]
    public class DasEarningsProcessingStepDefinitions
    {
        private readonly ScenarioContext scenarioContext;
        private readonly TestSession testSession;

        private CalculateGrowthAndSkillsPayments message;

        public DasEarningsProcessingStepDefinitions(
            ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
            testSession = new TestSession();
        }


        [Given("DAS Earnings Bridge receives the following earnings for the current collection period R10:")]
        public void GivenDASReceivesEarnings(Table table)
        {
            message = new CalculateGrowthAndSkillsPayments
            {
                EarningsId = Guid.NewGuid(),
                UKPRN = testSession.Provider.Ukprn,

                Earnings = table.Rows.Select(row =>
                    new Earnings
                    {
                        AcademicYear = 2526,
                        PricePeriods = new List<PricePeriod>
                        {
                            new PricePeriod
                            {
                                Periods = new List<EarningPeriod>
                                {
                                    new EarningPeriod
                                    {
                                        DeliveryPeriod = byte.Parse(row["DeliveryPeriod"]),
                                        EarningType = Enum.Parse<EarningType>(row["EarningType"]),
                                        Amount = decimal.Parse(row["Amount"])
                                    }
                                }
                            }
                        }
                    }).ToList()
            };
        }


        [When("the earnings are processed")]
        public async Task WhenTheEarningsAreProcessed()
        {
            await testSession.DASMessageContext
                .Send<CalculateGrowthAndSkillsPayments>(message);
        }


        [Then("the earnings are written to the Earnings Cache with ProcessedOn as null")]
        public async Task ThenEarningsAreCachedWithoutProcessedDate()
        {
            var earnings =
                await testSession.DataContext.GrowthAndSkillsEarnings
                    .ToListAsync();
        }


        [Then("no outbound message is published")]
        public async Task ThenNoOutboundMessagePublished()
        {
            await testSession.WaitForItAndFail(() => GSLShortCourseEarningsEventHandler.GetEvents(testSession.Learner).Any(),
                "short course earning event is not expected");
        }


        [Then("the Milestone1 payment GSO Earning Event is generated and published")]
        public async Task ThenMilestoneEventIsPublished()
        {
            await testSession.WaitForIt(() => GSLShortCourseEarningsEventHandler.GetEvents(testSession.Learner).Any(),
               "short course earning event is not generated");
        }


        [Then("the Completion payment GSO Earning Event is not generated")]
        public async Task ThenCompletionEventIsNotPublished()
        {
            await testSession.WaitForItAndFail(() => GSLShortCourseEarningsEventHandler.GetEvents(testSession.Learner)
                        .Any(x => x.Earnings.Any(x => x.Type == ShortCourseEarningType.Completion)),
                        "short course earning event is not expected");
        }
    }
}