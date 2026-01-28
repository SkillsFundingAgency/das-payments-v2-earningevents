
namespace SFA.DAS.Payments.EarningEvents.Messages.External
{
    public class EarningPeriod
    {
        public int DeliveryPeriod { get; set; }
        public EarningType EarningType { get; set; }
        public decimal Amount { get; set; }
        public Employer Employer { get; set; }
    }
}
