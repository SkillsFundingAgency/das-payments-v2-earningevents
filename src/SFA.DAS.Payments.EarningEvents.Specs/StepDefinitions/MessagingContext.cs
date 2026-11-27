
namespace SFA.DAS.Payments.EarningEvents.Specs.StepDefinitions
{
    public class MessagingContext
    {
        private IEndpointInstance endpointInstance;

        public MessagingContext(IEndpointInstance endpointInstance)
        {
            this.endpointInstance = endpointInstance;
        }

        public async Task Send<T>(T message)
        {
            await endpointInstance.Send("sfa-das-payments-earningsevents-dasearningsbridge", message);
        }

        public async Task SendTo<T>(string destination, T message)
        {
            await endpointInstance.Send(destination, message);
        }
    }
}