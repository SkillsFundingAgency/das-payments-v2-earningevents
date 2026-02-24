
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services
{
    public class CollectionPeriodService : ICollectionPeriodService
    {
        private readonly ICollectionPeriodApiClient _collectionPeriodApiClient;
        private readonly IGrowthAndSkillsMapper _mapper;

        public CollectionPeriodService(ICollectionPeriodApiClient collectionPeriodApiClient, IGrowthAndSkillsMapper mapper)
        {
            _collectionPeriodApiClient = collectionPeriodApiClient;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CollectionPeriodModel>> GetOpenCollectionPeriods()
        {
            var collectionPeriods = new List<CollectionPeriodModel>();

            var collectionYears = await _collectionPeriodApiClient.GetOpenCollectionYears();

            foreach (var collectionYear in collectionYears)
            {
                var collectionYearWithPeriods = await _collectionPeriodApiClient.GetOpenCollectionPeriods(collectionYear.Year.ToString());
                collectionPeriods.AddRange(_mapper.MapCollectionYearToCollectionPeriodModels(collectionYearWithPeriods));
            }

            return collectionPeriods;
        }
    }
}
