using SFA.DAS.Payments.EarningEvents.Model;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories;

public interface IEarningsRepository
{
    Task SaveEarnings(GrowthAndSkillsEarningModel growthAndSkillsEarningModel);
    Task <List<GrowthAndSkillsEarningModel>> GetGrowthAndSkillsEarnings(long ukPrn, long uln, string courseCode);
    Task InvalidateEarnings(Guid learningKey, long ukprn, long uln, string courseCode, CourseType courseType, LearningType learningType);
    Task<List<GrowthAndSkillsEarningModel>> GetUnprocessedEarningsForCollectionPeriod(short academicYear, byte collectionPeriod);
    Task MarkEarningProcessed(Guid earningsId, short academicYear, byte collectionPeriod, DateTime processedOn);
    Task MarkPricePeriodsProcessed(Guid earningsId, short academicYear, DateTime processedOn);
}