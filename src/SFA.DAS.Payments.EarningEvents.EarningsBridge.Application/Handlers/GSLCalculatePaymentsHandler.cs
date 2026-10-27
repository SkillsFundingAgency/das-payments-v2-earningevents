using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Logging;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Validators;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Handlers
{
    public class GSLCalculatePaymentsHandler : IGSLCalculatePaymentsHandler
    {
        private ICalculateGSLPaymentsValidator _validator;
        private IGrowthAndSkillsMapper _mapper;
        private IEarningsRepository _repository;
        private IGSLEarningsService _gslEarningsService;
        private IPaymentsServiceBusPublisher _publisher;
        private ICollectionPeriodService _collectionPeriodService;
        private ILogger<GSLCalculatePaymentsHandler> _logger;

        public GSLCalculatePaymentsHandler(
            ICalculateGSLPaymentsValidator validator,
            IGrowthAndSkillsMapper mapper,
            IEarningsRepository repository,
            IGSLEarningsService gslEarningsService,
            IPaymentsServiceBusPublisher publisher,
            ICollectionPeriodService collectionPeriodService,
            ILogger<GSLCalculatePaymentsHandler> logger)
        {
            _validator = validator;
            _mapper = mapper;
            _repository = repository;
            _gslEarningsService = gslEarningsService;
            _publisher = publisher;
            _collectionPeriodService = collectionPeriodService;
            _logger = logger;
        }

        public async Task HandleGslCalculatePaymentsMessage(CalculateGrowthAndSkillsPayments message)
        {
            var logProperties = new
            {
                ExternalEarningsId = message?.EarningsId,
                UKPRN = message?.UKPRN,
                ULN = message?.Learner?.ULN,
                AcademicYear = message?.Earnings?.FirstOrDefault()?.AcademicYear,
                CollectionPeriod = message?.Earnings?
                    .SelectMany(x => x.PricePeriods)
                    .SelectMany(x => x.Periods)
                    .Select(x => x.DeliveryPeriod)
                    .FirstOrDefault()
            };

            try
            {
                if (!_validator.Validate(message))
                {
                    _logger.LogEarningsBridgeTrace("EarningsBridge.MessageValidationFailed", logProperties);

                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogEarningsBridgeError(ex, "EarningsBridge.MessageValidationException", logProperties);

                throw;
            }            

            try
            {
                // Check if earnings in DB are the latest
                var dbEarnings = await _repository.GetGrowthAndSkillsEarnings(ukPrn: message.UKPRN, uln: message.Learner.ULN, courseCode: message.Training.CourseCode);
                var earningsAreLatest = _gslEarningsService.CheckEarningsAreLatest(dbEarnings, message.EarningsId);

                if (!earningsAreLatest)
                {
                    _logger.LogEarningsBridgeTrace("EarningsBridge.MessageReprocessed", logProperties);

                    return; // If earnings are not the latest, don't proceed
                }
            }
            catch (Exception ex)
            {
                _logger.LogEarningsBridgeError(ex, "EarningsBridge.MessageProcessingFailed", logProperties);

                throw;
            }

            var growthAndSkillsEarningModel = _mapper.MapToGrowthAndSkillsEarningModel(message);

            var openCollectionPeriods = await _collectionPeriodService.GetOpenCollectionPeriods();

            if (!openCollectionPeriods.Any())
            {
                _logger.LogEarningsBridgeTrace("EarningsBridge.NoOpenCollectionPeriods", logProperties);

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

                _logger.LogEarningsBridgeTrace("EarningsBridge.GSLShortCourseEarningsEventPublished",
                new
                {
                    ExternalEarningsId = requiredPaymentsEvent.ExternalEarningsId,
                    UKPRN = message.UKPRN,
                    ULN = message.Learner.ULN,
                    AcademicYear = message.Earnings?.FirstOrDefault()?.AcademicYear,
                    CollectionPeriod = requiredPaymentsEvent.CollectionYear,
                    FundingPlatformType = requiredPaymentsEvent.FundingPlatformType
                });
            }

            foreach (var fundingSourceEvent in fundingSourceEvents)
            {
                await _publisher.Publish<DasEarningsReceivedEvent>(fundingSourceEvent);

                _logger.LogEarningsBridgeTrace("EarningsBridge.DasEarningsReceivedEventPublished",
                new
                {
                    ExternalEarningsId = fundingSourceEvent.EarningsId,
                    UKPRN = fundingSourceEvent.UKPRN,
                    ULN = fundingSourceEvent.ULN,
                    AcademicYear = message.Earnings?.FirstOrDefault()?.AcademicYear,
                    CollectionPeriod = fundingSourceEvent.CollectionPeriod?.Period
                });
            }

            await _repository.SaveEarnings(growthAndSkillsEarningModel);
        }
    }
}