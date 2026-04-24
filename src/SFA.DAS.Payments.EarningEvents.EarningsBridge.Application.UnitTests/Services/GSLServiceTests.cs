using FluentAssertions;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.Model;
using UUIDNext.Tools;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.UnitTests.Services
{
    [TestFixture]
    public class GslServiceTests
    {
        private IGSLEarningsService _service;

        [SetUp]
        public void Setup()
        {
            _service = new GSLEarningsService();
        }

        [Test]
        public void CheckEarningsAreLatest_WhenEmptyEarnings_ShouldReturnTrue()
        {
            // Arrange
            var timestamp = DateTime.Now;
            var messageTimestamp = UuidToolkit.CreateUuidV7FromSpecificDate(timestamp);

            var dbEntries = new List<GrowthAndSkillsEarningModel>();
            // Act
            var result = _service.CheckEarningsAreLatest(dbEntries, messageTimestamp);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void CheckEarningsAreLatest_WhenMessageTimestamp_IsLatest_ShouldReturnTrue()
        {
            // Arrange
            var timestamp = DateTime.Now;
            var messageTimestamp = UuidToolkit.CreateUuidV7FromSpecificDate(timestamp);

            var dbEntries = new List<GrowthAndSkillsEarningModel>
            {
                CreateGrowthAndSkills(UuidToolkit.CreateUuidV7FromSpecificDate(timestamp.AddDays(-1))),
                CreateGrowthAndSkills(UuidToolkit.CreateUuidV7FromSpecificDate(timestamp.AddDays(-2))),
                CreateGrowthAndSkills(UuidToolkit.CreateUuidV7FromSpecificDate(timestamp.AddDays(-3)))
            };

            // Act
            var result = _service.CheckEarningsAreLatest(dbEntries, messageTimestamp);

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

            var dbEntries = new List<GrowthAndSkillsEarningModel>
            {
                CreateGrowthAndSkills(tableEntryTimestamp),
                CreateGrowthAndSkills(UuidToolkit.CreateUuidV7FromSpecificDate(timestamp.AddDays(+1)))
            };

            // Act
            var result = _service.CheckEarningsAreLatest(dbEntries, messageTimestamp);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void CheckEarningsAreLatest_WhenMessageTimestamp_IsEqual_ShouldReturnFalse()
        {
            // Arrange
            var timestamp = DateTime.Now;
            var matchingTimestamp = UuidToolkit.CreateUuidV7FromSpecificDate(timestamp);

            var dbEntries = new List<GrowthAndSkillsEarningModel>
            {
                CreateGrowthAndSkills(matchingTimestamp)
            };

            // Act
            var result = _service.CheckEarningsAreLatest(dbEntries, matchingTimestamp);
            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void CheckEarningsAreLatest_WhenMessageTimestamp_CannotBeDecoded_ShouldReturnFalse()
        {
            // Arrange
            var messageTimestamp = Guid.Empty;
            var tableEntryTimestamp = Guid.Empty;

            var dbEntries = new List<GrowthAndSkillsEarningModel>
            {
                CreateGrowthAndSkills(tableEntryTimestamp)
            };


            // Act
            var result = _service.CheckEarningsAreLatest(dbEntries, messageTimestamp);

            // Assert
            result.Should().BeFalse();
        }

        private GrowthAndSkillsEarningModel CreateGrowthAndSkills(Guid earningsEventId)
        {
            return new GrowthAndSkillsEarningModel
            {
                EarningsId = earningsEventId
            };
        }
    }
}
