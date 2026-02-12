using Autofac;
using SFA.DAS.Payments.Core.Configuration;
using SFA.DAS.Payments.EarningEvents.Data;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Handlers;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Validators;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Infrastructure.Ioc
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
            // TODO: move to Microsoft DI in Earning Bridge Azure function project
            builder.RegisterType<GSLEarningsMapper>()
                .As<IGSLEarningsMapper>()
                .InstancePerLifetimeScope();
            builder.RegisterType<GSLCalculatePaymentsHandler>()
                .As<IGSLCalculatePaymentsHandler>()
                .InstancePerLifetimeScope();
            builder.RegisterType<CalculateGSLPaymentsValidator>()
                .As<ICalculateGSLPaymentsValidator>()
                .InstancePerLifetimeScope();
            builder.RegisterType<EarningsRepository>()
                .As<IEarningsDataContext>()
                .InstancePerLifetimeScope();
        }


    }
}
