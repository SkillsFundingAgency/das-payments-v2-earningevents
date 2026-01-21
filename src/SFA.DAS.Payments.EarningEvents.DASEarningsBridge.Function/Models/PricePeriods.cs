using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.Payments.EarningEvents.DASEarningsSubscriber.Models
{
    internal class PricePeriods
    {
        public decimal Price { get; set; }
        public DateTime StartDate { get; set; }
        //check if this is the right way we want to handle nullable
        public DateTime? EndDate { get; set; }
        public int NumberOfInstalments { get; set; }
        public decimal InstalmentAmount { get; set; }
        public decimal CompletionAmount { get; set; }
        //check if we want to set this to nullable
        public EarningPeriod[] DeliveryPeriods { get; set; }
    }
}
