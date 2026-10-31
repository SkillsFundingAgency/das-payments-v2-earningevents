namespace SFA.DAS.Payments.EarningEvents.Model
{
    public class ReceivedEarning
    {
        public long Id { get; set; }
        public Guid EarningsId { get; set; }
        public long UKPRN { get; set; }
        public long LearnerUln { get; set; }
        public string LearnerReference { get; set; } = string.Empty;
        public Guid LearnerKey { get; set; }
        public CourseType CourseType { get; set; }
        public LearningType LearningType { get; set; }
        public Guid LearningKey { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseReference { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public string Payload { get; set; } = string.Empty;
        public List<ReceivedEarningProcessedCollectionPeriod> ProcessedCollectionPeriods { get; set; } = new List<ReceivedEarningProcessedCollectionPeriod>();
    }
}
