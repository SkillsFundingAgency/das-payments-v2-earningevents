using SFA.DAS.Payments.Messages.Common;
using SFA.DAS.Payments.Messages.Common.Events;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.EarningEvents.Messages.Events
{
    public class Act1FunctionalSkillEarningsEvent : FunctionalSkillEarningsEvent, IMonitoredMessage
    {
        public Act1FunctionalSkillEarningsEvent()
        {
            ContractType = ContractType.Act1;
            FundingPlatformType = FundingPlatformType.SubmitLearnerData;
        }
    }
}