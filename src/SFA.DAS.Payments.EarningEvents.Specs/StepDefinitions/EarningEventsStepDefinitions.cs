using Reqnroll;
using SFA.DAS.Payments.EarningEvents.Specs.StepDefinitions;

namespace SFA.DAS.Payments.EarningEvents.Specs.StepDefinitions
{
    [Binding]
    public class EarningEventsStepDefinitions
    {
        private readonly ScenarioContext scenarioContext;
        private readonly MessagingContext messagingContext;
        private TestSession testSession;
        private Model.Core.CollectionPeriod collectionPeriod;
        private short currentAcademicYear;

        public EarningEventsStepDefinitions(ScenarioContext scenarioContext, MessagingContext messagingContext)
        {
            this.scenarioContext = scenarioContext;
            this.messagingContext = messagingContext;            
        }

        protected void SetCurrentCollectionYear()
        {
            currentAcademicYear = new CollectionPeriodBuilder().WithDate(DateTime.Today).Build().AcademicYear;
        }

        [BeforeScenario]
        public async Task BeforeScenario()
        {
            testSession = new TestSession();
            await testSession.DataContext.ClearCollectionPeriodsData();
            SetCurrentCollectionYear();
            Console.WriteLine($"UKPRN : {testSession.Provider.Ukprn}, ULN: {testSession.Learner.Uln}, collection year: {currentAcademicYear}");
        }

        [AfterScenario]
        public void AfterScenario()
        {
        }




    }
}