namespace SFA.DAS.Payments.EarningEvents.Model
{
    public class ShortCourseEarningModel
    {
        public long Id { get; set; }
        public Guid EarningsId { get; set; }
        public long Ukprn { get; set; }
        public long LearnerId { get; set; }
        public long LearnerUln { get; set; }
        public string LearnerReference { get; set; }
        public TrainingType TrainingType { get; set; }
        public int CourseCode { get; set; }
        public DateTime StartDate { get; set; }
        public byte AgeAtStartOfTraining { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public TrainingStatus TrainingStatus { get; set; }
        public decimal EmployerContribution { get; set; }

        public List<ShortCourseEarningPricePeriodModel> PricePeriods { get; set; } = new List<ShortCourseEarningPricePeriodModel>();
    }
}
