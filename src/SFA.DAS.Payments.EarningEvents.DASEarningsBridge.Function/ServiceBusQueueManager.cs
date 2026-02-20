
using Azure.Messaging.ServiceBus.Administration;
using Azure.Messaging.ServiceBus;
using Azure;
using System.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Infrastructure.Configuration;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Function
{
    public  class ServiceBusQueueManager : IHostedService
    {
        private readonly ILogger<ServiceBusQueueManager> _logger;
        private readonly IEarningsBridgeConfiguration _configuration;

        public ServiceBusQueueManager(ILogger<ServiceBusQueueManager> logger, IEarningsBridgeConfiguration configuration)
        {
            _logger = logger;   
            _configuration = configuration;
        }

        public async Task StartAsync(CancellationToken cancellationToken) 
        {
            var serviceBusConnectionString = _configuration.DASServiceBusConnectionString;
            var queueName = _configuration.DASServiceBusQueueName;
            
            try
            {
                var adminClient = new ServiceBusAdministrationClient(serviceBusConnectionString);

                if (await adminClient.QueueExistsAsync(queueName, CancellationToken.None).ConfigureAwait(false))
                {
                    _logger.LogInformation($"Queue '{queueName}' already exists, skipping queue creation.");
                    return;
                }

                var options = new CreateQueueOptions(queueName)
                {
                    DefaultMessageTimeToLive = TimeSpan.FromDays(7),
                    DeadLetteringOnMessageExpiration = true,
                    LockDuration = TimeSpan.FromMinutes(5),
                    MaxDeliveryCount = 50,
                    MaxSizeInMegabytes = 5120
                };

                await adminClient.CreateQueueAsync(options, CancellationToken.None).ConfigureAwait(false);

                _logger.LogInformation($"Queue '{queueName}' created.");
            }
            catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityAlreadyExists)
            {
                _logger.LogInformation($"Queue '{queueName}' already exists: {ex.Message}. Another instance likely created it.");
            }
            catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.Conflict)
            {
                _logger.LogInformation($"Queue '{queueName}' already exists (409): {ex.Message}. Another instance likely created it.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error ensuring queue '{queueName}': {ex.Message}.", ex);
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        
    }
}
