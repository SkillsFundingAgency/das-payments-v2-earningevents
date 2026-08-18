using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Payments.EarningEvents.Model;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Processors
{
    public class GslProcessorFactory : IGslProcessorFactory
    {
        private IServiceProvider _serviceProvider;

        public GslProcessorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IGslProcessor CreateGslProcessor(LearningType learningType)
        {
            switch (learningType)
            {
                case LearningType.Apprenticeship:
                    return _serviceProvider.GetRequiredService<GslApprenticeshipPaymentsProcessor>();
                case LearningType.ApprenticeshipUnit:
                    return _serviceProvider.GetRequiredService<GslShortCoursePaymentsProcessor>();
                default:
                    // Route unsupported learning types to a pass-through processor so message handling can continue safely
                    return _serviceProvider.GetRequiredService<UnsupportedLearningTypeProcessor>();
            }
        }
    }
}
