using SFA.DAS.Payments.EarningEvents.Data;
using SFA.DAS.Payments.EarningEvents.Model;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories
{
    public class EarningsRepository
    {
        private readonly IEarningsDataContext earningsDataContext;
        
        //needs to add shortcoursearningsmodule

        public EarningsRepository(IEarningsDataContext earningsDataContext)
        {
            this.earningsDataContext = earningsDataContext; 
        }

        public void SaveEarningsToDatabase(ShortCourseEarningModel shortCourseEarningModel)
        {
            //responsible for 

            earningsDataContext.ShortCourseEarnings.Add(shortCourseEarningModel);

            //var collectionPeriods = paymentsDataContext.CollectionPeriod.ToList();//.Select(cp=>new{cp.AcademicYear,cp.Period});
            //gets current collection period from database
            //var collectionPeriods2 = paymentsDataContext.CollectionPeriod.Where(e => e.AcademicYear == 2425);
        }
    }
}
