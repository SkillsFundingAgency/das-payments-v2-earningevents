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

        [Given("an employer has already approved the initial funding a learner on an Apprenticeship Unit course")]
        public void GivenAnEmployerHasAlreadyApprovedTheInitialFundingALearnerOnAnApprenticeshipUnitCourse()
        {
            throw new PendingStepException();
        }

        [Given("the earnings were persisted")]
        public void GivenTheEarningsWerePersisted()
        {
            throw new PendingStepException();
        }

        [Given("the provider and employer have agreed a change to the delivery of training for the course within the same collection period as the previous earnings")]
        public void GivenTheProviderAndEmployerHaveAgreedAChangeToTheDeliveryOfTrainingForTheCourseWithinTheSameCollectionPeriodAsThePreviousEarnings()
        {
            throw new PendingStepException();
        }

        [Given("the change has resulted in new earnings generated for the training")]
        public void GivenTheChangeHasResultedInNewEarningsGeneratedForTheTraining()
        {
            throw new PendingStepException();
        }

        [When("the Payments Earnings Bridge component receives the DAS Earnings")]
        public void WhenThePaymentsEarningsBridgeComponentReceivesTheDASEarnings()
        {
            throw new PendingStepException();
        }

        [Then("it should convert them to a ShortCourseEarnings event")]
        public void ThenItShouldConvertThemToAShortCourseEarningsEvent()
        {
            throw new PendingStepException();
        }

        [Then("the earnings should use an identifier that is higher or later than the identifier used in the previous earnings")]
        public void ThenTheEarningsShouldUseAnIdentifierThatIsHigherOrLaterThanTheIdentifierUsedInThePreviousEarnings()
        {
            throw new PendingStepException();
        }



    }
}