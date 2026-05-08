using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Validators;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.Model.Core;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Handlers
{
    public class GSLCalculatePaymentsHandler : IGSLCalculatePaymentsHandler
    {
        private ICalculateGSLPaymentsValidator _validator;
        private IGrowthAndSkillsMapper _mapper;
        private IEarningsRepository _repository;
        private IPaymentsServiceBusPublisher _publisher;
        private ICollectionPeriodService _collectionPeriodService;
        private ILogger<GSLCalculatePaymentsHandler> _logger;

        public GSLCalculatePaymentsHandler(
            ICalculateGSLPaymentsValidator validator,
            IGrowthAndSkillsMapper mapper,
            IEarningsRepository repository,
            IPaymentsServiceBusPublisher publisher,
            ICollectionPeriodService collectionPeriodService,
            ILogger<GSLCalculatePaymentsHandler> logger)
        {
            _validator = validator;
            _mapper = mapper;
            _repository = repository;
            _publisher = publisher;
            _collectionPeriodService = collectionPeriodService;
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

            var openCollectionPeriods = await _collectionPeriodService.GetOpenCollectionPeriods();

            if(!openCollectionPeriods.Any())
            {
                await _repository.SaveEarnings(growthAndSkillsEarningModel);
                return;
            }

            foreach (var earning in growthAndSkillsEarningModel.PricePeriods)
            {
                if (openCollectionPeriods.Any(x => x.AcademicYear == earning.AcademicYear))
                {
                    earning.ProcessedOn = DateTime.UtcNow; // if ProcessedOn is not set then will be cached and picked up for processing later
                }
            }

            var requiredPaymentsEvents = _mapper.MapToShortCourseEarningEvents(message, openCollectionPeriods);

            var fundingSourceEvents = _mapper.MapToDasEarningsReceivedEvents(message, openCollectionPeriods);


            foreach (var requiredPaymentsEvent in requiredPaymentsEvents)
            {
                await _publisher.Publish<GSLShortCourseEarningsEvent>(requiredPaymentsEvent);
            }

            foreach (var fundingSourceEvent in fundingSourceEvents)
            {
                await _publisher.Publish<DasEarningsReceivedEvent>(fundingSourceEvent);
            }
            

            await _repository.SaveEarnings(growthAndSkillsEarningModel);
        }

    }
}

