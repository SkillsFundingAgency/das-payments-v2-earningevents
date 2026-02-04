using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Validators;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Handlers
{
    public interface IgSLCalculatePaymentsHandler
    {
        public void HandleGslCalculatePaymentsMessage(CalculateGSLPayments message);

    }

    public class GSLCalculatePaymentsHandler : IgSLCalculatePaymentsHandler
    {
        private ICalculateGSLPaymentsValidator _validator;
        private IgSLEarningsProcessor _processor;

        //takes in message object
        public GSLCalculatePaymentsHandler(ICalculateGSLPaymentsValidator validator,IgSLEarningsProcessor processor)
        {
            _validator = validator;
            _processor = processor;
        }


        // construction injection, route through interface, within module DI needs to happen for handler and processor
        public void HandleGslCalculatePaymentsMessage(CalculateGSLPayments message)
        {
            _validator.Validate(message);
                //error handling + logger needs to be done here
                //throw an exception if message invalid, log message + dead-letter queue the message by re-throwing
            //pings collection period API to do the collection period check
            _processor.MapToShortCourseEarningModel(message);
            //mapped object gets sent to SQL DB
            //mapped object gets converted into an event - could use mapper to map message to event model 
                //Double check if current collection period is stored in here
            //Sends off requiredpayments and earning audit message                







        }

    }
}
