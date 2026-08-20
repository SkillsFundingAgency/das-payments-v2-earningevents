using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Mapping;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Processors;
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
        
        public async Task HandleGSLCalculatePaymentsMessage(CalculateGrowthAndSkillsPayments message)
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

            try
            {
                // Check if earnings in DB are the latest
                var dbEarnings = await _repository.GetGrowthAndSkillsEarnings(ukPrn: message.UKPRN, uln: message.Learner.ULN, courseCode: message.Training.CourseCode);
                var earningsAreLatest = _gslEarningsService.CheckEarningsAreLatest(dbEarnings, message.EarningsId);
                if (!earningsAreLatest)
                {
                    _logger.LogWarning("Earnings received are not the latest. " +
                                           "Skipping processing for message with EarningsId: {EarningsId}, UKPRN: {UKPRN}, ULN: {ULN}, CourseCode: {CourseCode}",
                        message.EarningsId, message.UKPRN, message.Learner.ULN, message.Training.CourseCode);
                    return; // If earnings are not the latest, don't proceed
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing CalculateGrowthAndSkillsPayments with " +
                                     "EarningsId: {EarningsId}, UKPRN: {UKPRN}, ULN: {ULN}, CourseCode: {CourseCode}",
                    message.EarningsId, message.UKPRN, message.Learner.ULN, message.Training.CourseCode);
                throw;
            }


            var growthAndSkillsEarningModel = _growthAndSkillsMapper.MapToGrowthAndSkillsEarningModel(message);

            var openCollectionPeriods = await _collectionPeriodService.GetOpenCollectionPeriods();

            if (!openCollectionPeriods.Any())
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

            var processor = _gslProcessorFactory.CreateGSLProcessor(growthAndSkillsEarningModel.LearningType);
            await processor.Process(message, openCollectionPeriods);

            await _repository.SaveEarnings(growthAndSkillsEarningModel);
        }
    }
}

