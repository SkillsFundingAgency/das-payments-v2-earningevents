using System.Collections.Concurrent;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Specs.Models;

namespace SFA.DAS.Payments.EarningEvents.Specs.Handlers;

public class GSLShortCourseEarningsEventHandler: IHandleMessages<GSLShortCourseEarningsEvent>
{
    public static ConcurrentBag<GSLShortCourseEarningsEvent> ReceivedEvents { get; } =
        new ConcurrentBag<GSLShortCourseEarningsEvent>();

    public async Task Handle(GSLShortCourseEarningsEvent message, IMessageHandlerContext context)
    {
        Console.WriteLine($"Received short course earnings event: {message.Ukprn}, uln: {message.Learner.Uln}, return: {message.CollectionPeriod.AcademicYear}-{message.CollectionPeriod.Period}, Course: {message.LearningAim.LearningType} - {message.LearningAim.CourseCode}");
        ReceivedEvents.Add(message);
    }

    public static IEnumerable<GSLShortCourseEarningsEvent> GetEvents(Learner learner) => ReceivedEvents.Where(receivedEvent =>
        receivedEvent.Learner.Uln == learner.Uln
        && receivedEvent.Ukprn == learner.Ukprn
        && receivedEvent.Learner.ReferenceNumber == learner.LearnRefNumber);

}