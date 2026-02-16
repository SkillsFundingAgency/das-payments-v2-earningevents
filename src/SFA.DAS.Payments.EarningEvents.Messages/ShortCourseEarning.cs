using System.Collections.Generic;
using SFA.DAS.Payments.Model.Core;

namespace SFA.DAS.Payments.EarningEvents.Messages
{
    public class ShortCourseEarning
    {
        public ShortCourseEarningType Type { get; set; }
        public IEnumerable<EarningPeriod> Periods { get; set; }
    }
}
