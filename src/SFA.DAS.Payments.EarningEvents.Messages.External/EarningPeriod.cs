
namespace SFA.DAS.Payments.EarningEvents.Messages.External
{
    public class EarningPeriod
    {
        public byte DeliveryPeriod { get; set; }
        public EarningType EarningType { get; set; }
        public decimal Amount { get; set; }
        public Employer Employer { get; set; }
    }
}
