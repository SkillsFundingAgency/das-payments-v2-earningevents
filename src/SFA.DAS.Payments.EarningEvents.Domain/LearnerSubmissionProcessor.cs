﻿using System;
using SFA.DAS.Payments.Core.Validation;
using SFA.DAS.Payments.EarningEvents.Domain.Mapping;
using SFA.DAS.Payments.EarningEvents.Domain.Validation.Learner;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.Internal.Commands;

namespace SFA.DAS.Payments.EarningEvents.Domain
{
    public interface ILearnerSubmissionProcessor
    {
        (ValidationResult Validation, ApprenticeshipContractTypeEarningsEvent EarningsEvent) GenerateEarnings(
            ProcessLearnerCommand learnerSubmission);
    }

    public class LearnerSubmissionProcessor: ILearnerSubmissionProcessor
    {
        private readonly ILearnerValidator learnerValidator;
        private readonly IApprenticeshipContractTypeEarningsEventBuilder apprenticeshipContractTypeEarningsEventBuilder;
        public LearnerSubmissionProcessor(ILearnerValidator learnerValidator, IApprenticeshipContractTypeEarningsEventBuilder apprenticeshipContractTypeEarningsEventBuilder)
        {
            this.learnerValidator = learnerValidator ?? throw new ArgumentNullException(nameof(learnerValidator));
            this.apprenticeshipContractTypeEarningsEventBuilder = apprenticeshipContractTypeEarningsEventBuilder ?? throw new ArgumentNullException(nameof(apprenticeshipContractTypeEarningsEventBuilder));
        }

        public (ValidationResult Validation, ApprenticeshipContractTypeEarningsEvent EarningsEvent)  GenerateEarnings(ProcessLearnerCommand learnerSubmission)
        {
            var validationResult = learnerValidator.Validate(learnerSubmission.Learner);
            if (validationResult.Failed)
                return (Validation: validationResult, EarningsEvent:null);

            var earningsEvent =
                apprenticeshipContractTypeEarningsEventBuilder.Build(learnerSubmission);
            return (Validation: validationResult, EarningsEvent: earningsEvent); 
        }
    }
}