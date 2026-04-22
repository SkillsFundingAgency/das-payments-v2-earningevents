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
using SFA.DAS.Payments.Model.Core.Entities;
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
        private Mock<ICollectionPeriodService> _collectionPeriodService;
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
            _collectionPeriodService = new Mock<ICollectionPeriodService>();
            _logger = new Mock<ILogger<GSLCalculatePaymentsHandler>>();

            var collectionPeriods = new List<CollectionPeriodModel>
            {
                new CollectionPeriodModel
                {
                    AcademicYear = 2526,
                    Period = 2,
                    Status = CollectionPeriodStatus.Open
                }
            };
            _collectionPeriodService.Setup(x => x.GetOpenCollectionPeriods()).ReturnsAsync(collectionPeriods);
        }
        
        [Test]
        public async Task Earnings_are_not_processed_if_validation_fails()
        {
            // Arrange
            _message.UKPRN = 0;
            var handler = new GSLCalculatePaymentsHandler(_validator, _mapper, _repository.Object, _publisher.Object, 
                                                          _collectionPeriodService.Object, _logger.Object);
            
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

            _collectionPeriodService.Verify(x => x.GetOpenCollectionPeriods(), Times.Never);
            _publisher.Verify(p => p.Publish<GSLShortCourseEarningsEvent>(It.IsAny<GSLShortCourseEarningsEvent>()),
                Times.Never);
            _publisher.Verify(p => p.Publish<DasEarningsReceivedEvent>(It.IsAny<DasEarningsReceivedEvent>()),
                Times.Never);
            _repository.Verify(r => r.SaveEarnings(It.IsAny<GrowthAndSkillsEarningModel>()), Times.Never);
        }

        [Test]
        public async Task Earnings_are_not_processed_if_earnings_are_null()
        {
            // Arrange
            _message.Earnings = null;
            var handler = new GSLCalculatePaymentsHandler(_validator, _mapper, _repository.Object, _publisher.Object,
                _collectionPeriodService.Object, _logger.Object);

            // Act 
            Func<Task> act = async () => await handler.HandleGslCalculatePaymentsMessage(_message);
            act.Should().Throw<ArgumentException>()
                .WithMessage("Earnings are required");

            // Assert
            _logger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<ArgumentException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _collectionPeriodService.Verify(x => x.GetOpenCollectionPeriods(), Times.Never);
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
            var handler = new GSLCalculatePaymentsHandler(_validator, _mapper, _repository.Object, _publisher.Object, 
                                                          _collectionPeriodService.Object, _logger.Object);

            // Act
            await handler.HandleGslCalculatePaymentsMessage(_message);

            // Assert
            _collectionPeriodService.Verify(x => x.GetOpenCollectionPeriods(), Times.Once);
            _publisher.Verify(p => p.Publish<GSLShortCourseEarningsEvent>(It.IsAny<GSLShortCourseEarningsEvent>()),
                Times.Once);
            _publisher.Verify(p => p.Publish<DasEarningsReceivedEvent>(It.IsAny<DasEarningsReceivedEvent>()),
                Times.Once);
            _repository.Verify(r => r.SaveEarnings(It.Is<GrowthAndSkillsEarningModel>(
                y => y.PricePeriods.All(p => p.ProcessedOn != null))), Times.Once);
        }

        [Test]
        public async Task Earnings_are_not_sent_to_service_bus_if_collection_period_not_open()
        {
            // Arrange
            _message.Earnings.ToList()[0].AcademicYear = 2425;
            var collectionPeriods = new List<CollectionPeriodModel>();
            _collectionPeriodService.Setup(x => x.GetOpenCollectionPeriods()).ReturnsAsync(collectionPeriods);
            var expectedModel = new GrowthAndSkillsMapper().MapToGrowthAndSkillsEarningModel(_message);
            
            var handler = new GSLCalculatePaymentsHandler(_validator, _mapper, _repository.Object, _publisher.Object,
                _collectionPeriodService.Object, _logger.Object);

            // Act
            await handler.HandleGslCalculatePaymentsMessage(_message);

            // Assert
            _collectionPeriodService.Verify(x => x.GetOpenCollectionPeriods(), Times.Once);
            _publisher.Verify(p => p.Publish<GSLShortCourseEarningsEvent>(It.IsAny<GSLShortCourseEarningsEvent>()),
                Times.Never);
            _publisher.Verify(p => p.Publish<DasEarningsReceivedEvent>(It.IsAny<DasEarningsReceivedEvent>()),
                Times.Never);
            _repository.Verify(r => r.SaveEarnings(It.Is<GrowthAndSkillsEarningModel>(
                y => y.PricePeriods.All(p => p.ProcessedOn == null))), Times.Once);
        }

        [Test]
        public async Task Earnings_for_the_open_collection_period_are_processed_and_others_are_cached()
        {
            // Arrange
            _message.Earnings = new List<Earnings>
            {
                new Earnings
                {
                    AcademicYear = 2425,
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
                },
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
            };

            var handler = new GSLCalculatePaymentsHandler(_validator, _mapper, _repository.Object, _publisher.Object,
                _collectionPeriodService.Object, _logger.Object);

            // Act
            await handler.HandleGslCalculatePaymentsMessage(_message);

            // Assert
            _collectionPeriodService.Verify(x => x.GetOpenCollectionPeriods(), Times.Once);
            _publisher.Verify(p => p.Publish<GSLShortCourseEarningsEvent>(It.IsAny<GSLShortCourseEarningsEvent>()),
                Times.Once);
            _publisher.Verify(p => p.Publish<DasEarningsReceivedEvent>(It.IsAny<DasEarningsReceivedEvent>()),
                Times.Once);
            _repository.Verify(r => r.SaveEarnings(It.Is<GrowthAndSkillsEarningModel>(
                y => y.PricePeriods.Where(x => x.AcademicYear == 2425).
                    All(p => p.ProcessedOn == null))), Times.Once);
            _repository.Verify(r => r.SaveEarnings(It.Is<GrowthAndSkillsEarningModel>(
                y => y.PricePeriods.Where(x => x.AcademicYear == 2526)
                    .All(p => p.ProcessedOn != null))), Times.Once);
        }

        [Test]
        public async Task Earnings_are_sent_for_both_academic_years_if_two_collection_periods_are_open()
        {
            // Arrange
            _message.Earnings = new List<Earnings>
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
                },
                new Earnings
                {
                    AcademicYear = 2627,
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
            };
            var collectionPeriods = new List<CollectionPeriodModel>
            {
                new CollectionPeriodModel
                {
                    AcademicYear = 2526,
                    Period = 13,
                    Status = CollectionPeriodStatus.Open
                },
                new CollectionPeriodModel
                {
                    AcademicYear = 2627,
                    Period = 1,
                    Status = CollectionPeriodStatus.Open
                }
            };
            _collectionPeriodService.Setup(x => x.GetOpenCollectionPeriods()).ReturnsAsync(collectionPeriods);

            var handler = new GSLCalculatePaymentsHandler(_validator, _mapper, _repository.Object, _publisher.Object,
                _collectionPeriodService.Object, _logger.Object);
            
            // Act
            await handler.HandleGslCalculatePaymentsMessage(_message);

            // Assert
            _collectionPeriodService.Verify(x => x.GetOpenCollectionPeriods(), Times.Once);
            _publisher.Verify(p => p.Publish<GSLShortCourseEarningsEvent>(It.IsAny<GSLShortCourseEarningsEvent>()),
                Times.Exactly(2));
            _publisher.Verify(p => p.Publish<DasEarningsReceivedEvent>(It.IsAny<DasEarningsReceivedEvent>()),
                Times.Exactly(2));
            _repository.Verify(r => r.SaveEarnings(It.Is<GrowthAndSkillsEarningModel>(
                y => y.PricePeriods.Where(x => x.AcademicYear == 2425).
                    All(p => p.ProcessedOn != null))), Times.Once);
            _repository.Verify(r => r.SaveEarnings(It.Is<GrowthAndSkillsEarningModel>(
                y => y.PricePeriods.Where(x => x.AcademicYear == 2526)
                    .All(p => p.ProcessedOn != null))), Times.Once);
        }
        [Test]
        public async Task Message_With_Empty_Earnings_Generates_Record_And_RequiredPayments_Message()
        {
            // Arrange          
            _message.Earnings = [];
            var handler = new GSLCalculatePaymentsHandler(_validator, _mapper, _repository.Object, _publisher.Object,
                _collectionPeriodService.Object, _logger.Object);

            // Act
            await handler.HandleGslCalculatePaymentsMessage(_message);

            // Assert
            _collectionPeriodService.Verify(x => x.GetOpenCollectionPeriods(), Times.Once);
            _publisher.Verify(p => p.Publish<GSLShortCourseEarningsEvent>(It.IsAny<GSLShortCourseEarningsEvent>()),
                Times.Once);
            //Todo: expect one invocation
            _publisher.Verify(p => p.Publish<DasEarningsReceivedEvent>(It.IsAny<DasEarningsReceivedEvent>()),
                Times.Once);
            _repository.Verify(r => r.SaveEarnings(It.Is<GrowthAndSkillsEarningModel>(x => x.PricePeriods.Count == 0)), Times.Once);
        }
        [Test]
        public async Task Rollover_Message_With_Empty_Earnings_Generates_Record_And_RequiredPayments_Message()
        {
            // Arrange          
            _message.Earnings = [];
            var collectionPeriods = new List<CollectionPeriodModel>
            {
                new CollectionPeriodModel
                {
                    AcademicYear = 2526,
                    Period = 13,
                    Status = CollectionPeriodStatus.Open
                },
                new CollectionPeriodModel
                {
                    AcademicYear = 2627,
                    Period = 1,
                    Status = CollectionPeriodStatus.Open
                }
            };
            _collectionPeriodService.Setup(x => x.GetOpenCollectionPeriods()).ReturnsAsync(collectionPeriods);

            var handler = new GSLCalculatePaymentsHandler(_validator, _mapper, _repository.Object, _publisher.Object,
                _collectionPeriodService.Object, _logger.Object);

            // Act
            await handler.HandleGslCalculatePaymentsMessage(_message);

            // Assert
            _collectionPeriodService.Verify(x => x.GetOpenCollectionPeriods(), Times.Once);
            _publisher.Verify(p => p.Publish<GSLShortCourseEarningsEvent>(It.IsAny<GSLShortCourseEarningsEvent>()),
                Times.Exactly(2));
            _publisher.Verify(p => p.Publish<DasEarningsReceivedEvent>(It.IsAny<DasEarningsReceivedEvent>()),
                Times.Exactly(2));
            _repository.Verify(r => r.SaveEarnings(It.Is<GrowthAndSkillsEarningModel>(x => x.PricePeriods.Count == 0)), Times.Once);
        }
    }
}
