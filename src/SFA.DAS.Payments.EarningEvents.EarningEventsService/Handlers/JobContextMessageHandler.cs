﻿using System;
using System.Threading.Tasks;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.JobContextManager.Model;
using ESFA.DC.Serialization.Interfaces;
using NServiceBus;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.EarningEvents.Application.Interfaces;

namespace SFA.DAS.Payments.EarningEvents.EarningEventsService.Handlers
{
    public class JobContextMessageHandler: IHandleMessages<JobContextMessage>
    {
        private readonly IPaymentLogger paymentLogger;
        private readonly IEarningEventsProcessingService earningEventsProcessingService;
        private readonly IKeyValuePersistenceService redisService;
        private readonly IJsonSerializationService serializationService;

        public JobContextMessageHandler(IPaymentLogger paymentLogger, 
            IEarningEventsProcessingService earningEventsProcessingService,
            IKeyValuePersistenceService redisService,
            IJsonSerializationService serializationService)
        {
            this.paymentLogger = paymentLogger;
            this.earningEventsProcessingService = earningEventsProcessingService;
            this.redisService = redisService;
            this.serializationService = serializationService;
        }
        public async Task Handle(JobContextMessage message, IMessageHandlerContext context)
        {
            paymentLogger.LogInfo($"Processing Earning Event Service event for Message Id : {context.MessageId}");

            var currentExecutionContext = context as ESFA.DC.Logging.ExecutionContext;
            currentExecutionContext.JobId = message.JobId.ToString();

            try
            {
                var fm36Output = serializationService.Deserialize<FM36Global>(redisService
                    .GetAsync(message.KeyValuePairs["FundingFm36Output"].ToString()).Result);

                var earningEvents = earningEventsProcessingService.GetEarningEvents(fm36Output);

                foreach (var earningEvent in earningEvents)
                {
                    try
                    {
                        await context.Publish(earningEvent);

                        paymentLogger.LogInfo($"Successfully published EarningEvent");
                    }
                    catch (Exception ex)
                    {
                        paymentLogger.LogError($"Error publishing the event: EarningEvent", ex);
                        throw;
                    }

                    paymentLogger.LogInfo($"Successfully processed Earning event for Job Id {message.JobId}");
                }
            }
            catch (Exception ex)
            {
                paymentLogger.LogError($"Error while handling EarningService event", ex);
                throw;
            }
            
        }
    }
}
