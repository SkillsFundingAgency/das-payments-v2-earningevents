using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.Payments.EarningEvents.DASEarningsSubscriber.Models
{
    internal class Learner
    {
        public TYPE DASLearnedId { get; set; }
        public long ULN { get; set; }
        public String Reference { get; set; }
    }
}
