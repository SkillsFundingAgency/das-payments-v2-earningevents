
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.EarningEvents.Model
{
    public class CollectionYear
    {
        public short Year { get; set; }
        public CollectionPeriodStatus Status { get; set; }
        public IEnumerable<CollectionPeriod> Periods { get; set; }
    }
}
