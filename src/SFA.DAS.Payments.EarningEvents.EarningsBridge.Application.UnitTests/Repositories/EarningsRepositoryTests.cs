using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Payments.EarningEvents.Data;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories;
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

            _mockLogger = new Mock<ILogger<EarningsRepository>>();
            _repository = new EarningsRepository(_dataContext, _mockLogger.Object);

        }

        [TearDown]
        public void TearDown()
        {
            _dataContext.Dispose();
        }

        [Test]
        public async Task Earnings_Received_With_Empty_Table_Returns_True()
        {
            // Act
            var result = await _repository.CheckEarningsAreLatest(_message);
            // Assert
            result.Should().Be(true);
        }

        [Test]
        public async Task Earnings_Received_With_Older_Entries_In_Table_Returns_True()
        {
            // Arrange
            var timestamp = DateTime.Now;
            var oldGuid = UuidToolkit.CreateUuidV7FromSpecificDate(timestamp.AddDays(-1));
            var currentGuid = UuidToolkit.CreateUuidV7FromSpecificDate(timestamp);

            var oldEarning = CreateGrowthAndSkills(oldGuid);
            _dataContext.GrowthAndSkillsEarnings.Add(oldEarning);
            await _dataContext.SaveChangesAsync();

            _message.EarningsId = currentGuid;

            // Act
            var result = await _repository.CheckEarningsAreLatest(_message);
            // Assert
            result.Should().Be(true);
        }

        private GrowthAndSkillsEarningModel CreateGrowthAndSkills(Guid dateTimeGuid)
        {
            return new GrowthAndSkillsEarningModel
            {
                Id = 1,
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
                        Id = 1,
                        StartDate = new DateTime(2026, 1, 1),
                        EndDate = new DateTime(2026, 1, 31),
                        Price = 5000m,
                    }
                }
            };
        }
    }
}
