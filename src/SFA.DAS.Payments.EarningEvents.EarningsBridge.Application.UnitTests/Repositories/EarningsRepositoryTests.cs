using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Payments.EarningEvents.Data;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Model;
using EarningType = SFA.DAS.Payments.EarningEvents.Messages.External.EarningType;
using EmployerType = SFA.DAS.Payments.EarningEvents.Messages.External.EmployerType;
using LearningType = SFA.DAS.Payments.EarningEvents.Messages.External.LearningType;
using TrainingStatus = SFA.DAS.Payments.EarningEvents.Messages.External.TrainingStatus;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.UnitTests.Repositories
{
    public class EarningsRepositoryTests
    {
        private IEarningsRepository _repository;
        private CalculateGrowthAndSkillsPayments _message;
        private Mock<ILogger<EarningsRepository>> _mockLogger;
        private List<GrowthAndSkillsEarningModel> _growthAndSkillsEarnings;
        private EarningsDataContext _dataContext;
        private DbContextOptions<EarningsDataContext> _dbContextOptions;

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

            _growthAndSkillsEarnings = new List<GrowthAndSkillsEarningModel>
            {
                new ()
                {
                    //Copilot please fill in the fields
                    Id = 1,
                    EarningsId = Guid.NewGuid(),
                    UKPRN = 10002233,
                    LearnerKey = Guid.NewGuid(),
                    LearnerUln = 12345678,
                    LearnerReference = "LEARNREF001",
                    LearningType = Model.LearningType.ApprenticeshipUnit,
                    CourseCode = "123456",
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
                }
            };

            //Set up in-memory database for testing
            _dbContextOptions = new DbContextOptionsBuilder<EarningsDataContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryDatabase")
                .Options;

            using (_dataContext = new EarningsDataContext(_dbContextOptions))
            {

                _dataContext.GrowthAndSkillsEarnings.AddRange(_growthAndSkillsEarnings);
                _dataContext.SaveChanges();
            }
            _mockLogger = new Mock<ILogger<EarningsRepository>>();
            _repository = new EarningsRepository(_dataContext, _mockLogger.Object);


        }

        [Test]
        public async Task Earnings_Received_With_Empty_Table_Returns_True()
        {
            // Arrange

            // Act
            var result = await _repository.CheckEarningsAreLatest(_message);
            // Assert
            result.Should().Be(true);


        }
    }
}
