
namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services
{
    public interface IPaymentsServiceBusPublisher
    {
        Task Publish<T>(T message);
    }
}
