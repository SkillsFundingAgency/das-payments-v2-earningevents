using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.Payments.EarningEvents.DASEarningsSubscriber.Models
{
    internal class CalculateApprenticeshipPayments : CalculateGSLPayments
    {
        //It needs to be evaluated whether decimal should be used, additionally
        //Do we need to add decimal place range here?

        public decimal EmployerContribution { get; set; }
    }

}
