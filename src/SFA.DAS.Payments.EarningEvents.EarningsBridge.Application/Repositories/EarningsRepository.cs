using SFA.DAS.Payments.EarningEvents.Data;
using SFA.DAS.Payments.EarningEvents.Model;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories
{
    public class EarningsRepository : IEarningsRepository
    {
        private readonly IEarningsDataContext earningsDataContext;
        
        public EarningsRepository(IEarningsDataContext earningsDataContext)
        {
            this.earningsDataContext = earningsDataContext; 
        }

        public void SaveEarnings(GrowthAndSkillsEarningModel growthAndSkillsEarningModel)
        {
            //null check?
            earningsDataContext.ShortCourseEarnings.Add(growthAndSkillsEarningModel);
            earningsDataContext.SaveChanges(); 

            //var collectionPeriods = paymentsDataContext.CollectionPeriod.ToList();//.Select(cp=>new{cp.AcademicYear,cp.Period});
            //gets current collection period from database
            //var collectionPeriods2 = paymentsDataContext.CollectionPeriod.Where(e => e.AcademicYear == 2425);
        }
    }
}
