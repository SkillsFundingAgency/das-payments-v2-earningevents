
namespace SFA.DAS.Payments.EarningEvents.Messages.External
{
    public class Training
    {
        public CourseType CourseType { get; set; }
        public LearningType LearningType { get; set; }
        public string CourseCode { get; set; }
        public string CourseReference { get; set; }
        public DateTime StartDate { get; set; }
        public byte AgeAtStartOfTraining { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public TrainingStatus TrainingStatus { get; set; }
    }
}
