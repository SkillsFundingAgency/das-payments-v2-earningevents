using Microsoft.Extensions.Configuration;
using NServiceBus;
using Reqnroll;
using SFA.DAS.Payments.Messages.Common;

namespace SFA.DAS.Payments.EarningEvents.Specs.StepDefinitions
{
    [Binding]
    public class TestRunBindings
    {
        public static IEndpointInstance PV2Endpoint { get; private set; }
        public static IEndpointInstance DASEndpoint { get; private set; }
        public static IConfiguration Config { get; private set; }


        [BeforeTestRun]
        public static async Task SetUpMessaging()
        {
            Config = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appSettings.json"))
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appSettings.development.json"), true)
                .Build();

            DASEndpoint = await CreateEndpoint("DASServiceBusConnectionString", true);
            PV2Endpoint = await CreateEndpoint("ServiceBusConnectionString");
        }

        public static async Task<IEndpointInstance> CreateEndpoint(string connectionName, bool sendOnly = false)
        {
            var endpointConfig = new EndpointConfiguration("sfa-das-payments-earningevents-bridge-specs");
            var conventions = endpointConfig.Conventions();
            conventions.DefiningMessagesAs(type => type.IsMessage());
            endpointConfig.UseSerialization<NewtonsoftJsonSerializer>();
            if (sendOnly)
                endpointConfig.SendOnly();
            var storageConnectionString = Config["ConnectionStrings:StorageConnectionString"];
            endpointConfig.UsePersistence<AzureTablePersistence>().ConnectionString(storageConnectionString);
            var connectionConfig = $"ConnectionStrings:{connectionName}";
            var connectionString = Config[connectionConfig];
            Console.WriteLine($"Config: {connectionConfig}, ConnectionString: {connectionString}");
            endpointConfig.UseTransport<AzureServiceBusTransport>()
                .ConnectionString(connectionString);
            endpointConfig.EnableInstallers();
            var startable = await Endpoint.Create(endpointConfig);
            return await startable.Start();
        }
    }
}
