
namespace SFA.DAS.Payments.EarningEvents.Messages.External
{
    public class Training
    {
        public LearningType LearningType { get; set; }
        public String CourseCode { get; set; }
        public DateTime StartDate { get; set; }
        public byte AgeAtStartOfTraining { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public TrainingStatus TrainingStatus { get; set; }
    }
}
