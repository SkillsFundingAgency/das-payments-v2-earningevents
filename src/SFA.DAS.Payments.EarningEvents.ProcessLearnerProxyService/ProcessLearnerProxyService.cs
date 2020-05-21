using System.Collections.Generic;
using System.Fabric;
using Autofac;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.ServiceFabric.Core;

namespace SFA.DAS.Payments.EarningEvents.ProcessLearnerProxyService
{
    internal sealed class ProcessLearnerProxyService : StatelessService
    {
        private IStatelessEndpointCommunicationListener listener;
        private readonly ILifetimeScope lifetimeScope;
        private readonly IPaymentLogger paymentLogger;

        public ProcessLearnerProxyService(StatelessServiceContext context, ILifetimeScope lifetimeScope, IPaymentLogger paymentLogger) : base(context)
        {
            this.lifetimeScope = lifetimeScope;
            this.paymentLogger = paymentLogger;
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            paymentLogger.LogInfo($"Creating Service Instance Listeners For {nameof(ProcessLearnerProxyService)}");

            return new List<ServiceInstanceListener>
            {
                new ServiceInstanceListener(context =>listener = lifetimeScope.Resolve<IStatelessEndpointCommunicationListener>())
            };
        }
    }
}
