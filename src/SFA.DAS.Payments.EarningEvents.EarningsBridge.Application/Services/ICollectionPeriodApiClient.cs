using SFA.DAS.Payments.EarningEvents.Model;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;

public interface ICollectionPeriodApiClient
{
    Task<IEnumerable<CollectionYear>> GetOpenCollectionYears();
    Task<CollectionYear> GetOpenCollectionPeriods(string academicYear);
    Task<CollectionPeriod> GetCollectionPeriod(string academicYear, string period);
}