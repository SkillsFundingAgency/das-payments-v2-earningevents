using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Handlers;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Validators;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Model;
using EarningType = SFA.DAS.Payments.EarningEvents.Messages.External.EarningType;
using EmployerType = SFA.DAS.Payments.EarningEvents.Messages.External.EmployerType;
using LearningType = SFA.DAS.Payments.EarningEvents.Messages.External.LearningType;
using TrainingStatus = SFA.DAS.Payments.EarningEvents.Messages.External.TrainingStatus;
// ReSharper disable InconsistentNaming

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.UnitTests
{
    public class GSLCalculatePaymentsHandlerTests
    {
        private CalculateGrowthAndSkillsPayments _message;

        private CalculateGSLPaymentsValidator _validator;
        private GrowthAndSkillsMapper _mapper;
        private Mock<IEarningsRepository> _repository;
        private Mock<IPaymentsServiceBusPublisher> _publisher;
        private Mock<ILogger<GSLCalculatePaymentsHandler>> _logger;
        
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
                    CourseCode = "123456",
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
                    LearnerKey = Guid.NewGuid()
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

        _validator = new CalculateGSLPaymentsValidator();
        _mapper = new GrowthAndSkillsMapper();
        _repository = new Mock<IEarningsRepository>();
        _publisher = new Mock<IPaymentsServiceBusPublisher>();
        _logger = new Mock<ILogger<GSLCalculatePaymentsHandler>>();

        }
        
        [Test]
        public async Task Earnings_are_not_processed_if_validation_fails()
        {
            // Arrange
            _message.UKPRN = 0;
            var handler = new GSLCalculatePaymentsHandler(_validator, _mapper, _repository.Object, _publisher.Object, _logger.Object);

            // Act 
            Func<Task> act = async () => await handler.HandleGslCalculatePaymentsMessage(_message);
            act.Should().Throw<ArgumentException>()
                .WithMessage("UKPRN is required");

            // Assert
            _logger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<ArgumentException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _publisher.Verify(p => p.Publish<GSLShortCourseEarningsEvent>(It.IsAny<GSLShortCourseEarningsEvent>()),
                Times.Never);
            _publisher.Verify(p => p.Publish<DasEarningsReceivedEvent>(It.IsAny<DasEarningsReceivedEvent>()),
                Times.Never);
            _repository.Verify(r => r.SaveEarnings(It.IsAny<GrowthAndSkillsEarningModel>()), Times.Never);
        }

        [Test]
        public async Task Earnings_are_sent_to_service_bus_and_stored_to_database_cache()
        {
            // Arrange          
            var handler = new GSLCalculatePaymentsHandler(_validator, _mapper, _repository.Object, _publisher.Object, _logger.Object);

            // Act
            await handler.HandleGslCalculatePaymentsMessage(_message);

            // Assert
            _publisher.Verify(p => p.Publish<GSLShortCourseEarningsEvent>(It.IsAny<GSLShortCourseEarningsEvent>()),
                Times.Once);
            _publisher.Verify(p => p.Publish<DasEarningsReceivedEvent>(It.IsAny<DasEarningsReceivedEvent>()),
                Times.Once);
            _repository.Verify(r => r.SaveEarnings(It.IsAny<GrowthAndSkillsEarningModel>()), Times.Once);
        }
    }
}
