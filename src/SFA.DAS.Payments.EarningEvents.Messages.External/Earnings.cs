
namespace SFA.DAS.Payments.EarningEvents.Messages.External
{
    public class Earnings
    {
        public int AcademicYear { get; set; }
        public IEnumerable<PricePeriod> PricePeriods { get; set; }
    }
}
