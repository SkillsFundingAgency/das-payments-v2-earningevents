using Microsoft.Extensions.Configuration;
using NServiceBus;
using Reqnroll;
using SFA.DAS.Payments.Messages.Common;

namespace SFA.DAS.Payments.EarningEvents.Specs.StepDefinitions
{
    [Binding]
    public class TestRunBindings 
    {
        public static IEndpointInstance endpoint { get; private set; }
        public static IConfiguration Config { get; private set; }
        public static HttpClient HttpClient { get; private set; }

        [BeforeTestRun]
        public static async Task SetUpMessaging()
        {
            Config = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appSettings.json"))
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appSettings.development.json"), true)
                .Build();

            var endpointConfig = new EndpointConfiguration("sfa-das-payments-requiredpayments-specs");
            var conventions = endpointConfig.Conventions();
            conventions.DefiningMessagesAs(type => type.IsMessage());
            endpointConfig.UseSerialization<NewtonsoftJsonSerializer>();
            //endpointConfig.SendOnly();
            var storageConnectionString = Config["ConnectionStrings:StorageConnectionString"];
            endpointConfig.UsePersistence<AzureTablePersistence>().ConnectionString(storageConnectionString);
            endpointConfig.UseTransport<AzureServiceBusTransport>()
                .ConnectionString(Config["ConnectionStrings:ServiceBusConnectionString"]);
            endpointConfig.EnableInstallers();
            var startable = await Endpoint.Create(endpointConfig);
            endpoint = await startable.Start();
        }
    }
}
