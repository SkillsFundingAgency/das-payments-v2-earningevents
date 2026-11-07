using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Validators;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services
{
    public class GSLEarningsProcessor
    {
        //private readonly IGSLEarningsService gslEarningsService;
        //private readonly ICollectionPeriodService collectionPeriodService;
        private readonly ICalculateGSLPaymentsValidator validator;

        public GSLEarningsProcessor(ICalculateGSLPaymentsValidator validator/*, IGSLEarningsService gslEarningsService, ICollectionPeriodService collectionPeriodService*/)
        {
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
            //this.gslEarningsService = gslEarningsService ?? throw new ArgumentNullException(nameof(gslEarningsService));
            //this.collectionPeriodService = collectionPeriodService ?? throw new ArgumentNullException(nameof(collectionPeriodService));
        }

        public async Task ProcessEarnings(CalculateGrowthAndSkillsPayments earnings) 
        { 
        }
    }
}

