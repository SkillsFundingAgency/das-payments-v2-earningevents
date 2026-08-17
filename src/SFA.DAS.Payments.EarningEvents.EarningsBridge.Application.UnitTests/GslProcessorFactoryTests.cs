using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Mapping;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Processors;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.Model;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.UnitTests
{
    [TestFixture]
    public class GslProcessorFactoryTests
    {
        private GslProcessorFactory _sut;
        private GslApprenticeshipPaymentsProcessor _apprenticeshipProcessor;
        private GslShortCoursePaymentsProcessor _shortCourseProcessor;
        private Mock<IServiceProvider> _serviceProvider;

        [SetUp]
        public void SetUp()
        {
            _apprenticeshipProcessor = new Mock<GslApprenticeshipPaymentsProcessor>(
                Mock.Of<IGslApprenticeshipsMapper>(),
                Mock.Of<IPaymentsServiceBusPublisher>()).Object;

            _shortCourseProcessor = new Mock<GslShortCoursePaymentsProcessor>(
                Mock.Of<IGslShortCoursesMapper>(),
                Mock.Of<IPaymentsServiceBusPublisher>()).Object;

            _serviceProvider = new Mock<IServiceProvider>();

            _serviceProvider
                .Setup(x => x.GetService(typeof(GslApprenticeshipPaymentsProcessor)))
                .Returns(_apprenticeshipProcessor);

            _serviceProvider
                .Setup(x => x.GetService(typeof(GslShortCoursePaymentsProcessor)))
                .Returns(_shortCourseProcessor);

            _sut = new GslProcessorFactory(_serviceProvider.Object);
        }

        [Test]
        public void CreateGSLProcessor_ReturnsApprenticeshipProcessor_ForApprenticeshipLearningType()
        {
            var result = _sut.CreateGslProcessor(LearningType.Apprenticeship);

            result.Should().Be(_apprenticeshipProcessor);
        }

        [Test]
        public void CreateGSLProcessor_ReturnsShortCourseProcessor_ForApprenticeshipUnitLearningType()
        {
            var result = _sut.CreateGslProcessor(LearningType.ApprenticeshipUnit);

            result.Should().Be(_shortCourseProcessor);
        }

        [Test]
        public void CreateGSLProcessor_ThrowsForUnsupportedLearningType()
        {
            var unsupportedLearningType = default(LearningType);
            var act = () => _sut.CreateGslProcessor(unsupportedLearningType);

            act.Should().Throw<NotSupportedException>()
                .WithMessage("Unsupported learning type: 0");
        }
    }
}