namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Messages.Models
{
    public class YearlyEarnings
    {
        public int Year { get; set; }
        //Check
        public PricePeriods[] PricePeriods { get; set; }
    }
}
