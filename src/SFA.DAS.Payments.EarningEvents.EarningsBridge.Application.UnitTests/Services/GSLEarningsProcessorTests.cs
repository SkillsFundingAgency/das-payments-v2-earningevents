using Moq;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Validators;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Model;


namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.UnitTests.Services
{
    [TestFixture]
    public class GSLEarningsProcessorTests
    {
        private Moq.AutoMock.AutoMocker mocker;
        private ReceivedEarningsProcessor processor;
        private CalculateGrowthAndSkillsPayments earningsCommand;

        [SetUp]
        public void Setup()
        {
            mocker = new Moq.AutoMock.AutoMocker();
            processor = mocker.CreateInstance<ReceivedEarningsProcessor>();
            earningsCommand = new CalculateGrowthAndSkillsPayments
            {
                EarningsId = UUIDNext.Uuid.NewDatabaseFriendly(UUIDNext.Database.SqlServer),
                UKPRN = 12345678,
                Learner = new Messages.External.Learner
                {
                    ULN = 1234567890,
                    Reference = "LearnerReference",
                    LearnerKey = Guid.NewGuid()
                },
                Training = new Messages.External.Training
                {
                    CourseType = Messages.External.CourseType.Apprenticeship,
                    LearningKey = Guid.NewGuid(),
                    LearningType = Messages.External.LearningType.Apprenticeship,
                    CourseCode = "CourseCode",
                    CourseReference = "CourseReference",
                    StartDate = DateTime.Now
                },
                Earnings = new List<Messages.External.Earnings>
                {
                    new Messages.External.Earnings
                    {
                        AcademicYear = 2627,
                        PricePeriods = new List<Messages.External.PricePeriod>
                        {
                            new Messages.External.PricePeriod
                            {
                                CompletionAmount = 300,
                                Price = 15000,
                                StartDate = DateTime.Now.AddMonths(-6),
                                EndDate = DateTime.Now.AddMonths(6),
                                InstalmentAmount = 1000,
                                NumberOfInstalments = 12,
                                Periods = new List<EarningPeriod>
                                {
                                    new EarningPeriod
                                    {
                                        Amount = 1000,
                                        DeliveryPeriod = 1,
                                        EarningType = Messages.External.EarningType.Learning,
                                        Employer = new Employer
                                        {
                                            AccountId = 12345,
                                            EmployerType = Messages.External.EmployerType.Levy,
                                            FundingAccountId = 12345
                                        },
                                        LearningId = 123
                                    },
                                    new EarningPeriod
                                    {
                                        Amount = 1000,
                                        DeliveryPeriod = 2,
                                        EarningType = Messages.External.EarningType.Learning,
                                        Employer = new Employer
                                        {
                                            AccountId = 12345,
                                            EmployerType = Messages.External.EmployerType.Levy,
                                            FundingAccountId = 12345
                                        },
                                        LearningId = 123
                                    },
                                    new EarningPeriod
                                    {
                                        Amount = 1000,
                                        DeliveryPeriod = 3,
                                        EarningType = Messages.External.EarningType.Learning,
                                        Employer = new Employer
                                        {
                                            AccountId = 12345,
                                            EmployerType = Messages.External.EmployerType.Levy,
                                            FundingAccountId = 12345
                                        },
                                        LearningId = 123
                                    },
                                    new EarningPeriod
                                    {
                                        Amount = 1000,
                                        DeliveryPeriod = 4,
                                        EarningType = Messages.External.EarningType.Learning,
                                        Employer = new Employer
                                        {
                                            AccountId = 12345,
                                            EmployerType = Messages.External.EmployerType.Levy,
                                            FundingAccountId = 12345
                                        },
                                        LearningId = 123
                                    },
                                    new EarningPeriod
                                    {
                                        Amount = 1000,
                                        DeliveryPeriod = 5,
                                        EarningType = Messages.External.EarningType.Learning,
                                        Employer = new Employer
                                        {
                                            AccountId = 12345,
                                            EmployerType = Messages.External.EmployerType.Levy,
                                            FundingAccountId = 12345
                                        },
                                        LearningId = 123
                                    },
                                    new EarningPeriod
                                    {
                                        Amount = 1000,
                                        DeliveryPeriod = 6,
                                        EarningType = Messages.External.EarningType.Learning,
                                        Employer = new Employer
                                        {
                                            AccountId = 12345,
                                            EmployerType = Messages.External.EmployerType.Levy,
                                            FundingAccountId = 12345
                                        },
                                        LearningId = 123
                                    },
                                    new EarningPeriod
                                    {
                                        Amount = 1000,
                                        DeliveryPeriod = 7,
                                        EarningType = Messages.External.EarningType.Learning,
                                        Employer = new Employer
                                        {
                                            AccountId = 12345,
                                            EmployerType = Messages.External.EmployerType.Levy,
                                            FundingAccountId = 12345
                                        },
                                        LearningId = 123
                                    },
                                    new EarningPeriod
                                    {
                                        Amount = 1000,
                                        DeliveryPeriod = 8,
                                        EarningType = Messages.External.EarningType.Learning,
                                        Employer = new Employer
                                        {
                                            AccountId = 12345,
                                            EmployerType = Messages.External.EmployerType.Levy,
                                            FundingAccountId = 12345
                                        },
                                        LearningId = 123
                                    },
                                    new EarningPeriod
                                    {
                                        Amount = 1000,
                                        DeliveryPeriod = 9,
                                        EarningType = Messages.External.EarningType.Learning,
                                        Employer = new Employer
                                        {
                                            AccountId = 12345,
                                            EmployerType = Messages.External.EmployerType.Levy,
                                            FundingAccountId = 12345
                                        },
                                        LearningId = 123
                                    },
                                    new EarningPeriod
                                    {
                                        Amount = 1000,
                                        DeliveryPeriod = 10,
                                        EarningType = Messages.External.EarningType.Learning,
                                        Employer = new Employer
                                        {
                                            AccountId = 12345,
                                            EmployerType = Messages.External.EmployerType.Levy,
                                            FundingAccountId = 12345
                                        },
                                        LearningId = 123
                                    },
                                    new EarningPeriod
                                    {
                                        Amount = 1000,
                                        DeliveryPeriod = 11,
                                        EarningType = Messages.External.EarningType.Learning,
                                        Employer = new Employer
                                        {
                                            AccountId = 12345,
                                            EmployerType = Messages.External.EmployerType.Levy,
                                            FundingAccountId = 12345
                                        },
                                        LearningId = 123
                                    },
                                    new EarningPeriod
                                    {
                                        Amount = 1000,
                                        DeliveryPeriod = 12,
                                        EarningType = Messages.External.EarningType.Learning,
                                        Employer = new Employer
                                        {
                                            AccountId = 12345,
                                            EmployerType = Messages.External.EmployerType.Levy,
                                            FundingAccountId = 12345
                                        },
                                        LearningId = 123
                                    },

                                }

                            }
                        }

                    }
                }
            };
        }

        [Test]
        public async Task ProcessEarnings_Validates_ReceivedEarnings()
        {
            // Arrange
            // Act
            await processor.ProcessEarnings(earningsCommand);
            // Assert
            mocker.GetMock<ICalculateGSLPaymentsValidator>().Verify(x => x.Validate(It.IsAny<CalculateGrowthAndSkillsPayments>()), Times.Once);
        }

        [Test]
        public async Task ProcessEarnings_Saves_ReceivedEarnings() 
        {
            await processor.ProcessEarnings(earningsCommand);
            // Assert
            mocker.GetMock<IEarningsRepository>().Verify(x => x.SaveEarnings(It.IsAny<ReceivedEarning>()), Times.Once);
        }
    }
}
