using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.Model.Core.Entities;


namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services
{
    public class CollectionPeriodApiClient : ICollectionPeriodApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CollectionPeriodApiClient> _logger;

        public CollectionPeriodApiClient(HttpClient httpClient, ILogger<CollectionPeriodApiClient> logger)
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
