using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Mapping;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Processors
{
    public class GslApprenticeshipPaymentsProcessor : IGslProcessor
    {
        public GslApprenticeshipPaymentsProcessor(
            IGslApprenticeshipsMapper mapper,
            IPaymentsServiceBusPublisher publisher)
        {
        }

        public async Task Process(CalculateGrowthAndSkillsPayments message, IEnumerable<CollectionPeriodModel> openCollectionPeriods)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }
    }
}
