using AutoMapper;
using Castle.Core.Internal;
using SFA.DAS.Payments.EarningEvents.Application.Interfaces;
using SFA.DAS.Payments.EarningEvents.Domain.Mapping;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.Internal.Commands;
using SFA.DAS.Payments.Messages.Common.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Payments.EarningEvents.Application.Mapping
{
    public class ApprenticeshipContractTypeEarningsEventBuilder : EarningEventBuilderBase,
        IApprenticeshipContractTypeEarningsEventBuilder
    {
        private readonly IApprenticeshipContractTypeEarningsEventFactory factory;
        private readonly IRedundancyEarningService redundancyEarningService;
        private readonly IMapper mapper;
        private const string RemovedLearnerWithin42Days = "Removed Learner Within 42 Days";

        public ApprenticeshipContractTypeEarningsEventBuilder(IApprenticeshipContractTypeEarningsEventFactory factory,
            IRedundancyEarningService redundancyEarningService,
            IMapper mapper)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.redundancyEarningService = redundancyEarningService ?? throw new ArgumentNullException(nameof(redundancyEarningService));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public List<ApprenticeshipContractTypeEarningsEvent> Build(ProcessLearnerCommand learnerSubmission)
        {
            var intermediateResults = InitialLearnerTransform(learnerSubmission, true);
            var results = new List<ApprenticeshipContractTypeEarningsEvent>();

            foreach (var intermediateLearningAim in intermediateResults)
            {
                var episodesByContractType = intermediateLearningAim.PriceEpisodes.GroupBy(x => x.PriceEpisodeValues.PriceEpisodeContractType);

                //if there are no price episodes, this means the learner has been removed within 42 days of the start date
                if (episodesByContractType.IsNullOrEmpty())
                {
                    // Create an ineligible earning event for learners removed within 42 days
                    var ineligibleEarningEvent = factory.Create(RemovedLearnerWithin42Days);

                    mapper.Map(intermediateLearningAim, ineligibleEarningEvent);
                    results.Add(ineligibleEarningEvent);

                    continue;
                }

                var redundancyPeriods = CalculateRedundancyPeriods(intermediateLearningAim.PriceEpisodes);

                foreach (var priceEpisodes in episodesByContractType)
                {
                    var learnerWithSortedPriceEpisodes =
                        intermediateLearningAim.CopyReplacingPriceEpisodes(priceEpisodes);

                     var earningEvent = factory.Create(priceEpisodes.Key);
                    if (!earningEvent.IsPayable) continue;

                    mapper.Map(learnerWithSortedPriceEpisodes, earningEvent);

                    results.AddRange(redundancyEarningService.OriginalAndRedundancyEarningEventIfRequired(earningEvent, redundancyPeriods));
                }
            }

            return results;
        }
    }
}