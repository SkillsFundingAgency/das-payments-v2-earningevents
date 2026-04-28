using SFA.DAS.Payments.EarningEvents.Specs.Data;
using SFA.DAS.Payments.EarningEvents.Specs.Models;

namespace SFA.DAS.Payments.EarningEvents.Specs.StepDefinitions
{
    public class ProviderService
    {
        private readonly TestSessionDataContext dataContext;
        private string appGuid;

        public ProviderService(TestSessionDataContext dataContext, string appGuid )
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.appGuid = appGuid;
        }

        public Provider GetProvider()
        {
            //string appGuid =
            //    ((GuidAttribute)Assembly.GetExecutingAssembly().
            //        GetCustomAttributes(typeof(GuidAttribute), false).
            //        GetValue(0)).Value.ToString();

            Provider provider = null;
            using (var mutex = new Mutex(false, $"Global\\{{{appGuid}}}"))
            {
                if (mutex.WaitOne(TimeSpan.FromMinutes(1)))
                {
                    provider = dataContext.LeastRecentlyUsed();
                    provider.Use();
                    dataContext.SaveChanges();
                    //var blockedList = jobService.GetJobsByStatus(provider.Ukprn, 2, 3).Result;
                    //if (blockedList.Any())
                    //{
                    //    provider = GetProvider();
                    //}

                    mutex.ReleaseMutex();
                }
                else
                {
                    throw new ApplicationException("Unable to obtain a Ukprn due to a locked Mutex");
                }
            }

            return provider;
        }
    }
}