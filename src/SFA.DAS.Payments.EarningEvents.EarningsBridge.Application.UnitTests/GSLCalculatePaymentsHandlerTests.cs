using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Handlers;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Validators;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Model;
using EarningType = SFA.DAS.Payments.EarningEvents.Messages.External.EarningType;
using EmployerType = SFA.DAS.Payments.EarningEvents.Messages.External.EmployerType;
using LearningType = SFA.DAS.Payments.EarningEvents.Messages.External.LearningType;
using TrainingStatus = SFA.DAS.Payments.EarningEvents.Messages.External.TrainingStatus;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.UnitTests
{
    public class GSLCalculatePaymentsHandlerTests
    {
        private CalculateGrowthAndSkillsPayments _message;

        [SetUp]
        public void SetUp()
        {
            _message = new CalculateGrowthAndSkillsPayments
            {
                EarningsId = Guid.NewGuid(),
                EmployerContribution = 1000m,
                UKPRN = 10002233,
                Training = new Training
                {
                    CourseCode = "ABC123",
                    CourseReference = "ZSC00123",
                    LearningType = LearningType.ApprenticeshipUnit,
                    StartDate = new DateTime(2026, 1, 1),
                    TrainingStatus = TrainingStatus.Continuing,
                    AgeAtStartOfTraining = 25,
                    PlannedEndDate = new DateTime(2026, 1, 15),
                    ActualEndDate = new DateTime(2026, 1, 31)
                },
                Learner = new Learner
                {
                    ULN = 12345678,
                    Reference = "LEARNREF001",
                    LearnerId = Guid.NewGuid()
                },
                Earnings = new List<Earnings>
                {
                    new Earnings
                    {
                        AcademicYear = 2526,
                        PricePeriods = new List<PricePeriod>
                        {
                            new PricePeriod
                            {
                                StartDate = new DateTime(2026, 1, 1),
                                Price = 5000m,
                                EndDate = new DateTime(2026, 1, 31),
                                CompletionAmount = 1000m,
                                InstalmentAmount = 2000m,
                                NumberOfInstalments = 2,
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
                                        Amount = 2000m,
                                        DeliveryPeriod = 1,
                                        EarningType = EarningType.Milestone1,
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
        public async Task Ensure_Validator_Exception_Caught_For_Incorrect_UKPRN()
        {
            // Arrange
            _message.UKPRN = 0;
            var validator = new CalculateGSLPaymentsValidator();
            var mapper = new GrowthAndSkillsMapper();
            var repository = new Mock<IEarningsRepository>();
            var collectionPeriodService = new Mock<ICollectionPeriodService>();
            var logger = new Mock<ILogger<GSLCalculatePaymentsHandler>>();

            var handler = new GSLCalculatePaymentsHandler(
                validator,
                mapper,
                repository.Object,
                collectionPeriodService.Object,
                logger.Object);


            // Act 
            Action act = () => handler.HandleGslCalculatePaymentsMessage(_message);
            act.Should().Throw<ArgumentException>()
                .WithMessage("UKPRN is required");

            // Assert
            logger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<ArgumentException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            repository.Verify(r => r.SaveEarnings(It.IsAny<GrowthAndSkillsEarningModel>()), Times.Never);

        }

        [Test]
        public void Ensure_Repository_Save_Is_Called_With_Mapped_Earnings()
        {
            // Arrange
            var validator = new Mock<ICalculateGSLPaymentsValidator>();
            var mapper = new GrowthAndSkillsMapper();
            var repository = new Mock<IEarningsRepository>();
            var collectionPeriodService = new Mock<ICollectionPeriodService>();
            var logger = new Mock<ILogger<GSLCalculatePaymentsHandler>>();


            var handler = new GSLCalculatePaymentsHandler(
                validator.Object,
                mapper,
                repository.Object,
                collectionPeriodService.Object,
                logger.Object);

            validator.Setup(x => x.Validate(It.IsAny<CalculateGrowthAndSkillsPayments>()))
                .Returns(true);

            // Act
            handler.HandleGslCalculatePaymentsMessage(_message);

            // Assert
            repository.Verify(r => r.SaveEarnings(It.IsAny<GrowthAndSkillsEarningModel>()), Times.Once);
        }
    }
}
