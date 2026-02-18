using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Validators;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Model;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Handlers
{


    public class GSLCalculatePaymentsHandler : IGSLCalculatePaymentsHandler
    {
        private ICalculateGSLPaymentsValidator _validator;
        private IGSLEarningsMapper _mapper;
        private IEarningsRepository _repository;
        private ICollectionPeriodApiClient _collectionPeriodApiClient;
        private ILogger<GSLCalculatePaymentsHandler> _logger;

        public GSLCalculatePaymentsHandler(
            ICalculateGSLPaymentsValidator validator,
            IGSLEarningsMapper mapper,
            IEarningsRepository repository,
            ICollectionPeriodApiClient collectionPeriodApiClient,
            ILogger<GSLCalculatePaymentsHandler> logger)
        {
            _validator = validator;
            _mapper = mapper;
            _repository = repository;
            _collectionPeriodApiClient = collectionPeriodApiClient;
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

            GrowthAndSkillsEarningModel mappedValues = _mapper.MapToGrowthAndSkillsEarningModel(message);


            // comment out temporary code
            //List<CollectionPeriodModel> imaginaryListFromCollectionPeriodAPI = new List<CollectionPeriodModel>(); //dictionary of academic years

            //foreach (var mappedValue in mappedValues.PricePeriods)
            //{
                
            //    if (imaginaryListFromCollectionPeriodAPI.Any(x =>
            //            x.AcademicYear = 2425 && x.Status == CollectionPeriodStatus.Open))
            //    {
            //        //using mapped values within here
            //        //set processed on a datetime , IEarningEvents + send required messages events out
            //        _mapper.MapToReceivedDASEarningsMessageModel(
            //            message); //won't be sent out if there wasn't an open collection period
            //    }
            //}

            //saved to the cache at the end 
            _repository.SaveEarnings(mappedValues);

        }

    }
}
//NotStarted = 1,
//Open = 2,
//Closed = 3,
//Completed = 4,
