using Autofac;
using SFA.DAS.Payments.Core.Configuration;
using SFA.DAS.Payments.EarningEvents.Data;

namespace SFA.DAS.Payments.EarningEvents.Application.Infrastructure.Ioc
{
    public class ShortCourseEarningsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register((c, p) =>
                {
                    var configHelper = c.Resolve<IConfigurationHelper>();
                    return new EarningsDataContext(configHelper.GetConnectionString("PaymentsConnectionString"));
                })
                .As<IEarningsDataContext>()
                .InstancePerLifetimeScope();
        }
    }
}
