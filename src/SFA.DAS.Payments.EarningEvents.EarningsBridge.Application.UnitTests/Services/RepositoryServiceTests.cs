using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using UUIDNext.Tools;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.UnitTests.Services
{
    [TestFixture]
    public class RepositoryServiceTests
    {
        private IRepositoryService _service;

        [SetUp]
        public void Setup()
        {
            _service = new RepositoryService();
        }

        [Test]
        public void CheckEarningsAreLatest_WhenMessageTimestamp_IsLatest_ShouldReturnTrue()
        {
            // Arrange
            var timestamp = DateTime.Now;
            var messageTimestamp = UuidToolkit.CreateUuidV7FromSpecificDate(timestamp);
            var tableEntryTimestamp = UuidToolkit.CreateUuidV7FromSpecificDate(timestamp.AddDays(-1));

            // Act
            var result = _service.CheckEarningsAreLatest(messageTimestamp, tableEntryTimestamp);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void CheckEarningsAreLatest_WhenMessageTimestamp_IsEarlier_ShouldReturnFalse()
        {
            // Arrange
            var timestamp = DateTime.Now;
            var messageTimestamp = UuidToolkit.CreateUuidV7FromSpecificDate(timestamp.AddDays(-1));
            var tableEntryTimestamp = UuidToolkit.CreateUuidV7FromSpecificDate(timestamp);

            // Act
            var result = _service.CheckEarningsAreLatest(messageTimestamp, tableEntryTimestamp);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void CheckEarningsAreLatest_WhenMessageTimestamp_IsEqual_ShouldReturnFalse()
        {
            // Arrange
            var timestamp = DateTime.Now;
            var messageTimestamp = UuidToolkit.CreateUuidV7FromSpecificDate(timestamp);
            var tableEntryTimestamp = UuidToolkit.CreateUuidV7FromSpecificDate(timestamp);

            // Act
            var result = _service.CheckEarningsAreLatest(messageTimestamp, tableEntryTimestamp);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void CheckEarningsAreLatest_WhenMessageTimestamp_CannotBeDecoded_ShouldReturnFalse()
        {
            // Arrange
            var messageTimestamp = Guid.Empty;
            var tableEntryTimestamp = Guid.Empty;

            // Act
            var result = _service.CheckEarningsAreLatest(messageTimestamp, tableEntryTimestamp);

            // Assert
            result.Should().BeFalse();
        }
    }
}
