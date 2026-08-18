using FluentAssertions;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Processors;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.UnitTests
{
    [TestFixture]
    public class UnsupportedLearningTypeProcessorTests
    {
        private UnsupportedLearningTypeProcessor _sut;
        private CalculateGrowthAndSkillsPayments _message;
        private IEnumerable<CollectionPeriodModel> _openCollectionPeriods;

        [SetUp]
        public void SetUp()
        {
            _sut = new UnsupportedLearningTypeProcessor();
            _message = new CalculateGrowthAndSkillsPayments();
            _openCollectionPeriods = Array.Empty<CollectionPeriodModel>();
        }

        [Test]
        public async Task Process_Returns_CompletedTask_And_Does_Not_Throw()
        {
            var result = _sut.Process(_message, _openCollectionPeriods);

            result.Should().BeSameAs(Task.CompletedTask);

            await result;
        }
    }
}
