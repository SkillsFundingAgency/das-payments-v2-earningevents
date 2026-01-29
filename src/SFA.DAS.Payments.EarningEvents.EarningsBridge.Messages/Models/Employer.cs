namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Messages.Models;

public class Employer
{
    public long AccountId { get; set; }
    public EmployerType EmployerType { get; set; }
    public long FundingAccountId { get; set; }
}

public enum EmployerType{
    Levy,
    NonLevy //it needs to be decided if this is going to have a hyphen or not 
}