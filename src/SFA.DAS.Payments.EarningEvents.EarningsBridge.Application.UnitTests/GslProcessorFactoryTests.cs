using System;
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
    public class GSLProcessorFactoryTests
    {
        private GSLProcessorFactory _sut;
        private GSLApprenticeshipPaymentsProcessor _apprenticeshipProcessor;
        private GSLShortCoursePaymentsProcessor _shortCourseProcessor;
        private GSLFunctionalSkillProcessor _functionalSkillProcessor;
        private UnsupportedLearningTypeProcessor _unsupportedProcessor;
        private Mock<IServiceProvider> _serviceProvider;

        [SetUp]
        public void SetUp()
        {
            _apprenticeshipProcessor = new Mock<GSLApprenticeshipPaymentsProcessor>(
                Mock.Of<IGSLApprenticeshipsMapper>(),
                Mock.Of<IPaymentsServiceBusPublisher>()).Object;

            _shortCourseProcessor = new Mock<GSLShortCoursePaymentsProcessor>(
                Mock.Of<IGSLShortCoursesMapper>(),
                Mock.Of<IPaymentsServiceBusPublisher>()).Object;

            _functionalSkillProcessor = new Mock<GSLFunctionalSkillProcessor>().Object;

            _unsupportedProcessor = new UnsupportedLearningTypeProcessor();

            _serviceProvider = new Mock<IServiceProvider>();

            _serviceProvider
                .Setup(x => x.GetService(typeof(GSLApprenticeshipPaymentsProcessor)))
                .Returns(_apprenticeshipProcessor);

            _serviceProvider
                .Setup(x => x.GetService(typeof(GSLShortCoursePaymentsProcessor)))
                .Returns(_shortCourseProcessor);

            _serviceProvider
                .Setup(x => x.GetService(typeof(GSLFunctionalSkillProcessor)))
                .Returns(_functionalSkillProcessor);

            _serviceProvider
                .Setup(x => x.GetService(typeof(UnsupportedLearningTypeProcessor)))
                .Returns(_unsupportedProcessor);

            _sut = new GSLProcessorFactory(_serviceProvider.Object);
        }

        [Test]
        public void CreateGSLProcessor_Returns_ApprenticeshipProcessor_For_Apprenticeship_LearningType()
        {
            var result = _sut.CreateGSLProcessor(CourseType.Apprenticeship);

            result.Should().Be(_apprenticeshipProcessor);
        }

        [Test]
        public void CreateGSLProcessor_Returns_ShortCourseProcessor_For_ApprenticeshipUnit_LearningType()
        {
            var result = _sut.CreateGSLProcessor(CourseType.ShortCourse);

            result.Should().Be(_shortCourseProcessor);
        }

        [Test]
        public void CreateGSLProcessor_Returns_UnsupportedLearningTypeProcessor_For_Unsupported_LearningType()
        {
            var result = _sut.CreateGSLProcessor((CourseType)0);

            result.Should().Be(_unsupportedProcessor);
        }

        [Test]
        public void CreateGSLProcessor_Returns_FunctionalSkillProcessor_For_FunctionalSkill_Course_Type()
        {
            var result = _sut.CreateGSLProcessor(CourseType.FunctionalSkill);

            result.Should().BeAssignableTo<GSLFunctionalSkillProcessor>();
        }
    }
}