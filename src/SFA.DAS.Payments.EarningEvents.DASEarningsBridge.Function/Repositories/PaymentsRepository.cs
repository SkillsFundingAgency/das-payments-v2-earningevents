using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.Payments.Application.Repositories;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Function.Repositories
{
    public class PaymentsRepository
    {
        private readonly IPaymentsDataContext paymentsDataContext;

        public PaymentsRepository(IPaymentsDataContext paymentsDataContext)
        {
            this.paymentsDataContext = paymentsDataContext; 
        }

        public void GetCollectionPeriod()
        {
            //var collectionPeriods = paymentsDataContext.CollectionPeriod.ToList();//.Select(cp=>new{cp.AcademicYear,cp.Period});
            //gets current collection period from database
            //var collectionPeriods2 = paymentsDataContext.CollectionPeriod.Where(e => e.AcademicYear == 2425);
        }
    }
}
