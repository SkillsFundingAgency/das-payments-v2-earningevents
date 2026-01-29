
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.EarningEvents.Model;

namespace SFA.DAS.Payments.EarningEvents.Data
{
    public interface IEarningsDataContext
    {
        DbSet<ShortCourseEarningModel> ShortCourseEarnings { get; set; }
        DbSet<ShortCourseEarningPricePeriodModel> ShortCourseEarningPricePeriods { get; set; }
    }
}
