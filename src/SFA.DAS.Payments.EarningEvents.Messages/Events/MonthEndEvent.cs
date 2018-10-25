using SFA.DAS.Payments.Messages.Core;
using SFA.DAS.Payments.Model.Core;

namespace SFA.DAS.Payments.EarningEvents.Messages.Events
{
    public class MonthEndEvent : IPaymentsMessage
    {
        public long JobId { get; set; }
        public CalendarPeriod CollectionPeriod { get; set; }
    }
}
