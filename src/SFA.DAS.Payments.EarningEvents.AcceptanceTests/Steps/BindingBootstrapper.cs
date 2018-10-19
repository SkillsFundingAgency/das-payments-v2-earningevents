﻿using System;
using System.Configuration;
using Autofac;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.IO.Redis;
using ESFA.DC.IO.Redis.Config;
using ESFA.DC.IO.Redis.Config.Interfaces;
using ESFA.DC.Serialization.Interfaces;
using ESFA.DC.Serialization.Json;
using NServiceBus;
using SFA.DAS.Payments.AcceptanceTests.Core;
using SFA.DAS.Payments.Core.Configuration;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.Internal.Commands;
using SFA.DAS.Payments.Messages.Core;
using TechTalk.SpecFlow;

namespace SFA.DAS.Payments.EarningEvents.AcceptanceTests.Steps
{
    [Binding]
    public class BindingBootstrapper : StepsBase
    {
        public BindingBootstrapper(ScenarioContext scenarioContext) : base(scenarioContext)
        {
        }

        [BeforeTestRun(Order = 51)]
        public static void AddRoutingConfig()
        {
            var endpointConfiguration = Container.Resolve<EndpointConfiguration>();
            endpointConfiguration.Conventions().DefiningEventsAs(type => type.IsEvent<ApprenticeshipContractType2EarningEvent>());
            var transportConfig = Container.Resolve<TransportExtensions<AzureServiceBusTransport>>();
            var routing = transportConfig.Routing();
            routing.RouteToEndpoint(typeof(ProcessLearnerCommand), EndpointNames.EarningEventsService);
        }

        [BeforeTestRun(Order = 2)]
        public static void AddDcConfig()
        {
            Builder.RegisterType<JsonSerializationService>().As<IJsonSerializationService>();
            Builder.Register(c => new RedisKeyValuePersistenceServiceConfig
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["AzureRedisConnectionString"].ConnectionString,
                KeyExpiry = new TimeSpan(14, 0, 0, 0)
            }).As<IRedisKeyValuePersistenceServiceConfig>().SingleInstance();

            Builder.RegisterType<RedisKeyValuePersistenceService>().As<IKeyValuePersistenceService>()
                .InstancePerLifetimeScope();
        }
    }
}
