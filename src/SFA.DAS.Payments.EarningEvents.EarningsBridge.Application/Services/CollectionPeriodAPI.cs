using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Handlers;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.Model.Core.Entities;


namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services
{
    public class CollectionPeriodAPI
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CollectionPeriodAPI> _logger;

        public CollectionPeriodAPI(HttpClient httpClient, ILogger<CollectionPeriodAPI> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        //second one where we get open collection periods but not in an object?
        
        public async Task<CollectionPeriodModel> GetCollectionPeriod(int academicYear)
        {
            try
            {
                CollectionPeriodModel collectionPeriod = null;
                HttpResponseMessage response = await _httpClient.GetAsync(academicYear.ToString()); 
                if (response.IsSuccessStatusCode)
                {
                    collectionPeriod = await response.Content.ReadAsAsync<CollectionPeriodModel>();
                }

                return collectionPeriod;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while calling Collection Period API to get collection periods for academic year: {academicYear}. Exception: {ex.Message}");
                throw;
            }
        }
    }
}
