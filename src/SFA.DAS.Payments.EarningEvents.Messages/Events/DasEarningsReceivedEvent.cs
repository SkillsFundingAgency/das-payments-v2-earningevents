using System;
using SFA.DAS.Payments.Model.Core;

namespace SFA.DAS.Payments.EarningEvents.Messages.Events
{
    public class DasEarningsReceivedEvent
    {
        public Guid EarningsId { get; set; }
        public string CourseCode { get; set; }
        public CollectionPeriod CollectionPeriod { get; set; }
        public short AcademicYear { get; set; }
        public long ULN { get; set; }
        public long UKPRN { get; set; }
        public string LearningAimReference { get; set; }
    }
}