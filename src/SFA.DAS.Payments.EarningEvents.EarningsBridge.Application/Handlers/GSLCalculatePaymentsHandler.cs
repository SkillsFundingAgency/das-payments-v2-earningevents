using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Validators;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Model;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Handlers
{


    public class GSLCalculatePaymentsHandler : IGSLCalculatePaymentsHandler
    {
        private ICalculateGSLPaymentsValidator _validator;
        private IGSLEarningsMapper _mapper;
        private IEarningsRepository _repository;
        private ICollectionPeriodApi _collectionPeriodAPI;
        private ILogger<GSLCalculatePaymentsHandler> _logger;

        public GSLCalculatePaymentsHandler(
            ICalculateGSLPaymentsValidator validator,
            IGSLEarningsMapper mapper,
            IEarningsRepository repository,
            ICollectionPeriodApi collectionPeriodAPI,
            ILogger<GSLCalculatePaymentsHandler> logger)
        {
            _validator = validator;
            _mapper = mapper;
            _repository = repository;
            _collectionPeriodAPI = collectionPeriodAPI;
            _logger = logger;
            
        }


        public void HandleGslCalculatePaymentsMessage(CalculateGSLPayments message)
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

            ShortCourseEarningModel mappedValues = _mapper.MapToShortCourseEarningModel(message);



            List<CollectionPeriodModel> imaginaryListFromCollectionPeriodAPI = new List<CollectionPeriodModel>(); //dictionary of academic years

            foreach (var mappedValue in mappedValues.PricePeriods)
            {
                
                if (imaginaryListFromCollectionPeriodAPI.Any(x =>
                        x.AcademicYear = 2425 && x.Status == CollectionPeriodStatus.Open))
                {
                    //using mapped values within here
                    //set processed on a datetime , IEarningEvents + send required messages events out
                    _mapper.MapToReceivedDASEarningsMessageModel(
                        message); //won't be sent out if there wasn't an open collection period
                }
            }

            //last step sent to DB if it hasn't been processed
            _repository.SaveEarnings(mappedValues);  //mapped object gets sent to SQL DB - now called cache

        }

    }
}
//NotStarted = 1,
//Open = 2,
//Closed = 3,
//Completed = 4,
