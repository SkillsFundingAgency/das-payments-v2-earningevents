
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services
{
    public interface ICollectionPeriodService
    {
        Task<IEnumerable<CollectionPeriodModel>> GetOpenCollectionPeriods();
    }
}
