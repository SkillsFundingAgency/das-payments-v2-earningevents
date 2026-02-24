using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Validators;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Model;
using SFA.DAS.Payments.Model.Core;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Handlers
{


    public class GSLCalculatePaymentsHandler : IGSLCalculatePaymentsHandler
    {
        private ICalculateGSLPaymentsValidator _validator;
        private IGSLEarningsMapper _mapper;
        private IEarningsRepository _repository;
        private IPaymentsServiceBusPublisher _publisher;
        private ILogger<GSLCalculatePaymentsHandler> _logger;

        public GSLCalculatePaymentsHandler(
            ICalculateGSLPaymentsValidator validator,
            IGSLEarningsMapper mapper,
            IEarningsRepository repository,
            IPaymentsServiceBusPublisher publisher,
            ILogger<GSLCalculatePaymentsHandler> logger)
        {
            _validator = validator;
            _mapper = mapper;
            _repository = repository;
            _publisher = publisher;
            _logger = logger;
        }
        
        public async Task HandleGslCalculatePaymentsMessage(CalculateGrowthAndSkillsPayments message)
        {
            try
            {
                if (!_validator.Validate(message))
                {
                    return;
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate GSL calculate payments message");
                throw;
            }

            var growthAndSkillsEarningModel = _mapper.MapToGrowthAndSkillsEarningModel(message);

            // TEMPORARY CODE TO ENABLE TESTING BEFORE COLLECTION PERIOD API INTEGRATION - REMOVE BEFORE MERGE TO MAIN!!!
            
            // use hard-coded collection period values for outbound messages
            
            var currentCollectionPeriod = new CollectionPeriod
            {
                Period = 1,
                AcademicYear = 2526
            };

            // assume that all inbound earnings are associated with an academic year
            // with an open collection period so can be processed downstream

            foreach (var earning in growthAndSkillsEarningModel.PricePeriods)
            {
                earning.ProcessedOn = DateTime.UtcNow;
            }

            var requiredPaymentsEvent = _mapper.MapToShortCourseEarningEvent(message, 
                                                                             currentCollectionPeriod.AcademicYear, 
                                                                             currentCollectionPeriod.Period);

            var fundingSourceEvent = _mapper.MapToDasEarningsReceivedEvent(message,
                                                                           currentCollectionPeriod.AcademicYear, 
                                                                           currentCollectionPeriod.Period);

            // TEMPORARY CODE TO ENABLE TESTING BEFORE COLLECTION PERIOD API INTEGRATION - REMOVE BEFORE MERGE TO MAIN!!!

            await _publisher.Publish<GSLShortCourseEarningsEvent>(requiredPaymentsEvent);
            await _publisher.Publish<DasEarningsReceivedEvent>(fundingSourceEvent);

            await _repository.SaveEarnings(growthAndSkillsEarningModel);
        }

    }
}

