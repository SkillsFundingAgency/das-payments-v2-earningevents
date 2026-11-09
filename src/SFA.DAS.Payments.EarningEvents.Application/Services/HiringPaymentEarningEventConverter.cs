using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using SFA.DAS.Payments.EarningEvents.Application.Mapping;
using SFA.DAS.Payments.EarningEvents.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.Domain.Mapping;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Model;
using SFA.DAS.Payments.Model.Core.Incentives;
using Common = SFA.DAS.Payments.Model.Core;

namespace SFA.DAS.Payments.EarningEvents.Application.Services
{
    public class HiringPaymentEarningEventConverter : IHiringPaymentEarningEventConverter
    {
        private readonly IHiringPaymentEarningsRepository repository;
        private readonly IApprenticeshipEmployerTypeRepository employerTypeRepository;
        private readonly IMapper mapper;

        public HiringPaymentEarningEventConverter(
            IHiringPaymentEarningsRepository repository,
            IApprenticeshipEmployerTypeRepository employerTypeRepository,
            IMapper mapper)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.employerTypeRepository = employerTypeRepository ?? throw new ArgumentNullException(nameof(employerTypeRepository));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public List<EarningEvent> Convert(List<EarningEvent> earningEvents)
        {
            return ConvertEligibleEventsToGsl(earningEvents).GetAwaiter().GetResult();
        }

        private async Task<List<EarningEvent>> ConvertEligibleEventsToGsl(List<EarningEvent> earningEvents)
        {
            var converted = new List<EarningEvent>();

            foreach (var earningEvent in earningEvents)
            {
                if (earningEvent is ApprenticeshipContractType1EarningEvent act1Event && await IsEligible(act1Event).ConfigureAwait(false))
                {
                    converted.Add(await ConvertToGslEvent(act1Event).ConfigureAwait(false));
                    continue;
                }

                converted.Add(earningEvent);
            }

            return converted;
        }

        private async Task<bool> IsEligible(ApprenticeshipContractType1EarningEvent act1Event)
        {
            if (!HiringPaymentEligibility.MeetsStartDateAndAge(act1Event))
            {
                return false;
            }

            var employerType = await employerTypeRepository
                .GetEmployerType(act1Event.Ukprn, act1Event.Learner.Uln, act1Event.LearningAim.StandardCode)
                .ConfigureAwait(false);

            return employerType == Common.Entities.ApprenticeshipEmployerType.NonLevy;
        }

        private async Task<GSLApprenticeshipEarningsEvent> ConvertToGslEvent(ApprenticeshipContractType1EarningEvent act1Event)
        {
            var storedEarnings = await repository
                .GetLatestHiringPaymentEarnings(act1Event.Ukprn, act1Event.Learner.Uln, act1Event.LearningAim.StandardCode.ToString())
                .ConfigureAwait(false);

            var gslEvent = mapper.Map<GSLApprenticeshipEarningsEvent>(act1Event);
            gslEvent.ExternalEarningsId = storedEarnings?.EarningsId ?? Guid.Empty;
            gslEvent.IncentiveEarnings = (act1Event.IncentiveEarnings ?? new List<IncentiveEarning>())
                .Concat(MapHiringIncentiveEarnings(act1Event, storedEarnings))
                .ToList();

            return gslEvent;
        }

        private static List<IncentiveEarning> MapHiringIncentiveEarnings(ApprenticeshipContractType1EarningEvent act1Event, GrowthAndSkillsEarningModel storedEarnings)
        {
            if (storedEarnings == null)
            {
                return new List<IncentiveEarning>();
            }

            var collectionPeriod = act1Event.CollectionPeriod;

            return storedEarnings.PricePeriods
                .Where(x => x.AcademicYear == collectionPeriod.AcademicYear)
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
