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
                    throw new NotSupportedException($"Unsupported learning type: {learningType}");
            }
        }
    }
}
