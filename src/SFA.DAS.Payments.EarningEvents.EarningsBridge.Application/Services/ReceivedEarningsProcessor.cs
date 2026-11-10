using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Handlers;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Validators;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services
{
    public interface IReceivedEarningsProcessor 
    {
        Task ProcessEarnings(CalculateGrowthAndSkillsPayments earnings);
    }

    public class ReceivedEarningsProcessor: IReceivedEarningsProcessor
    {
        private readonly ILogger<GSLCalculatePaymentsHandler> logger;

        //private readonly IGSLEarningsService gslEarningsService;
        //private readonly ICollectionPeriodService collectionPeriodService;
        private readonly ICalculateGSLPaymentsValidator validator;

        public ReceivedEarningsProcessor(ILogger<GSLCalculatePaymentsHandler> logger, ICalculateGSLPaymentsValidator validator/*, IGSLEarningsService gslEarningsService, ICollectionPeriodService collectionPeriodService*/)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
            //this.gslEarningsService = gslEarningsService ?? throw new ArgumentNullException(nameof(gslEarningsService));
            //this.collectionPeriodService = collectionPeriodService ?? throw new ArgumentNullException(nameof(collectionPeriodService));
        }

        public async Task ProcessEarnings(CalculateGrowthAndSkillsPayments earnings) 
        {
            ValidateEarnings(earnings);
        }

        private void ValidateEarnings(CalculateGrowthAndSkillsPayments earnings)
        {
            try 
            {
                validator.Validate(earnings);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to validate calculate GSL payments message!!! Error: {ex.Message}");
                throw;
            }
        }
    }
}

