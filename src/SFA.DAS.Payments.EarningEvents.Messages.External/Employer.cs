
namespace SFA.DAS.Payments.EarningEvents.Messages.External
{
    public class Employer
    {
        public long AccountId { get; set; }
        public EmployerType EmployerType { get; set; }
        public long FundingAccountId { get; set; }
    }
}
