using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Mapping
{
    public interface IGSLApprenticeshipsMapper : IGrowthAndSkillsMapper
    {
        IEnumerable<GSLApprenticeshipEarningsEvent> MapToApprenticeshipEarningEvents(CalculateGrowthAndSkillsPayments source, IEnumerable<CollectionPeriodModel> openCollectionPeriods);
    }
}
