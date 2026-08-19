using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Mapping
{
    public interface IGSLShortCoursesMapper : IGrowthAndSkillsMapper
    {
        IEnumerable<GSLShortCourseEarningsEvent> MapToShortCourseEarningEvents(CalculateGrowthAndSkillsPayments source, IEnumerable<CollectionPeriodModel> openCollectionPeriods);

    }
}
