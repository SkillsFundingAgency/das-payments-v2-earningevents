using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SFA.DAS.Payments.EarningEvents.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.Domain.Mapping;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Model;
using SFA.DAS.Payments.Model.Core.Incentives;
using Common = SFA.DAS.Payments.Model.Core;

namespace SFA.DAS.Payments.EarningEvents.Application.Services
{
    public class HiringIncentiveEarningsProvider : IHiringIncentiveEarningsProvider
    {
        private readonly IHiringPaymentEarningsRepository repository;

        public HiringIncentiveEarningsProvider(IHiringPaymentEarningsRepository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public List<(ApprenticeshipContractType1EarningEvent Event, List<IncentiveEarning> IncentiveEarnings)> GetHiringIncentiveEarnings(List<ApprenticeshipContractType1EarningEvent> candidates)
        {
            var results = new List<(ApprenticeshipContractType1EarningEvent Event, List<IncentiveEarning> IncentiveEarnings)>();

            var eventsByApprenticeship = candidates
                .GroupBy(act1Event => (act1Event.Ukprn, act1Event.Learner.Uln, act1Event.LearningAim.StandardCode));

            foreach (var apprenticeshipEvents in eventsByApprenticeship)
            {
                var first = apprenticeshipEvents.First();
                var stored = repository.GetLatestHiringPaymentEarnings(first.Ukprn, first.Learner.Uln, first.LearningAim.StandardCode.ToString());
                if (stored == null)
                {
                    continue;
                }

                var target = apprenticeshipEvents.OrderBy(x => x.StartDate).First();
                var hiringIncentives = MapHiringIncentiveEarnings(target, stored);
                if (hiringIncentives.Any())
                {
                    results.Add((target, hiringIncentives));
                }
            }

            return results;
        }

        private static List<IncentiveEarning> MapHiringIncentiveEarnings(ApprenticeshipContractType1EarningEvent act1Event, GrowthAndSkillsEarningModel storedEarnings)
        {
            var collectionPeriod = act1Event.CollectionPeriod;

            return storedEarnings.PricePeriods
                .Where(x => x.AcademicYear == collectionPeriod.AcademicYear && x.EmployerType == EmployerType.NonLevy)
                .GroupBy(x => (IncentiveEarningType)(byte)x.EarningType)
                .Select(group => new IncentiveEarning
                {
                    Type = group.Key,
                    Periods = new ReadOnlyCollection<Common.EarningPeriod>(group
                        .OrderBy(x => x.DeliveryPeriod)
                        .Select(x => MapEarningPeriod(x, act1Event))
                        .ToList())
                })
                .ToList();
        }

        private static Common.EarningPeriod MapEarningPeriod(GrowthAndSkillsEarningPricePeriodModel stored, ApprenticeshipContractType1EarningEvent act1Event)
        {
            return new Common.EarningPeriod
            {
                Period = stored.DeliveryPeriod,
                Amount = stored.Amount,
                PriceEpisodeIdentifier = FindPriceEpisodeIdentifier(act1Event, stored),
                AccountId = stored.EmployerAccountId,
                ApprenticeshipId = stored.ApprenticeshipId,
                ApprenticeshipEmployerType = (Common.Entities.ApprenticeshipEmployerType)(byte)stored.EmployerType,
                SfaContributionPercentage = 1m,
                TransferSenderAccountId = stored.FundingAccountId != stored.EmployerAccountId ? stored.FundingAccountId : (long?)null
            };
        }

        private static string FindPriceEpisodeIdentifier(ApprenticeshipContractType1EarningEvent act1Event, GrowthAndSkillsEarningPricePeriodModel stored)
        {
            var priceEpisodes = act1Event.PriceEpisodes ?? new List<Common.PriceEpisode>();
            var periodStart = DeliveryPeriodStart(stored.AcademicYear, stored.DeliveryPeriod);

            var covering = priceEpisodes
                .Where(x => x.StartDate <= periodStart)
                .OrderByDescending(x => x.StartDate)
                .FirstOrDefault();

            return (covering ?? priceEpisodes.OrderBy(x => x.StartDate).FirstOrDefault())?.Identifier;
        }

        private static DateTime DeliveryPeriodStart(short academicYear, byte deliveryPeriod)
        {
            var startYear = 2000 + academicYear / 100;
            return new DateTime(startYear, 8, 1).AddMonths(deliveryPeriod - 1);
        }
    }
}
