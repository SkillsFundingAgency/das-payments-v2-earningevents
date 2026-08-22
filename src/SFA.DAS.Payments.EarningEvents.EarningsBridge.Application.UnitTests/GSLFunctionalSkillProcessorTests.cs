using Moq;
using Moq.AutoMock;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Processors;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.UnitTests
{
    [TestFixture]
    public class GSLFunctionalSkillProcessorTests 
    {
        private AutoMocker mocker;
        private CalculateGrowthAndSkillsPayments sourceMessage;

        [SetUp]
        public void SetUp() 
        { 
            mocker = new AutoMocker(MockBehavior.Loose);
            sourceMessage = new CalculateGrowthAndSkillsPayments
            {
                EarningsId = Guid.NewGuid(),
                EmployerContribution = 1000m,
                UKPRN = 10002233,
                Training = new Training
                {
                    CourseCode = "123456",
                    CourseType = Messages.External.CourseType.FunctionalSkill,
                    CourseReference = "ZSC00123",
                    LearningType = Messages.External.LearningType.MathsAndEnglish,
                    StartDate = new DateTime(2026, 1, 1),
                    TrainingStatus = TrainingStatus.Continuing,
                    AgeAtStartOfTraining = 25,
                    PlannedEndDate = new DateTime(2026, 1, 15),
                    ActualEndDate = new DateTime(2026, 1, 31),
                    LearningKey = Guid.NewGuid()
                },
                Learner = new Learner
                {
                    ULN = 12345678,
                    Reference = "LEARNREF001",
                    LearnerKey = Guid.NewGuid()
                },
                Earnings = new List<Earnings>
                {
                    new Earnings
                    {
                        AcademicYear = 2627,
                        PricePeriods = new List<PricePeriod>
                        {
                            new PricePeriod
                            {
                                StartDate = new DateTime(2026, 1, 1),
                                Price = 0,
                                EndDate = new DateTime(2026, 1, 31),
                                CompletionAmount = 0,
                                InstalmentAmount = 0,
                                NumberOfInstalments = 0,
                                Periods = new List<EarningPeriod>
                                {
                                    new EarningPeriod
                                    {
                                        Employer = new Employer
                                        {
                                            EmployerType = EmployerType.Levy,
                                            AccountId = 10000,
                                            FundingAccountId = 10000
                                        },
                                        Amount = 600m,
                                        DeliveryPeriod = 1,
                                        EarningType = EarningType.OnProgrammeMathsAndEnglish,
                                        LearningId = 123456
                                    },
                                    new EarningPeriod
                                    {
                                        Employer = new Employer
                                        {
                                            EmployerType = EmployerType.Levy,
                                            AccountId = 10001,
                                            FundingAccountId = 10001
                                        },
                                        Amount = 1200m,
                                        DeliveryPeriod = 2,
                                        EarningType = EarningType.BalancingMathsAndEnglish,
                                        LearningId = 123456
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        [Test]
        public async Task Does_Not_Publish_Events_If_No_Open_Periods()
        {
            var processor = mocker.CreateInstance<GSLFunctionalSkillProcessor>();

            await processor.Process(sourceMessage, new List<CollectionPeriodModel>());

            mocker.Verify<IPaymentsServiceBusPublisher>(x => x.Publish(It.IsAny<GSLFunctionalSkillEarningsEvent>()), Times.Never);
        }
    }
}