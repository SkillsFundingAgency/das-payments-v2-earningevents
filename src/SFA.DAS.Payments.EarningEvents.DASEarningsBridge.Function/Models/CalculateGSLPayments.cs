using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.Payments.EarningEvents.DASEarningsSubscriber.Models
{
    internal class CalculateGSLPayments
    {

        public TYPE EarningsId { get; set; }
        public long UKPRN { get; set; }
        public Learner Learner { get; set; }
        public Training Training { get; set; }
        public YearlyEarnings[] Earnings { get; set; }

    }
}
