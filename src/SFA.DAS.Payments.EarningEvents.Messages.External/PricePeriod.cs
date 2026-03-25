
namespace SFA.DAS.Payments.EarningEvents.Messages.External
{
    public class PricePeriod
    {
        public decimal Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int NumberOfInstalments { get; set; }
        public decimal InstalmentAmount { get; set; }
        public decimal CompletionAmount { get; set; }
        public IEnumerable<EarningPeriod> Periods { get; set; }
    }
}
