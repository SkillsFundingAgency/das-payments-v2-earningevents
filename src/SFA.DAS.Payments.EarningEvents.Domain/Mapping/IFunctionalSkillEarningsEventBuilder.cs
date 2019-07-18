﻿using System.Collections.Generic;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.Internal.Commands;

namespace SFA.DAS.Payments.EarningEvents.Domain.Mapping
{
    public interface IFunctionalSkillEarningsEventBuilder
    {
        List<ApprenticeshipContract2TypeFunctionalSkillEarningsEvent> Build(ProcessLearnerCommand learnerSubmission);
    }
}