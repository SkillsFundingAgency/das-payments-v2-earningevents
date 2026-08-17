using SFA.DAS.Payments.EarningEvents.Model;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Processors
{
    public interface IGslProcessorFactory
    {
        IGslProcessor CreateGslProcessor(LearningType learningType);
    }
}
