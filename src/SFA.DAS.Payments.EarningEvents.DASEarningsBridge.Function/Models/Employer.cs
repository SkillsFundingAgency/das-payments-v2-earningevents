using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.Payments.EarningEvents.DASEarningsSubscriber.Models
{
    internal class Employer
    {
        public long AccountId { get; set; }
        public EmployerType EmployerType { get; set; }
        public long FundingAccountId { get; set; }
    }
}

public enum EmployerType{
    Levy,
    NonLevy //it needs to be decided if this is going to have a hyphen or not 
}