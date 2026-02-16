namespace SFA.DAS.Payments.EarningEvents.Model
{
    public class GrowthAndSkillsEarningModel
    {
        public long Id { get; set; }
        public Guid EarningsId { get; set; }
        public long UKPRN { get; set; } 
        public Guid LearnerId { get; set; }
        public long LearnerUln { get; set; }
        public string LearnerReference { get; set; } 
        public LearningType LearningType { get; set; }
        public string CourseCode { get; set; }
        public DateTime StartDate { get; set; }
        public byte AgeAtStartOfTraining { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public TrainingStatus TrainingStatus { get; set; }
        public decimal EmployerContribution { get; set; }
        public CourseType CourseType { get; set; }

        public List<GrowthAndSkillsEarningPricePeriodModel> PricePeriods { get; set; } = new List<GrowthAndSkillsEarningPricePeriodModel>();
    }
}
