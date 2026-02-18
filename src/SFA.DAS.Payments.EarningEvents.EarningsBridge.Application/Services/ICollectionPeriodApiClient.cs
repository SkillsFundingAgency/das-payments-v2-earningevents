using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;

public interface ICollectionPeriodApiClient
{
    Task<CollectionPeriodModel> GetCollectionPeriod(int academicYear);
}