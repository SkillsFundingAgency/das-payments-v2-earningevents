namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services
{
    public interface IRepositoryService
    {
        bool CheckEarningsAreLatest(Guid messageTimestamp, Guid tableEntryTimestamp);
    }
}
