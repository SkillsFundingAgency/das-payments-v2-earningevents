using System.Collections.Generic;
using SFA.DAS.Payments.EarningEvents.Messages.Events;

namespace SFA.DAS.Payments.EarningEvents.Domain.Mapping
{
    public interface IHiringPaymentEarningEventConverter
    {
        List<EarningEvent> Convert(List<EarningEvent> earningEvents);
    }
}
