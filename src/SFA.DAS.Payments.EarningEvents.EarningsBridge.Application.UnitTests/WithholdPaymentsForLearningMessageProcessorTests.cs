using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Processors;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core.Entities;
using UUIDNext.Tools;
using ExternalCourseType = SFA.DAS.Payments.EarningEvents.Messages.External.CourseType;
using ExternalLearningType = SFA.DAS.Payments.EarningEvents.Messages.External.LearningType;
using ModelCourseType = SFA.DAS.Payments.EarningEvents.Model.CourseType;
using ModelLearningType = SFA.DAS.Payments.EarningEvents.Model.LearningType;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.UnitTests;

[TestFixture]
public class WithholdPaymentsForLearningMessageProcessorTests
{
    private WithholdPaymentsForLearningEvent _message;
    private Mock<IEarningsRepository> _repository;
    private Mock<ICollectionPeriodService> _collectionPeriods;
    private Mock<IPaymentsServiceBusPublisher> _publisher;
    private Mock<ILogger<WithholdPaymentsForLearningMessageProcessor>> _logger;
    private List<DasEarningsReceivedEvent> _published;
    private WithholdPaymentsForLearningMessageProcessor _processor;

    [SetUp]
    public void SetUp()
    {
        _message = new WithholdPaymentsForLearningEvent
        {
            UKPRN = 10001234,
            ULN = 1234567890,
            LearningKey = Guid.NewGuid(),
            CourseCode = "123",
            LearningAimReference = "ZPROG001",
            CourseType = ExternalCourseType.Apprenticeship,
            LearningType = ExternalLearningType.Apprenticeship
        };

        _repository = new Mock<IEarningsRepository>();
        _repository.Setup(x => x.InvalidateEarnings(
                It.IsAny<Guid>(),
                It.IsAny<long>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<ModelCourseType>(),
                It.IsAny<ModelLearningType>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _collectionPeriods = new Mock<ICollectionPeriodService>();
        _collectionPeriods.Setup(x => x.GetOpenCollectionPeriods()).ReturnsAsync(new[]
        {
            new CollectionPeriodModel { AcademicYear = 2526, Period = 1 },
            new CollectionPeriodModel { AcademicYear = 2526, Period = 2 }
        });

        _published = new List<DasEarningsReceivedEvent>();
        _publisher = new Mock<IPaymentsServiceBusPublisher>();
        _publisher.Setup(x => x.Publish(It.IsAny<DasEarningsReceivedEvent>()))
            .Callback<DasEarningsReceivedEvent>(_published.Add)
            .Returns(Task.CompletedTask);

        _logger = new Mock<ILogger<WithholdPaymentsForLearningMessageProcessor>>();

        _processor = new WithholdPaymentsForLearningMessageProcessor(
            _repository.Object,
            _collectionPeriods.Object,
            _publisher.Object,
            _logger.Object);
    }


    [Test]
    public async Task PublishesOneDasEarningsReceivedEventPerOpenPeriodUsingTheEarningsId()
    {
        // Arrange
        var messageBody = BinaryData.FromObjectAsJson(_message);

        // Act
        await _processor.Process(messageBody);

        // Assert
        _repository.Verify(x => x.InvalidateEarnings(
            _message.LearningKey,
            _message.UKPRN,
            _message.ULN,
            _message.CourseCode,
            (ModelCourseType)_message.CourseType,
            (ModelLearningType)_message.LearningType,
            _message.LearningAimReference), Times.Once);

        _published.Should().HaveCount(2);
        _published.Select(x => x.EarningsId).Distinct().Should().ContainSingle();

        _published.Select(x => (x.CollectionPeriod.AcademicYear, x.CollectionPeriod.Period))
            .Should().BeEquivalentTo(((short)2526, (byte)1), ((short)2526, (byte)2));
        
        _published.Should().OnlyContain(x =>
            x.UKPRN == _message.UKPRN
            && x.ULN == _message.ULN
            && x.CourseCode == _message.CourseCode
            && x.LearningAimReference == _message.LearningAimReference);
    }

    [Test]
    public async Task InvalidatesButDoesNotPublishWhenThereAreNoOpenPeriods()
    {
        // Arrange
        _collectionPeriods.Setup(x => x.GetOpenCollectionPeriods())
            .ReturnsAsync(Array.Empty<CollectionPeriodModel>());

        // Act
        await _processor.Process(BinaryData.FromObjectAsJson(_message));

        // Assert
        _repository.Verify(x => x.InvalidateEarnings(
            It.IsAny<Guid>(),
            It.IsAny<long>(),
            It.IsAny<long>(),
            It.IsAny<string>(),
            It.IsAny<ModelCourseType>(),
            It.IsAny<ModelLearningType>(),
            It.IsAny<string>()), Times.Once);

        _publisher.Verify(x => x.Publish(
            It.IsAny<DasEarningsReceivedEvent>()), Times.Never);

        _published.Should().BeEmpty();
    }

    [Test]
    public async Task LogsWarningWhenThereAreNoOpenPeriods()
    {
        // Arrange
        _collectionPeriods.Setup(x => x.GetOpenCollectionPeriods())
            .ReturnsAsync(Array.Empty<CollectionPeriodModel>());

        // Act
        Func<Task> act = () =>
            _processor.Process(BinaryData.FromObjectAsJson(_message));

        // Assert
        await act.Should().NotThrowAsync();

        _logger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((value, type) =>
                    value.ToString().Contains("WithholdPaymentsForLearningEvent received when there are no open collection periods.")
                    && value.ToString().Contains(_message.LearningKey.ToString())
                    && value.ToString().Contains(_message.UKPRN.ToString())
                    && value.ToString().Contains(_message.ULN.ToString())
                    && value.ToString().Contains(_message.CourseCode)
                    && value.ToString().Contains(_message.LearningAimReference)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task LogsErrorAndDoesNotPublishWhenInvalidatingEarningsFails()
    {
        // Arrange
        var exception = new InvalidOperationException(
            "Unable to invalidate earnings.");

        _repository.Setup(x => x.InvalidateEarnings(
                It.IsAny<Guid>(),
                It.IsAny<long>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<ModelCourseType>(),
                It.IsAny<ModelLearningType>(),
                It.IsAny<string>()))
            .ThrowsAsync(exception);

        // Act
        Func<Task> act = () =>
            _processor.Process(BinaryData.FromObjectAsJson(_message));

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(exception.Message);

        _logger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((value, type) =>
                    value.ToString().Contains("Unable to process WithholdPaymentsForLearningEvent")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _collectionPeriods.Verify(x => x.GetOpenCollectionPeriods(),
            Times.Never);

        _publisher.Verify(x => x.Publish(
            It.IsAny<DasEarningsReceivedEvent>()), Times.Never);
    }

    [Test]
    public async Task LogsErrorAndDoesNotPublishWhenCollectionPeriodServiceThrowsException()
    {
        // Arrange
        var exception = new HttpRequestException(
            "Unable to retrieve open collection periods.");

        _collectionPeriods.Setup(x => x.GetOpenCollectionPeriods())
            .ThrowsAsync(exception);

        // Act
        Func<Task> act = () =>
            _processor.Process(BinaryData.FromObjectAsJson(_message));

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage(exception.Message);

        _logger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((value, type) =>
                    value.ToString().Contains("Unable to process WithholdPaymentsForLearningEvent")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _publisher.Verify(x => x.Publish(
            It.IsAny<DasEarningsReceivedEvent>()), Times.Never);

        _published.Should().BeEmpty();
    }

    [Test]
    public async Task LogsErrorWhenPublishingFails()
    {
        // Arrange
        var exception = new InvalidOperationException("Unable to publish event.");

        _publisher.Setup(x => x.Publish(It.IsAny<DasEarningsReceivedEvent>()))
            .ThrowsAsync(exception);

        // Act
        Func<Task> act = () =>
            _processor.Process(BinaryData.FromObjectAsJson(_message));

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(exception.Message);

        _logger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((value, type) =>
                    value.ToString().Contains("Unable to process WithholdPaymentsForLearningEvent")
                    && value.ToString().Contains(_message.LearningKey.ToString())
                    && value.ToString().Contains(_message.UKPRN.ToString())
                    && value.ToString().Contains(_message.ULN.ToString())
                    && value.ToString().Contains(_message.CourseCode)
                    && value.ToString().Contains(_message.LearningAimReference)),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
