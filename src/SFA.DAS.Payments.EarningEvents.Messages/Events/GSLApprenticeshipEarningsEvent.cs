using System;
using SFA.DAS.Payments.Messages.Common.Events;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.EarningEvents.Messages.Events
{
    public class GSLApprenticeshipEarningsEvent: ApprenticeshipContractTypeEarningsEvent, IContractTypeEarningEvent
    {
        public Guid ExternalEarningsId { get; set; }
        public ContractType ContractType { get; set; }
        public FundingPlatformType FundingPlatformType { get; set; }
    }
}