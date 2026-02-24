using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.EarningEvents.Model;
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

        public async Task<IEnumerable<CollectionYear>> GetOpenCollectionYears()
        {
            try
            {
                var collectionYears = new List<CollectionYear>();
                HttpResponseMessage response = await _httpClient.GetAsync("/api/v1/collectionyear");
                if (response.IsSuccessStatusCode)
                {
                    collectionYears = await response.Content.ReadAsAsync<List<CollectionYear>>();
                }

                return collectionYears;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while calling Collection Period API to get open collection years. Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<CollectionYear> GetOpenCollectionPeriods(string academicYear)
        {
            try
            {
                CollectionYear collectionYear = null;
                HttpResponseMessage response = await _httpClient.GetAsync($"/api/v1/collectionyear/{academicYear}?status=Open");
                if (response.IsSuccessStatusCode)
                {
                    collectionYear = await response.Content.ReadAsAsync<CollectionYear>();
                }

                return collectionYear;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while calling Collection Period API to get collection year {academicYear}. Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<CollectionPeriod> GetCollectionPeriod(string academicYear, string period)
        {
            try
            {
                CollectionPeriod collectionPeriod = null;
                HttpResponseMessage response = await _httpClient.GetAsync($"/api/v1/collectionyear/{academicYear}/collectionperiod/{period}");
                if (response.IsSuccessStatusCode)
                {
                    collectionPeriod = await response.Content.ReadAsAsync<CollectionPeriod>();
                }

                return collectionPeriod;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while calling Collection Period API to get collection period {period} {academicYear}. Exception: {ex.Message}");
                throw;
            }
        }
    }
}
