using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Model;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;

public interface IGrowthAndSkillsMapper
{
    GrowthAndSkillsEarningModel MapToGrowthAndSkillsEarningModel(CalculateGrowthAndSkillsPayments source);    
    GSLShortCourseEarningsEvent MapToShortCourseEarningEvent(CalculateGrowthAndSkillsPayments source, short academicYear, byte collectionPeriod);

    IEnumerable<CollectionPeriodModel> MapCollectionYearToCollectionPeriodModels(CollectionYear collectionYear);

    DasEarningsReceivedEvent MapToDasEarningsReceivedEvent(CalculateGrowthAndSkillsPayments source, short academicYear, byte collectionPeriod);
}
