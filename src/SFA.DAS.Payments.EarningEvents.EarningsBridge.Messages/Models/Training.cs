namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Messages.Models;

public class Training
{
    public TrainingType TrainingType { get; set; }
    public int CourseCode { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime ActualEndDate { get; set; }
    public TrainingStatus TrainingStatus { get; set; }
}

public enum TrainingType
{
    Apprenticeship,
    FoundationApprenticeship,
    FunctionalSkill,
    ApprenticeshipUnit
}

public enum TrainingStatus
{
    Continuing,
    Completed,
    Withdrawn,
    BreakInLearning
}