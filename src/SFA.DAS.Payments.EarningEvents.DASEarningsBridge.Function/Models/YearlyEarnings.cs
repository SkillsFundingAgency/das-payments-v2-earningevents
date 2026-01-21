using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.Payments.EarningEvents.DASEarningsSubscriber.Models
{
    internal class YearlyEarnings
    {
        public int Year { get; set; }
        //Check
        public PricePeriods[] PricePeriods { get; set; }
    }
}
