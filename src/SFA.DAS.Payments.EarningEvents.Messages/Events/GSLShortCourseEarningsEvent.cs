using System;
using System.Collections.Generic;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.EarningEvents.Messages.Events
{
    // ReSharper disable once InconsistentNaming
    public class GSLShortCourseEarningsEvent : EarningEvent 
    {
        public Guid ExternalEarningsId { get; set; } 
        public IEnumerable<ShortCourseEarning> Earnings { get; set; }
        public FundingPlatformType FundingPlatformType { get; set; }
    }
}
