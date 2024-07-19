using System;
using System.Threading;
using SFA.DAS.Payments.ServiceFabric.Core.Infrastructure.Ioc;

<<<<<<<< HEAD:src/SFA.DAS.Payments.EarningEvents.EarningEventsService2425/Program.cs
namespace SFA.DAS.Payments.EarningEvents.EarningEventsService2425
========
namespace SFA.DAS.Payments.EarningEvents.EarningEventsService2324
>>>>>>>> 841fb31 (Merge pull request #1202 from SkillsFundingAgency/PV2-3299_earning_event_2425_AY_service):src/SFA.DAS.Payments.EarningEvents.EarningEventsService2324/Program.cs
{
    internal static class Program
    {
        private static void Main()
        {
            try
            {
<<<<<<<< HEAD:src/SFA.DAS.Payments.EarningEvents.EarningEventsService2425/Program.cs
                using (ServiceFabricContainerFactory.CreateContainerForStatelessService<EarningEvents.EarningEventsService2425.EarningEventsService2425>())
========
                using (ServiceFabricContainerFactory.CreateContainerForStatelessService<EarningEventsService2324>())
>>>>>>>> 841fb31 (Merge pull request #1202 from SkillsFundingAgency/PV2-3299_earning_event_2425_AY_service):src/SFA.DAS.Payments.EarningEvents.EarningEventsService2324/Program.cs
                {
                    Thread.Sleep(Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
