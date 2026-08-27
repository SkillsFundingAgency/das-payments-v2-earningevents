using System.Collections.Concurrent;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Specs.Models;

namespace SFA.DAS.Payments.EarningEvents.Specs.Handlers;

public class DasEarningsReceivedEventHandler: IHandleMessages<DasEarningsReceivedEvent>
{
    public static ConcurrentBag<DasEarningsReceivedEvent> ReceivedEvents { get; } =
        new ConcurrentBag<DasEarningsReceivedEvent>();

    public async Task Handle(DasEarningsReceivedEvent message, IMessageHandlerContext context)
    {
        Console.WriteLine("*****************************");
        Console.WriteLine(
            $"Received DAS earnings received event: {message.UKPRN}, uln: {message.ULN}, return: {message.CollectionPeriod.AcademicYear}-{message.CollectionPeriod.Period}");
        ReceivedEvents.Add(message);
    }

    public static IEnumerable<DasEarningsReceivedEvent> GetEvents(Learner learner) => ReceivedEvents.Where(receivedEvent =>
        receivedEvent.ULN == learner.Uln
        && receivedEvent.UKPRN == learner.Ukprn
        && receivedEvent.LearningAimReference == learner.LearnRefNumber);

}