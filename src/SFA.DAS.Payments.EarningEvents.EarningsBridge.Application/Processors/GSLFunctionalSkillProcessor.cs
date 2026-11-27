using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Mapping;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Processors
{
    public class GSLFunctionalSkillProcessor : IGSLProcessor
    {
        private ILogger<GSLFunctionalSkillProcessor> logger;
        private IGSLFunctionalSkillMapper mapper;
        private IPaymentsServiceBusPublisher messagePublisher;

        public GSLFunctionalSkillProcessor(ILogger<GSLFunctionalSkillProcessor> logger, IGSLFunctionalSkillMapper mapper, IPaymentsServiceBusPublisher messagePublisher)
        {   
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper)); 
            this.messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher)); ;
        }

        public async Task Process(CalculateGrowthAndSkillsPayments message, IEnumerable<CollectionPeriodModel> openCollectionPeriods)
        {
            logger.LogDebug($"Processing Functional Skills CalculateGrowthAndSkillsPayments message.  Earning Id: {message.EarningsId}, Learner Id: {message.Learner.LearnerKey}, Course Code: {message.Training.CourseCode}");
            var functionalSKillEvents = new List<GSLFunctionalSkillEarningsEvent>();
            foreach (var collectionPeriod in openCollectionPeriods) 
            {
                logger.LogDebug($"Creating functional skill earning events for collection period: {collectionPeriod.Period}-{collectionPeriod.AcademicYear}");
                var functionalSKillEvent = new GSLFunctionalSkillEarningsEvent();
                mapper.Map(message, collectionPeriod, functionalSKillEvent);
                logger.LogTrace($"Finished mapping the functional skill event. Now publishing. Event id: {functionalSKillEvent.EventId}");
                functionalSKillEvents.Add(functionalSKillEvent);
            }

            var earningsReceivedEvents = mapper.MapToDasEarningsReceivedEvents(message, openCollectionPeriods);
            foreach (var earningsEvent in functionalSKillEvents)
            {
                try 
                {
                    var earningsReceivedEvent = earningsReceivedEvents
                        .FirstOrDefault(e => e.CollectionPeriod.Period == earningsEvent.CollectionPeriod.Period &&
                            e.CollectionPeriod.AcademicYear == earningsEvent.CollectionPeriod.AcademicYear) ?? throw new InvalidOperationException($"Couldn't find matching DAS Earnings Received event for collection period: {earningsEvent.CollectionPeriod.Period}-{earningsEvent.CollectionPeriod.AcademicYear}");
                    logger.LogTrace($"Now publishing functional skill event. Event id: {earningsEvent.EventId}");
                    await messagePublisher.Publish(earningsEvent);
                    await messagePublisher.Publish(earningsReceivedEvent);
                }
                catch (Exception ex) 
                { 
                    logger.LogError(ex, $"Error publishing functional skill event for collection period: {earningsEvent.CollectionPeriod.Period}-{earningsEvent.CollectionPeriod.AcademicYear}. Event id: {earningsEvent.EventId}");
                    throw;
                }
            }     
            

            logger.LogInformation($"Finished processing the Functional Skill CalculateGrowthAndSkillsPayments message. Earning Id: {message.EarningsId}, Learner Id: {message.Learner.LearnerKey}, Course Code: {message.Training.CourseCode}");
        }
    }
}
