using System.Linq;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.EarningEvents.Data;
using SFA.DAS.Payments.EarningEvents.Model;

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
            return earningsDataContext.GrowthAndSkillsEarnings
                .Where(x => x.UKPRN == ukprn && x.LearnerUln == uln && x.CourseCode == courseCode)
                .OrderByDescending(x => x.Id)
                .Include(x => x.PricePeriods.Where(p =>
                    p.EarningType == EarningType.FirstEmployerHiringPayment ||
                    p.EarningType == EarningType.SecondEmployerHiringPayment))
                .FirstOrDefault();
        }
    }
}
