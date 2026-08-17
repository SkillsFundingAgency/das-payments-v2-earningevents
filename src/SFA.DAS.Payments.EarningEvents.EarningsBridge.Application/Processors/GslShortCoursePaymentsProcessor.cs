using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Mapping;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Processors
{
    public class GslShortCoursePaymentsProcessor : IGslProcessor
    {
        private readonly IGslShortCoursesMapper _mapper;
        private readonly IPaymentsServiceBusPublisher _publisher;

        public GslShortCoursePaymentsProcessor(
            IGslShortCoursesMapper mapper, 
            IPaymentsServiceBusPublisher publisher
            )
        {
            _mapper = mapper;
            _publisher = publisher;
        }


        public async Task Process(CalculateGrowthAndSkillsPayments message, IEnumerable<CollectionPeriodModel> openCollectionPeriods)
        {
            var requiredPaymentsEvents = _mapper.MapToShortCourseEarningEvents(message, openCollectionPeriods);

            if (requiredPaymentsEvents is null || !requiredPaymentsEvents.Any())
            {
                return;
            }

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
