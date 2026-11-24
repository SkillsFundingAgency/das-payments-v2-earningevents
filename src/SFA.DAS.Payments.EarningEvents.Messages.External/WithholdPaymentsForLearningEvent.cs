using CourseType = SFA.DAS.Payments.EarningEvents.Messages.External.CourseType;
using LearningType = SFA.DAS.Payments.EarningEvents.Messages.External.LearningType;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Types
{
    public class BaseEvent
    {
    }

    public class WithholdPaymentsForLearningEvent : BaseEvent
    {
        public string CourseCode { get; set; } = string.Empty;
        public CourseType CourseType { get; set; }
        public Guid LearningKey { get; set; }
        public string LearningAimReference { get; set; } = string.Empty;
        public LearningType LearningType { get; set; }
        public long UKPRN { get; set; }
        public long ULN { get; set; }
    }
}