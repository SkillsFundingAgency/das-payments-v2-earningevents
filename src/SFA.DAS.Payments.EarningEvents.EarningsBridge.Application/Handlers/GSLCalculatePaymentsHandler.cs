using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Mapping;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Processors;
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
        private IGrowthAndSkillsMapper _growthAndSkillsMapper;
        private IEarningsRepository _repository;
        private IGSLEarningsService _gslEarningsService;
        private ICollectionPeriodService _collectionPeriodService;
        private IGSLProcessorFactory _gslProcessorFactory;
        private ILogger<GSLCalculatePaymentsHandler> _logger;

        public GSLCalculatePaymentsHandler(
            ICalculateGSLPaymentsValidator validator,
            IGrowthAndSkillsMapper growthAndSkillsMapper,
            IEarningsRepository repository,
            IGSLEarningsService gslEarningsService,
            ICollectionPeriodService collectionPeriodService,
            IGSLProcessorFactory processorFactory,
            ILogger<GSLCalculatePaymentsHandler> logger)
        {
            _validator = validator;
            _growthAndSkillsMapper = growthAndSkillsMapper;
            _repository = repository;
            _gslEarningsService = gslEarningsService;
            _collectionPeriodService = collectionPeriodService;
            _gslProcessorFactory = processorFactory;
            _logger = logger;
        }

        public async Task HandleGslCalculatePaymentsMessage(CalculateGrowthAndSkillsPayments message, bool isReprocessing = false)
        {
            var logProperties = new
            {
                ExternalEarningsId = message?.EarningsId,
                UKPRN = message?.UKPRN,
                ULN = message?.Learner?.ULN,
                AcademicYear = message?.Earnings?.FirstOrDefault()?.AcademicYear,
                IsReprocessing = isReprocessing,
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

            if (!isReprocessing)
            {
                try
                {
                    // Check if earnings in DB are the latest
                    var dbEarnings = await _repository.GetGrowthAndSkillsEarnings(ukPrn: message.UKPRN, uln: message.Learner.ULN, courseCode: message.Training.CourseCode);
                    var earningsAreLatest = _gslEarningsService.CheckEarningsAreLatest(dbEarnings, message.EarningsId);

                    if (!earningsAreLatest)
                    {
                        _logger.LogEarningsBridgeTrace("EarningsBridge.OutdatedEarningsSkipped", logProperties);
                        return; // If earnings are not the latest, don't proceed
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogEarningsBridgeError(ex, "EarningsBridge.MessageProcessingFailed", logProperties);

                    throw;
                }
            }


            var growthAndSkillsEarningModel = _growthAndSkillsMapper.MapToGrowthAndSkillsEarningModel(message);

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
                    earning.ProcessedOn = DateTime.UtcNow;
                }
            }

            _logger.LogEarningsBridgeTrace("EarningsBridge.EarningsProcessingStarted", logProperties);

            var processor = _gslProcessorFactory.CreateGSLProcessor(growthAndSkillsEarningModel.CourseType);
            await processor.Process(message, openCollectionPeriods);

            var matchingOpenPeriods = openCollectionPeriods
                .Where(period => growthAndSkillsEarningModel.PricePeriods.Any(pricePeriod => pricePeriod.AcademicYear == period.AcademicYear))
                .ToList();

            foreach (var period in matchingOpenPeriods)
            {
                await _repository.MarkEarningProcessed(growthAndSkillsEarningModel.EarningsId, period.AcademicYear, (byte)period.Period, DateTime.UtcNow);
                if (isReprocessing)
                {
                    await _repository.MarkPricePeriodsProcessed(growthAndSkillsEarningModel.EarningsId, period.AcademicYear, DateTime.UtcNow);
                }
            }

            if (!isReprocessing)
            {
                await _repository.SaveEarnings(growthAndSkillsEarningModel);
            }

            _logger.LogEarningsBridgeTrace("EarningsBridge.EarningsProcessingCompleted", logProperties);
        }
    }
}