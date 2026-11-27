using System.Linq;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.EarningEvents.Data;
using SFA.DAS.Payments.EarningEvents.Domain;
using SFA.DAS.Payments.EarningEvents.Model;
using UUIDNext.Tools;

namespace SFA.DAS.Payments.EarningEvents.Application.Repositories
{
    public interface IHiringPaymentEarningsRepository
    {
        GrowthAndSkillsEarningModel GetLatestHiringPaymentEarnings(long ukprn, long uln, string courseCode);
    }

    public class HiringPaymentEarningsRepository : IHiringPaymentEarningsRepository
    {
        private readonly IEarningsDataContext earningsDataContext;

        public HiringPaymentEarningsRepository(IEarningsDataContext earningsDataContext)
        {
            this.earningsDataContext = earningsDataContext;
        }

        public GrowthAndSkillsEarningModel GetLatestHiringPaymentEarnings(long ukprn, long uln, string courseCode)
        {
            var candidates = earningsDataContext.GrowthAndSkillsEarnings
                .Where(x => x.UKPRN == ukprn && x.LearnerUln == uln && x.CourseCode == courseCode
                            && x.CourseType == CourseType.Apprenticeship
                            && x.StartDate >= HiringPaymentEligibility.StartDateFrom)
                .Include(x => x.PricePeriods.Where(p =>
                    p.EarningType == EarningType.FirstEmployerHiringPayment ||
                    p.EarningType == EarningType.SecondEmployerHiringPayment))
                .ToList();

            return candidates
                .Select(x => new { Earning = x, Decoded = UuidDecoder.TryDecodeTimestamp(x.EarningsId, out var timestamp), Timestamp = timestamp })
                .Where(x => x.Decoded)
                .OrderByDescending(x => x.Timestamp)
                .ThenByDescending(x => x.Earning.Id)
                .Select(x => x.Earning)
                .FirstOrDefault();
        }
    }
}
