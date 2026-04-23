using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Payments.EarningEvents.Data;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Model;
using UUIDNext.Tools;
using EarningType = SFA.DAS.Payments.EarningEvents.Messages.External.EarningType;
using EmployerType = SFA.DAS.Payments.EarningEvents.Messages.External.EmployerType;
using LearningType = SFA.DAS.Payments.EarningEvents.Messages.External.LearningType;
using TrainingStatus = SFA.DAS.Payments.EarningEvents.Messages.External.TrainingStatus;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.UnitTests.Repositories
{
    [TestFixture]
    public class EarningsRepositoryTests
    {
        private IEarningsRepository _repository;
        private CalculateGrowthAndSkillsPayments _message;
        private Mock<ILogger<EarningsRepository>> _mockLogger;
        private EarningsDataContext _dataContext;
        private DbContextOptions<EarningsDataContext> _dbContextOptions;
        private IRepositoryService _repositoryService;

        private long _ukPrn;
        private long _uln;
        private string _courseCode;

        [SetUp]
        public void SetUp()
        {
            _ukPrn = 10001234;
            _uln = 12345678;
            _courseCode = "123456";

            _message = new CalculateGrowthAndSkillsPayments
            {
                EarningsId = Guid.NewGuid(),
                EmployerContribution = 1000m,
                UKPRN = _ukPrn,
                Training = new Training
                {
                    CourseCode = _courseCode,
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
                    ULN = _uln,
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


            //Set up in-memory database for testing
            _dbContextOptions = new DbContextOptionsBuilder<EarningsDataContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryDatabase")
                .Options;
            _dataContext = new EarningsDataContext(_dbContextOptions);
            _repositoryService = new RepositoryService();
            _mockLogger = new Mock<ILogger<EarningsRepository>>();
            _repository = new EarningsRepository(_dataContext, _repositoryService, _mockLogger.Object);

        }

        [TearDown]
        public void TearDown()
        {
            _dataContext.Database.EnsureDeleted();
            _dataContext.Dispose();
        }

        [Test]
        public async Task Earnings_Received_With_Empty_Table_Returns_True()
        {
            // Act
            var result = await _repository.CheckEarningsAreLatest(_message);
            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task Earnings_Received_With_Older_Entry_In_Table_Returns_True()
        {
            // Arrange
            var timestamp = DateTime.Now;
            var tableEarningsId = UuidToolkit.CreateUuidV7FromSpecificDate(timestamp.AddDays(-1));
            var messageEarningsId = UuidToolkit.CreateUuidV7FromSpecificDate(timestamp);

            var tablesEarning = CreateGrowthAndSkills(tableEarningsId, 1);
            _dataContext.GrowthAndSkillsEarnings.Add(tablesEarning);
            await _dataContext.SaveChangesAsync();

            _message.EarningsId = messageEarningsId;

            // Act
            var result = await _repository.CheckEarningsAreLatest(_message);
            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task Earnings_Received_With_Older_Entries_In_Table_Returns_True()
        {
            // Arrange
            var timestamp = DateTime.Now;
            var messageEarningsId = UuidToolkit.CreateUuidV7FromSpecificDate(timestamp);

            var olderTableEntries = new List<GrowthAndSkillsEarningModel>
            {
                CreateGrowthAndSkills(UuidToolkit.CreateUuidV7FromSpecificDate(timestamp.AddDays(-1)), 1),
                CreateGrowthAndSkills(UuidToolkit.CreateUuidV7FromSpecificDate(timestamp.AddDays(-2)), 2),
                CreateGrowthAndSkills(UuidToolkit.CreateUuidV7FromSpecificDate(timestamp.AddDays(-3)), 3),

            };
            _dataContext.GrowthAndSkillsEarnings.AddRange(olderTableEntries);
            await _dataContext.SaveChangesAsync();

            _message.EarningsId = messageEarningsId;

            // Act
            var result = await _repository.CheckEarningsAreLatest(_message);
            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task Earnings_Received_With_Newer_Entry_In_Table_Returns_False()
        {
            // Arrange
            var timestamp = DateTime.Now;
            var messageEarningsId = UuidToolkit.CreateUuidV7FromSpecificDate(timestamp.AddDays(-1));

            var newerTableEntries = new List<GrowthAndSkillsEarningModel>
            {
                CreateGrowthAndSkills(UuidToolkit.CreateUuidV7FromSpecificDate(timestamp.AddDays(1)), 1)

            };
            _dataContext.GrowthAndSkillsEarnings.AddRange(newerTableEntries);
            await _dataContext.SaveChangesAsync();

            _message.EarningsId = messageEarningsId;

            // Act
            var result = await _repository.CheckEarningsAreLatest(_message);
            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task Earnings_Received_With_Newer_Entries_In_Table_Returns_False()
        {
            // Arrange
            var timestamp = DateTime.Now;
            var messageEarningsId = UuidToolkit.CreateUuidV7FromSpecificDate(timestamp.AddDays(-1));

            var newerTableEntries = new List<GrowthAndSkillsEarningModel>
            {
                CreateGrowthAndSkills(UuidToolkit.CreateUuidV7FromSpecificDate(timestamp.AddDays(1)), 1),
                CreateGrowthAndSkills(UuidToolkit.CreateUuidV7FromSpecificDate(timestamp.AddDays(2)), 2),
                CreateGrowthAndSkills(UuidToolkit.CreateUuidV7FromSpecificDate(timestamp.AddDays(3)), 3),

            };
            _dataContext.GrowthAndSkillsEarnings.AddRange(newerTableEntries);
            await _dataContext.SaveChangesAsync();

            _message.EarningsId = messageEarningsId;

            // Act
            var result = await _repository.CheckEarningsAreLatest(_message);
            // Assert
            result.Should().BeFalse();
        }
        [Test]
        public async Task Earnings_Received_With_Matching_Entries_In_Table_Returns_False()
        {
            // Arrange
            var timestamp = DateTime.Now;
            var messageEarningsId = UuidToolkit.CreateUuidV7FromSpecificDate(timestamp);

            var newerTableEntries = new List<GrowthAndSkillsEarningModel>
            {
                CreateGrowthAndSkills(UuidToolkit.CreateUuidV7FromSpecificDate(timestamp), 1),

            };
            _dataContext.GrowthAndSkillsEarnings.AddRange(newerTableEntries);
            await _dataContext.SaveChangesAsync();

            _message.EarningsId = messageEarningsId;

            // Act
            var result = await _repository.CheckEarningsAreLatest(_message);
            // Assert
            result.Should().BeFalse();
        }

        private GrowthAndSkillsEarningModel CreateGrowthAndSkills(Guid dateTimeGuid, long id)
        {
            return new GrowthAndSkillsEarningModel
            {
                Id = id,
                EarningsId = dateTimeGuid,
                UKPRN = _ukPrn,
                LearnerKey = Guid.NewGuid(),
                LearnerUln = _uln,
                LearnerReference = "LEARNREF001",
                LearningType = Model.LearningType.ApprenticeshipUnit,
                CourseCode = _courseCode,
                CourseReference = "ZSC00123",
                StartDate = new DateTime(2026, 1, 1),
                AgeAtStartOfTraining = 25,
                PlannedEndDate = new DateTime(2026, 1, 15),
                ActualEndDate = new DateTime(2026, 1, 31),
                TrainingStatus = Model.TrainingStatus.Continuing,
                EmployerContribution = 1000m,
                CourseType = Model.CourseType.Apprenticeship,
                PricePeriods = new List<GrowthAndSkillsEarningPricePeriodModel>
                {
                    new GrowthAndSkillsEarningPricePeriodModel
                    {
                        Id = id,
                        StartDate = new DateTime(2026, 1, 1),
                        EndDate = new DateTime(2026, 1, 31),
                        Price = 5000m,
                    }
                }
            };
        }
    }
}
