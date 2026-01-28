
namespace SFA.DAS.Payments.EarningEvents.Messages.External
{
    public class PricePeriod
    {
        public decimal Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public IEnumerable<EarningPeriod> Periods { get; set; }
    }
}
