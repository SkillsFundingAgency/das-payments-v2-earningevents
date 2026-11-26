using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core;
using UUIDNext;
using CourseType = SFA.DAS.Payments.EarningEvents.Model.CourseType;
using LearningType = SFA.DAS.Payments.EarningEvents.Model.LearningType;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Processors;

public class WithholdPaymentsForLearningMessageProcessor : IIncomingEarningsMessageProcessor
{
    private readonly IEarningsRepository _repository;
    private readonly ICollectionPeriodService _collectionPeriodService;
    private readonly IPaymentsServiceBusPublisher _publisher;
    private readonly ILogger<WithholdPaymentsForLearningMessageProcessor> _logger;

    public WithholdPaymentsForLearningMessageProcessor(
        IEarningsRepository repository,
        ICollectionPeriodService collectionPeriodService,
        IPaymentsServiceBusPublisher publisher,
        ILogger<WithholdPaymentsForLearningMessageProcessor> logger)
    {
        _repository = repository;
        _collectionPeriodService = collectionPeriodService;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Process(BinaryData messageBody)
    {
        var message = messageBody.ToObjectFromJson<WithholdPaymentsForLearningEvent>()
            ?? throw new InvalidOperationException("The WithholdPaymentsForLearningEvent message body is empty.");

        var openCollectionPeriods = await _collectionPeriodService.GetOpenCollectionPeriods();

        try
        {
            if (!openCollectionPeriods.Any())
            {
                throw new InvalidOperationException(
                    "WithholdPaymentsForLearningEvent received when there are no open collection periods.");
            }

            await _repository.InvalidateEarnings(
                message.LearningKey,
                message.UKPRN,
                message.ULN,
                message.CourseCode,
                (CourseType)message.CourseType,
                (LearningType)message.LearningType);

            var temporaryEarningsId = Uuid.NewDatabaseFriendly(Database.SqlServer);

            foreach (var period in openCollectionPeriods)
            {
                await _publisher.Publish(new DasEarningsReceivedEvent
                {
                    EarningsId = temporaryEarningsId,
                    UKPRN = message.UKPRN,
                    ULN = message.ULN,
                    CourseCode = message.CourseCode,
                    LearningAimReference = message.LearningAimReference,
                    CollectionPeriod = new CollectionPeriod
                    {
                        AcademicYear = period.AcademicYear,
                        Period = period.Period
                    }
                });
            }

        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(
                ex,
                "Unable to process WithholdPaymentsForLearningEvent" +
                "LearningKey: {LearningKey}, UKPRN: {UKPRN}, ULN: {ULN}, CourseCode: {CourseCode}",
                message.LearningKey,
                message.UKPRN,
                message.ULN,
                message.CourseCode);

            throw;
        }
    }
}
