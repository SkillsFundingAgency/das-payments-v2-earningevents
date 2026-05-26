
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
    }
}