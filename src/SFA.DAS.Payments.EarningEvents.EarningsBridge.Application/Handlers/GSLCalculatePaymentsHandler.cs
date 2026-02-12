using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Validators;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Handlers
{


    public class GSLCalculatePaymentsHandler : IGSLCalculatePaymentsHandler
    {
        private ICalculateGSLPaymentsValidator _validator;
        private IGSLEarningsMapper mapper;
        private IEarningsRepository _repository;
        //Need to double check if the Ilogger needs to be handled differently / using a different class
        private ILogger<GSLCalculatePaymentsHandler> _logger;

        public GSLCalculatePaymentsHandler(
            ICalculateGSLPaymentsValidator validator,
            IGSLEarningsMapper mapper,
            IEarningsRepository repository,
            ILogger<GSLCalculatePaymentsHandler> logger)
        {
            _validator = validator;
            this.mapper = mapper;
            _repository = repository;
            _logger = logger;
        }


        public void HandleGslCalculatePaymentsMessage(CalculateGrowthAndSkillsPayments message)
        {
            try
            {
                _validator.Validate(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate GSL calculate payments message");
                throw;
            }

            //throw an exception if message invalid, log message + dead-letter queue the message by re-throwing
            //pings collection period API to do the collection period check

            //mapped object gets sent to SQL DB - DI
            var mappedValues = mapper.MapToShortCourseEarningModel(message);
            _repository.SaveEarnings(mappedValues);

            //mapped object gets converted into an event - could use mapper to map message to event model 
                //Double check if current collection period is stored in here
            //Sends off requiredpayments and earning audit message                
        }

    }
}
