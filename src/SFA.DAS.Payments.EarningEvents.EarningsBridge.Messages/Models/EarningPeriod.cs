namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Messages.Models;

public class EarningPeriod
{
    public int DeliveryPeriod { get; set; }
    public EarningType EarningType { get; set; }
    //ideally should be bigdecimal? 
        
    //Look up precision data annotation[]
    public decimal Amount { get; set; }
    public Employer Employer { get; set; }
}

public enum EarningType{
    Learning,
    Completion,
    Balancing,
    First16To18ProviderIncentive,
    Second16To18EmployerIncentive,
    Second16To18ProviderIncentive,
    OnProgramme16To18FrameworkUplift,
    Completion16To18FrameworkUplift,
    Balancing16To18FrameworkUplift,
    FirstDisadvantagePayment,
    SecondDisadvantagePayment,
    OnProgrammeMathsAndEnglish,
    BalancingMathsAndEnglish,
    LearningSupport,
    Milestone1
}