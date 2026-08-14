using SFA.DAS.Payments.EarningEvents.Model;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Processors
{
    public class GslProcessorFactory : IGslProcessorFactory
    {
        private GslApprenticeshipPaymentsProcessor _apprenticeshipPaymentsProcessor;
        private GslShortCoursePaymentsProcessor _shortCoursePaymentsProcessor;

        public GslProcessorFactory(
            GslApprenticeshipPaymentsProcessor apprenticeshipPaymentsProcessor,
            GslShortCoursePaymentsProcessor shortCoursePaymentsProcessor)
        {
            _apprenticeshipPaymentsProcessor = apprenticeshipPaymentsProcessor;
            _shortCoursePaymentsProcessor = shortCoursePaymentsProcessor;
        }

        public IGslProcessor CreateGslProcessor(LearningType learningType)
        {
            switch (learningType)
            {
                case LearningType.Apprenticeship:
                    return _apprenticeshipPaymentsProcessor;
                case LearningType.ApprenticeshipUnit:
                    return _shortCoursePaymentsProcessor;
                default:
                    throw new NotSupportedException($"Unsupported learning type: {learningType}");
            }
        }
    }
}
