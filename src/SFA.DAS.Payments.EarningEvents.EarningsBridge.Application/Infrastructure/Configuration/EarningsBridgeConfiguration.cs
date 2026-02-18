namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Infrastructure.Configuration
{
    public class EarningsBridgeConfiguration : IEarningsBridgeConfiguration
    {
        public string PaymentsConnectionString { get; set; }
        public string DASServiceBusConnectionString { get; set; }
        public string DASServiceBusQueueName { get; set; }
        public string PaymentsServiceBusConnectionString { get; set; }
        public string CollectionPeriodApiBaseAddress { get; set; }
    }
}
