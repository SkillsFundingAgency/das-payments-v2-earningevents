using System.Collections.Concurrent;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Specs.Models;

namespace SFA.DAS.Payments.EarningEvents.Specs.Handlers;

public class ApprenticeshipContractType1EarningEventHandler : IHandleMessages<ApprenticeshipContractType1EarningEvent>
{
    public static ConcurrentBag<ApprenticeshipContractType1EarningEvent> ReceivedEvents { get; } =
        new ConcurrentBag<ApprenticeshipContractType1EarningEvent>();

    public async Task Handle(ApprenticeshipContractType1EarningEvent message, IMessageHandlerContext context)
    {
        Console.WriteLine($"Received ACT1 earnings event: {message.Ukprn}, uln: {message.Learner.Uln}, return: {message.CollectionPeriod.AcademicYear}-{message.CollectionPeriod.Period}");
        ReceivedEvents.Add(message);
        await Task.CompletedTask;
    }

    public static IEnumerable<ApprenticeshipContractType1EarningEvent> GetEvents(Learner learner) => ReceivedEvents.Where(receivedEvent =>
        receivedEvent.Learner.Uln == learner.Uln
        && receivedEvent.Ukprn == learner.Ukprn
        && receivedEvent.Learner.ReferenceNumber == learner.LearnRefNumber);
}
