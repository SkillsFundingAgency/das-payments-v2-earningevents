using Reqnroll;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Specs.Handlers;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using UUIDNext;
using UUIDNext.Tools;
using CourseType = SFA.DAS.Payments.EarningEvents.Messages.External.CourseType;
using EarningPeriod = SFA.DAS.Payments.EarningEvents.Messages.External.EarningPeriod;
using Learner = SFA.DAS.Payments.EarningEvents.Messages.External.Learner;

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
        private CollectionPeriod currentPeriod;
        private Guid previousIdentifier;

        public EarningEventsStepDefinitions(ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
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

        [Given("the collection period has opened recently")]
        [Given("that the collection period has opened recently")]
        public async Task GivenThatTheCollectionPeriodHasOpenedRecently()
        {
            currentPeriod = new CollectionPeriodBuilder().WithDate(DateTime.Today).Build();
            testSession.DataContext.CollectionPeriods.Add(new CollectionPeriodModel
            {
                AcademicYear = currentPeriod.AcademicYear,
                CalendarMonth = (byte)DateTime.Today.Month,
                CalendarYear = (byte)DateTime.Today.Year,
                CompletionDate = DateTime.Today,
                EndDateTime = null,
                Period = currentPeriod.Period,
                ReferenceDataValidationDate = null,
                StartDateTime = DateTime.Today,
                Status = CollectionPeriodStatus.Open
            });
            await testSession.DataContext.SaveChangesAsync();
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

        [Given("the Payments system has already recorded the payments and associated earnings for the most recent Earnings for the training")]
        public void GivenThePaymentsSystemHasAlreadyRecordedThePaymentsAndAssociatedEarningsForTheMostRecentEarningsForTheTraining()
        {
            throw new PendingStepException();
        }

        [Given("there was an issue in the DAS Earnings system resulting in an older set of earnings being sent to the Payments system")]
        public void GivenThereWasAnIssueInTheDASEarningsSystemResultingInAnOlderSetOfEarningsBeingSentToThePaymentsSystem()
        {
            throw new PendingStepException();
        }


        [Given("the Payments system has already recorded the payments and associated earnings transactions for earnings that were approved today")]
        public void GivenThePaymentsSystemHasAlreadyRecordedThePaymentsAndAssociatedEarningsTransactionsForEarningsThatWereApprovedToday()
        {
            throw new PendingStepException();
        }

        [Given("there was an issue in the DAS Earnings system resulting in the previous set of earnings being resent to the Payments system")]
        public void GivenThereWasAnIssueInTheDASEarningsSystemResultingInThePreviousSetOfEarningsBeingResentToThePaymentsSystem()
        {
            throw new PendingStepException();
        }

        [Given("an employer has approved funding for a short course training")]
        public void GivenAnEmployerHasApprovedFundingForAShortCourseTraining()
        {
            throw new PendingStepException();
        }

        [Given("the earnings for the initial verion of the training delivery were not sent to the payments system")]
        public void GivenTheEarningsForTheInitialVerionOfTheTrainingDeliveryWereNotSentToThePaymentsSystem()
        {
            throw new PendingStepException();
        }

        [Given("the employer approves funding for a change to the earnings delivery")]
        public void GivenTheEmployerApprovesFundingForAChangeToTheEarningsDelivery()
        {
            throw new PendingStepException();
        }


        [Given("a previous set of earnings were recorded for the short course")]
        public void GivenAPreviousSetOfEarningsWereRecordedForTheShortCourse()
        {
            previousIdentifier = Uuid.NewDatabaseFriendly(Database.SqlServer);
            Console.WriteLine($"Previous id is: {previousIdentifier}");
        }

        [When("new changes are approved and the resultant earnings are sent to the Payments system")]
        public async Task WhenNewChangesAreApprovedAndTheResultantEarningsAreSentToThePaymentsSystem()
        {
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
                    LearningType = Messages.External.LearningType.ApprenticeshipUnit,
                    PlannedEndDate = DateTime.Today.AddMonths(1),
                    StartDate = DateTime.Today,
                    TrainingStatus = TrainingStatus.Continuing,
                    LearningKey = Uuid.NewDatabaseFriendly(Database.SqlServer)
                },
                Earnings = new List<Earnings>
                {
                    new Earnings
                    {
                        AcademicYear = currentAcademicYear,
                        PricePeriods = new List<PricePeriod>
                        {
                            new PricePeriod
                            {
                                StartDate = DateTime.Now,
                                CompletionAmount = 700,
                                InstalmentAmount = 300,
                                NumberOfInstalments = 1,
                                Price = 1000,
                                Periods = new List<EarningPeriod>
                                {
                                    new EarningPeriod
                                    {
                                        Amount = 300,
                                        DeliveryPeriod = 1,
                                        EarningType = EarningType.Milestone1,
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
                    }

                }
            };
            await testSession.DASMessageContext.Send<CalculateGrowthAndSkillsPayments>(earnings);
        }

        [When("the Payments Earnings Bridge component receives the older, now invalid earnings")]
        public void WhenThePaymentsEarningsBridgeComponentReceivesTheOlderNowInvalidEarnings()
        {
            throw new PendingStepException();
        }


        [When("the Payments Earnings Bridge component receives the duplicate earnings")]
        public void WhenThePaymentsEarningsBridgeComponentReceivesTheDuplicateEarnings()
        {
            throw new PendingStepException();
        }

        [When("the Payments Earnings Bridge component receives the DAS Earnings")]
        public void WhenThePaymentsEarningsBridgeComponentReceivesTheDASEarnings()
        {
            throw new PendingStepException();
        }

        [Then("it should discard the earnings")]
        public void ThenItShouldDiscardTheEarnings()
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

        [Then("the new earnings should have identifiers that indicate they are later than the previous earnings")]
        public async Task ThenTheNewEarningsShouldHaveIdentifiersThatIndicateTheyAreLaterThanThePreviousEarnings()
        {
            await testSession.WaitForIt(() => GSLShortCourseEarningsEventHandler.GetEvents(testSession.Learner)
                .Any(earning => IsLaterThan(previousIdentifier, earning.EventId)),"Failed to find the short course earning event");
        }

        private bool IsLaterThan(Guid previousEventId, Guid newEventId)
        {
            Console.WriteLine($"Comparing previous guid: {previousEventId} to new guid: {newEventId}");
            
            var firstEventIdDecodesToTimestamp = UuidDecoder.TryDecodeTimestamp(previousEventId, out var firstEventDateTime);
            var secondEventIdDecodesToTimestamp = UuidDecoder.TryDecodeTimestamp(newEventId, out var secondEventDateTime);
            if (firstEventIdDecodesToTimestamp && secondEventIdDecodesToTimestamp)
            {
                if (firstEventDateTime >= secondEventDateTime)
                {
                    return false;
                }

                if (secondEventDateTime > firstEventDateTime)
                {
                    return true;
                }
            }

            return false;
        }
    }
}