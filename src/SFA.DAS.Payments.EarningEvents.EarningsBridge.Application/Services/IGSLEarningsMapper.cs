using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Model;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;

public interface IGSLEarningsMapper
{
    ShortCourseEarningModel MapToShortCourseEarningModel(CalculateGrowthAndSkillsPayments source);
    GSLShortCourseEarningsEvent MapToShortCourseEarningEvent(CalculateGrowthAndSkillsPayments source, short academicYear, byte collectionPeriod);
}
