
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.EarningEvents.Model;

namespace SFA.DAS.Payments.EarningEvents.Data
{
    public interface IEarningsDataContext
    {
        DbSet<GrowthAndSkillsEarningModel> ShortCourseEarnings { get; set; }
        DbSet<GrowthAndSkillsEarningPricePeriodModel> ShortCourseEarningPricePeriods { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
        int SaveChanges();
    }
}
