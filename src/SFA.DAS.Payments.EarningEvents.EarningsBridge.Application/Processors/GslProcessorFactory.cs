using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Payments.EarningEvents.Model;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Processors
{
    public class GSLProcessorFactory : IGSLProcessorFactory
    {
        private IServiceProvider _serviceProvider;

        public GSLProcessorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IGSLProcessor CreateGSLProcessor(LearningType learningType)
        {
            switch (learningType)
            {
                case LearningType.Apprenticeship:
                    return _serviceProvider.GetRequiredService<GSLApprenticeshipPaymentsProcessor>();
                case LearningType.ApprenticeshipUnit:
                    return _serviceProvider.GetRequiredService<GSLShortCoursePaymentsProcessor>();
                default:
                    // Route unsupported learning types to a pass-through processor so message handling can continue safely
                    return _serviceProvider.GetRequiredService<UnsupportedLearningTypeProcessor>();
            }
        }
    }
}
