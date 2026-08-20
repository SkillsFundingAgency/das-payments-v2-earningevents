using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Mapping
{
    public interface IGSLFunctionalSkillMapper: IGrowthAndSkillsMapper
    {
        void Map(CalculateGrowthAndSkillsPayments source, CollectionPeriodModel collectionPeriod, GSLFunctionalSkillEarningsEvent destination);
    }

    public class GSLFunctionalSkillMapper : GrowthAndSkillsMapper, IGSLFunctionalSkillMapper
    {
        public void Map(CalculateGrowthAndSkillsPayments source, CollectionPeriodModel collectionPeriod, GSLFunctionalSkillEarningsEvent destination)
        {
            throw new NotImplementedException();
        }
    }
}