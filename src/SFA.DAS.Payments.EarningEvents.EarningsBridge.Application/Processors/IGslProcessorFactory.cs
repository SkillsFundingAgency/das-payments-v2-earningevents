using SFA.DAS.Payments.EarningEvents.Model;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Processors
{
    public interface IGSLProcessorFactory
    {
        IGSLProcessor CreateGSLProcessor(CourseType courseType);
    }
}
