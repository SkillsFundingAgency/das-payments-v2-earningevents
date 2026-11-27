using System.Collections.Concurrent;
using SFA.DAS.Payments.EarningEvents.Messages.Events;

namespace SFA.DAS.Payments.EarningEvents.Specs.Handlers;

public class DasEarningsReceivedEventHandler : IHandleMessages<DasEarningsReceivedEvent>
{
    public static ConcurrentBag<DasEarningsReceivedEvent> ReceivedEvents { get; } =
        new ConcurrentBag<DasEarningsReceivedEvent>();

    public Task Handle(DasEarningsReceivedEvent message, IMessageHandlerContext context)
    {
        Console.WriteLine($"Received DAS earnings received event: {message.UKPRN}, uln: {message.ULN}, return: {message.CollectionPeriod.AcademicYear}-{message.CollectionPeriod.Period}, Course: {message.CourseCode}");
        ReceivedEvents.Add(message);
        return Task.CompletedTask;
    }
}
