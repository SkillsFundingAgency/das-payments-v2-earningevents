using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Handlers;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Mapping;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Processors;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Validators;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Model;
using SFA.DAS.Payments.Model.Core.Entities;
using UUIDNext;
using UUIDNext.Tools;
using EarningType = SFA.DAS.Payments.EarningEvents.Messages.External.EarningType;
using EmployerType = SFA.DAS.Payments.EarningEvents.Messages.External.EmployerType;
using LearningType = SFA.DAS.Payments.EarningEvents.Messages.External.LearningType;
using TrainingStatus = SFA.DAS.Payments.EarningEvents.Messages.External.TrainingStatus;
// ReSharper disable InconsistentNaming

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.UnitTests
{
    public class GslCalculatePaymentsHandlerTests
    {
        private CalculateGrowthAndSkillsPayments _message;

        private CalculateGSLPaymentsValidator _validator;
        private GrowthAndSkillsMapper _mapper;
        private Mock<IGslProcessor> _gslProcessor;
        private Mock<IEarningsRepository> _repository;
        private Mock<IGSLEarningsService> _gslService;
        private Mock<ICollectionPeriodService> _collectionPeriodService;
        private Mock<IGslProcessorFactory> _processorFactory;
        private Mock<ILogger<GslCalculatePaymentsHandler>> _logger;
        
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
            _gslProcessor = new Mock<IGslProcessor>();
            _collectionPeriodService = new Mock<ICollectionPeriodService>();
            _processorFactory = new Mock<IGslProcessorFactory>();
            _logger = new Mock<ILogger<GslCalculatePaymentsHandler>>();
            _gslService = new Mock<IGSLEarningsService>();
            _processorFactory.Setup(x => x.CreateGslProcessor(It.IsAny<SFA.DAS.Payments.EarningEvents.Model.LearningType>()))
                .Returns(_gslProcessor.Object);

            _repository.Setup(x => x.GetGrowthAndSkillsEarnings(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(new List<GrowthAndSkillsEarningModel>());
            _gslService.Setup(x => x.CheckEarningsAreLatest(It.IsAny<List<GrowthAndSkillsEarningModel>>(), It.IsAny<Guid>())).Returns(true);
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
            var handler = new GslCalculatePaymentsHandler(_validator, _mapper,_repository.Object, _gslService.Object, 
                                                          _collectionPeriodService.Object, _processorFactory.Object, _logger.Object);
            
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
            _processorFactory.Verify(x => x.CreateGslProcessor(It.IsAny<SFA.DAS.Payments.EarningEvents.Model.LearningType>()), Times.Never);
            _gslProcessor.Verify(x => x.Process(It.IsAny<CalculateGrowthAndSkillsPayments>(), It.IsAny<IEnumerable<CollectionPeriodModel>>()), Times.Never);
            _repository.Verify(r => r.SaveEarnings(It.IsAny<GrowthAndSkillsEarningModel>()), Times.Never);
        }

        [Test]
        public async Task Earnings_with_open_collection_periods_are_processed_and_stored_to_database_cache()
        {
            // Arrange          
            IEnumerable<CollectionPeriodModel> passedCollectionPeriods = null;
            _gslProcessor
                .Setup(x => x.Process(It.IsAny<CalculateGrowthAndSkillsPayments>(), It.IsAny<IEnumerable<CollectionPeriodModel>>()))
                .Callback<CalculateGrowthAndSkillsPayments, IEnumerable<CollectionPeriodModel>>((_, collectionPeriods) =>
                {
                    passedCollectionPeriods = collectionPeriods;
                })
                .Returns(Task.CompletedTask);

            var handler = new GslCalculatePaymentsHandler(_validator, _mapper,_repository.Object, _gslService.Object, 
                                                          _collectionPeriodService.Object, _processorFactory.Object, _logger.Object);

            // Act
            await handler.HandleGslCalculatePaymentsMessage(_message);

            // Assert
            _collectionPeriodService.Verify(x => x.GetOpenCollectionPeriods(), Times.Once);
            _processorFactory.Verify(x => x.CreateGslProcessor(SFA.DAS.Payments.EarningEvents.Model.LearningType.ApprenticeshipUnit), Times.Once);
            _gslProcessor.Verify(x => x.Process(_message, It.IsAny<IEnumerable<CollectionPeriodModel>>()), Times.Once);
            passedCollectionPeriods.Should().BeEquivalentTo(new[]
            {
                new CollectionPeriodModel
                {
                    AcademicYear = 2526,
                    Period = 2,
                    Status = CollectionPeriodStatus.Open
                }
            });
            _repository.Verify(r => r.SaveEarnings(It.Is<GrowthAndSkillsEarningModel>(
                y => y.PricePeriods.All(p => p.ProcessedOn != null))), Times.Once);
        }
        
        [Test]
        public async Task Earnings_are_not_processed_if_collection_period_is_not_open()
        {
            // Arrange
            _message.Earnings.ToList()[0].AcademicYear = 2425;
            var collectionPeriods = new List<CollectionPeriodModel>();
            _collectionPeriodService.Setup(x => x.GetOpenCollectionPeriods()).ReturnsAsync(collectionPeriods);
            var handler = new GslCalculatePaymentsHandler(_validator, _mapper, _repository.Object, _gslService.Object,
                _collectionPeriodService.Object, _processorFactory.Object, _logger.Object);

            // Act
            await handler.HandleGslCalculatePaymentsMessage(_message);

            // Assert
            _collectionPeriodService.Verify(x => x.GetOpenCollectionPeriods(), Times.Once);
            _repository.Verify(r => r.SaveEarnings(It.Is<GrowthAndSkillsEarningModel>(
                y => y.PricePeriods.All(p => p.ProcessedOn == null))), Times.Once);
            _processorFactory.Verify(x => x.CreateGslProcessor(It.IsAny<SFA.DAS.Payments.EarningEvents.Model.LearningType>()), Times.Never);
            _gslProcessor.Verify(x => x.Process(It.IsAny<CalculateGrowthAndSkillsPayments>(), It.IsAny<IEnumerable<CollectionPeriodModel>>()), Times.Never);
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

            var handler = new GslCalculatePaymentsHandler(_validator, _mapper, _repository.Object, _gslService.Object,
                _collectionPeriodService.Object, _processorFactory.Object, _logger.Object);

            // Act
            await handler.HandleGslCalculatePaymentsMessage(_message);

            // Assert
            _collectionPeriodService.Verify(x => x.GetOpenCollectionPeriods(), Times.Once);
            _repository.Verify(r => r.SaveEarnings(It.Is<GrowthAndSkillsEarningModel>(
                y => y.PricePeriods.Where(x => x.AcademicYear == 2425).
                    All(p => p.ProcessedOn == null))), Times.Once);
            _repository.Verify(r => r.SaveEarnings(It.Is<GrowthAndSkillsEarningModel>(
                y => y.PricePeriods.Where(x => x.AcademicYear == 2526)
                    .All(p => p.ProcessedOn != null))), Times.Once);
            _processorFactory.Verify(x => x.CreateGslProcessor(It.IsAny<SFA.DAS.Payments.EarningEvents.Model.LearningType>()), Times.Once);
            _gslProcessor.Verify(x => x.Process(It.IsAny<CalculateGrowthAndSkillsPayments>(), It.IsAny<IEnumerable<CollectionPeriodModel>>()), Times.Once);
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

            var handler = new GslCalculatePaymentsHandler(_validator, _mapper, _repository.Object, _gslService.Object,
                _collectionPeriodService.Object, _processorFactory.Object, _logger.Object);
            
            // Act
            await handler.HandleGslCalculatePaymentsMessage(_message);

            // Assert
            _collectionPeriodService.Verify(x => x.GetOpenCollectionPeriods(), Times.Once);
            _processorFactory.Verify(x => x.CreateGslProcessor(It.IsAny<SFA.DAS.Payments.EarningEvents.Model.LearningType>()), Times.Once);
            _gslProcessor.Verify(x => x.Process(It.IsAny<CalculateGrowthAndSkillsPayments>(), It.IsAny<IEnumerable<CollectionPeriodModel>>()), Times.Once);
            _repository.Verify(r => r.SaveEarnings(It.Is<GrowthAndSkillsEarningModel>(
                y => y.PricePeriods.Count == 2
                    && y.PricePeriods.Any(x => x.AcademicYear == 2526 && x.ProcessedOn != null)
                    && y.PricePeriods.Any(x => x.AcademicYear == 2627 && x.ProcessedOn != null))), Times.Once);
        }

        [Test]
        public async Task Earnings_Are_Older_Than_Latest_DB_Earnings_Should_Ignore_Message()
        {
            // Arrange
            // Mocking the repository to return existing earnings with a newer EarningsId
            _repository.Setup(repo => repo.GetGrowthAndSkillsEarnings(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>()))
                       .ReturnsAsync(new List<GrowthAndSkillsEarningModel>()); // Simulate that there are existing earnings

            _gslService.Setup(service => service.CheckEarningsAreLatest(It.IsAny<List<GrowthAndSkillsEarningModel>>(), It.IsAny<Guid>()))
                       .Returns(false); // Simulate that the earnings are older than the latest in DB


            var handler = new GslCalculatePaymentsHandler(_validator, _mapper, _repository.Object, _gslService.Object,
                _collectionPeriodService.Object, _processorFactory.Object, _logger.Object);

            // Act
            await handler.HandleGslCalculatePaymentsMessage(_message);

            // Assert
            _collectionPeriodService.Verify(x => x.GetOpenCollectionPeriods(), Times.Never);
            _repository.Verify(r => r.SaveEarnings(It.IsAny<GrowthAndSkillsEarningModel>()), Times.Never);
            _processorFactory.Verify(x => x.CreateGslProcessor(It.IsAny<SFA.DAS.Payments.EarningEvents.Model.LearningType>()), Times.Never);
            _gslProcessor.Verify(x => x.Process(It.IsAny<CalculateGrowthAndSkillsPayments>(), It.IsAny<IEnumerable<CollectionPeriodModel>>()), Times.Never);
        }

        [Test]
        public async Task Earnings_Are_Latest_Should_Process_And_Save()
        {
            // Arrange
            // Mocking the repository to return that the earnings are the latest
            _repository.Setup(repo => repo.GetGrowthAndSkillsEarnings(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>()))
                       .ReturnsAsync(new List<GrowthAndSkillsEarningModel>()); // Simulate that there are existing earnings

            _gslService.Setup(service => service.CheckEarningsAreLatest(It.IsAny<List<GrowthAndSkillsEarningModel>>(), It.IsAny<Guid>()))
                       .Returns(true); // Simulate that the earnings are the latest in DB

            var handler = new GslCalculatePaymentsHandler(_validator, _mapper, _repository.Object, _gslService.Object,
                _collectionPeriodService.Object, _processorFactory.Object, _logger.Object);

            // Act
            await handler.HandleGslCalculatePaymentsMessage(_message);

            // Assert
            _collectionPeriodService.Verify(x => x.GetOpenCollectionPeriods(), Times.Once);
            _processorFactory.Verify(x => x.CreateGslProcessor(It.IsAny<SFA.DAS.Payments.EarningEvents.Model.LearningType>()), Times.Once);
            _gslProcessor.Verify(x => x.Process(It.IsAny<CalculateGrowthAndSkillsPayments>(), It.IsAny<IEnumerable<CollectionPeriodModel>>()), Times.Once);
            _repository.Verify(r => r.SaveEarnings(It.Is<GrowthAndSkillsEarningModel>(model => model.PricePeriods.Any())), Times.Once);
        }

        [Test]
        public void Exception_In_GetGrowthAndSkillsEarnings_Should_Log_Error_And_Abort()
        {
            // Arrange
            _repository.Setup(repo => repo.GetGrowthAndSkillsEarnings(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>()))
                .Throws(new Exception("Database error"));

            var handler = new GslCalculatePaymentsHandler(_validator, _mapper, _repository.Object, _gslService.Object,
                _collectionPeriodService.Object, _processorFactory.Object, _logger.Object);

            // Act
            Assert.ThrowsAsync<Exception>(async () => await handler.HandleGslCalculatePaymentsMessage(_message));

            // Assert
            _collectionPeriodService.Verify(x => x.GetOpenCollectionPeriods(), Times.Never);
            _processorFactory.Verify(x => x.CreateGslProcessor(It.IsAny<SFA.DAS.Payments.EarningEvents.Model.LearningType>()), Times.Never);
            _gslProcessor.Verify(x => x.Process(It.IsAny<CalculateGrowthAndSkillsPayments>(), It.IsAny<IEnumerable<CollectionPeriodModel>>()), Times.Never);
            _repository.Verify(r => r.SaveEarnings(It.IsAny<GrowthAndSkillsEarningModel>()), Times.Never);
            _logger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
