using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Mapping;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Processors
{

    public class GSLShortCoursePaymentsProcessor : IGSLProcessor
    {
        private readonly IGSLShortCoursesMapper _mapper;
        private readonly IPaymentsServiceBusPublisher _publisher;

        public GSLShortCoursePaymentsProcessor(
            IGSLShortCoursesMapper mapper, 
            IPaymentsServiceBusPublisher publisher
            )
        {
            _mapper = mapper;
            _publisher = publisher;
        }


        public async Task Process(CalculateGrowthAndSkillsPayments message, IEnumerable<CollectionPeriodModel> openCollectionPeriods)
        {
            //TODO: This is NOT a required payment event.  Needs to be renamed.
            var requiredPaymentsEvents = _mapper.MapToShortCourseEarningEvents(message, openCollectionPeriods);

            if (requiredPaymentsEvents is null || !requiredPaymentsEvents.Any())
            {
                return;
            }

            //TODO: These are not funding source events.  Needs to be renamed.
            var fundingSourceEvents = _mapper.MapToDasEarningsReceivedEvents(message, openCollectionPeriods);

            foreach (var requiredPaymentsEvent in requiredPaymentsEvents)
            {
                await _publisher.Publish<GSLShortCourseEarningsEvent>(requiredPaymentsEvent);
            }

            foreach (var fundingSourceEvent in fundingSourceEvents)
            {
                await _publisher.Publish<DasEarningsReceivedEvent>(fundingSourceEvent);
            }

        }
    }
}
