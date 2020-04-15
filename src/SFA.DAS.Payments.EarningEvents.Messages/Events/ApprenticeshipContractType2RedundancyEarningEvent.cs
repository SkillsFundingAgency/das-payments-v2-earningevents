﻿using System;
using SFA.DAS.Payments.Messages.Core;

namespace SFA.DAS.Payments.EarningEvents.Messages.Events
{
    public class ApprenticeshipContractType2RedundancyEarningEvent :ApprenticeshipContractTypeEarningsEvent,ILeafLevelMessage,IRedundancyEarningEvent
    {
        public ApprenticeshipContractType2RedundancyEarningEvent()
        {
            EventId = Guid.NewGuid();
        }
    }
}