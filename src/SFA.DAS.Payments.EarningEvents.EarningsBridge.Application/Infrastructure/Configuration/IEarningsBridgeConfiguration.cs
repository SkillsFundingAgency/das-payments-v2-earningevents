namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Infrastructure.Configuration
{
    public interface IEarningsBridgeConfiguration
    {
        string PaymentsConnectionString { get; set; }
        string DASServiceBusConnectionString { get; set; }
        string DASServiceBusQueueName { get; set; }
        string PaymentsServiceBusConnectionString { get; set; }
        string CollectionPeriodApiBaseAddress { get; set; }
    }
}
