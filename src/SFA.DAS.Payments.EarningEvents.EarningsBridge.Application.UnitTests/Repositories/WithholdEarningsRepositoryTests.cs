using System.Diagnostics;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Payments.EarningEvents.Data;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.Model;
using ExternalCourseType = SFA.DAS.Payments.EarningEvents.Messages.External.CourseType;
using ExternalLearningType = SFA.DAS.Payments.EarningEvents.Messages.External.LearningType;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.UnitTests.Repositories;

[TestFixture]
public class WithholdEarningsRepositoryTests
{
    private EarningsDataContext _dataContext;
    private IEarningsRepository _repository;
    private WithholdPaymentsForLearningEvent _message;
    private Mock<ILogger<EarningsRepository>> _mockLogger;
    private DbContextOptions<EarningsDataContext> _dbContextOptions;

    [SetUp]
    public void SetUp()
    {
        _dbContextOptions = new DbContextOptionsBuilder<EarningsDataContext>()
            .UseInMemoryDatabase(databaseName: "WithholdEarningsInMemoryDatabase")
            .Options;
        _dataContext = new EarningsDataContext(_dbContextOptions);
        _mockLogger = new Mock<ILogger<EarningsRepository>>();
        _repository = new EarningsRepository(_dataContext, _mockLogger.Object);
        _message = new WithholdPaymentsForLearningEvent
        {
            LearningKey = Guid.NewGuid(),
            UKPRN = 10001234,
            ULN = 1234567890,
            CourseCode = "123",
            LearningAimReference = "ZPROG001",
            CourseType = ExternalCourseType.Apprenticeship,
            LearningType = ExternalLearningType.Apprenticeship
        };
    }

    [TearDown]
    public void TearDown()
    {
        _dataContext.Database.EnsureDeleted();
        _dataContext.Dispose();
    }

    [Test]
    public async Task InvalidateEarnings_OnlyInvalidatesMatchingEarnings()
    {
        // Arrange
        var firstMatchedEarning = CreateEarning();
        var secondMatchedEarning = CreateEarning();

        var earningWithDifferentLearningKey = CreateEarning();
        earningWithDifferentLearningKey.LearningKey = Guid.NewGuid();
        var earningWithDifferentUkprn = CreateEarning();
        earningWithDifferentUkprn.UKPRN++;
        var earningWithDifferentUln = CreateEarning();
        earningWithDifferentUln.LearnerUln++;
        var earningWithDifferentCourseCode = CreateEarning();
        earningWithDifferentCourseCode.CourseCode = "234";
        var earningWithDifferentCourseType = CreateEarning();
        earningWithDifferentCourseType.CourseType = CourseType.ShortCourse;
        var earningWithDifferentLearningType = CreateEarning();
        earningWithDifferentLearningType.LearningType = LearningType.MathsAndEnglish;

        _dataContext.GrowthAndSkillsEarnings.AddRange(firstMatchedEarning, secondMatchedEarning, earningWithDifferentLearningKey, earningWithDifferentUkprn,
            earningWithDifferentUln, earningWithDifferentCourseCode, earningWithDifferentCourseType, earningWithDifferentLearningType);
        await _dataContext.SaveChangesAsync();

        // Act
        await _repository.InvalidateEarnings(
            _message.LearningKey,
            _message.UKPRN,
            _message.ULN,
            _message.CourseCode,
            (CourseType)_message.CourseType,
            (LearningType)_message.LearningType);

        // Assert
        var storedEarnings = await _dataContext.GrowthAndSkillsEarnings.AsNoTracking().ToListAsync();
        storedEarnings.Where(x => x.IsInvalidated == true).Select(x => x.EarningsId)
            .Should().BeEquivalentTo(firstMatchedEarning.EarningsId, secondMatchedEarning.EarningsId);
    }

    [Test]
    public async Task GetUnprocessedEarnings_ExcludesInvalidatedEarningsAndIncludesNewApprovedEarnings()
    {
        // Arrange
        var originalEarning = CreateEarning();
        var explicitlyValidEarning = CreateEarning();
        explicitlyValidEarning.LearningKey = Guid.NewGuid();
        explicitlyValidEarning.IsInvalidated = false;
        _dataContext.GrowthAndSkillsEarnings.AddRange(originalEarning, explicitlyValidEarning);
        await _dataContext.SaveChangesAsync();
        await _repository.InvalidateEarnings(
            _message.LearningKey,
            _message.UKPRN,
            _message.ULN,
            _message.CourseCode,
            (CourseType)_message.CourseType,
            (LearningType)_message.LearningType);

        var newApprovedEarning = CreateEarning();
        _dataContext.GrowthAndSkillsEarnings.Add(newApprovedEarning);
        await _dataContext.SaveChangesAsync();

        // Act
        var pendingEarnings = await _repository.GetUnprocessedEarningsForCollectionPeriod(2526, 1);

        // Assert
        originalEarning.IsInvalidated.Should().BeTrue();
        newApprovedEarning.IsInvalidated.Should().BeNull();
        explicitlyValidEarning.IsInvalidated.Should().BeFalse();
        pendingEarnings.Should().HaveCount(2);
        pendingEarnings.Should().Contain(x => x.EarningsId == newApprovedEarning.EarningsId);
        pendingEarnings.Should().Contain(x => x.EarningsId == explicitlyValidEarning.EarningsId);
        pendingEarnings.Should().NotContain(x => x.EarningsId == originalEarning.EarningsId);
    }

    private GrowthAndSkillsEarningModel CreateEarning()
    {
        return new GrowthAndSkillsEarningModel
        {
            EarningsId = Guid.NewGuid(),
            LearningKey = _message.LearningKey,
            UKPRN = _message.UKPRN,
            LearnerUln = _message.ULN,
            CourseCode = _message.CourseCode,
            CourseType = CourseType.Apprenticeship,
            LearningType = LearningType.Apprenticeship,
            CourseReference = "ZPROG001",
            LearnerReference = "learner",
            PricePeriods = new List<GrowthAndSkillsEarningPricePeriodModel> { new() { AcademicYear = 2526 } }
        };
    }
}
