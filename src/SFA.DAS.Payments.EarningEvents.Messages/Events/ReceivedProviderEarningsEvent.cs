using SFA.DAS.Payments.Model.Core;
using System;
using SFA.DAS.Payments.Messages.Common;

namespace SFA.DAS.Payments.EarningEvents.Messages.Events
{
    public class ReceivedProviderEarningsEvent : IPaymentsMessage
    {
        public long JobId { get; set; }
        public DateTimeOffset EventTime { get; set; }
        public long Ukprn { get; set; }
        public DateTime IlrSubmissionDateTime { get; set; }
        public CollectionPeriod CollectionPeriod { get; set; }
    }
}
