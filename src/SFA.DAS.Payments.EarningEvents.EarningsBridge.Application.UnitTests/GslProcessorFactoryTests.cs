using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.AutoMock;
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
        private AutoMocker mocker;
        private GSLProcessorFactory _sut;
        private Mock<IServiceProvider> _serviceProvider;

        [SetUp]
        public void SetUp()
        {
            mocker = new AutoMocker(MockBehavior.Loose);

            _serviceProvider = mocker.GetMock<IServiceProvider>();            

            _sut = mocker.CreateInstance<GSLProcessorFactory>();
        }

        [TestCase(CourseType.Apprenticeship,typeof(GSLApprenticeshipPaymentsProcessor))]
        [TestCase(CourseType.ShortCourse, typeof(GSLShortCoursePaymentsProcessor))]
        [TestCase(CourseType.FunctionalSkill, typeof(GSLFunctionalSkillProcessor))]
        public void CreateGSLProcessor_Returns_Correct_Processor(CourseType courseType, Type processorType)
        {
            _serviceProvider.Setup(x => x.GetService(processorType)).Returns(mocker.CreateInstance(processorType));
            var result = _sut.CreateGSLProcessor(courseType);
            mocker.GetMock<IServiceProvider>().Verify(x => x.GetService(processorType), Times.Once());
            result.Should().BeAssignableTo(processorType);
        }


        [Test]
        public void Unknown_Course_Type_Throws_Exception() 
        {
            _serviceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns(mocker.CreateInstance<GSLApprenticeshipPaymentsProcessor>());
            Assert.Throws<InvalidOperationException>(() => _sut.CreateGSLProcessor((CourseType)0));
        }
    }
}