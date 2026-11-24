using FluentAssertions;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Handlers;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Processors;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.UnitTests;

[TestFixture]
public class IncomingEarningsMessageProcessorFactoryTests
{
    [Test]
    public void RoutesByTypeNameRegardlessOfAssemblyName()
    {
        // Arrange
        var calculateProcessor = new CalculatePaymentsMessageProcessor(Mock.Of<IGSLCalculatePaymentsHandler>());
        var withholdProcessor = new WithholdPaymentsForLearningMessageProcessor(
            Mock.Of<IEarningsRepository>(),
            Mock.Of<ICollectionPeriodService>(),
            Mock.Of<IPaymentsServiceBusPublisher>());
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider
            .Setup(x => x.GetService(typeof(CalculatePaymentsMessageProcessor)))
            .Returns(calculateProcessor);
        serviceProvider
            .Setup(x => x.GetService(typeof(WithholdPaymentsForLearningMessageProcessor)))
            .Returns(withholdProcessor);
        var factory = new IncomingEarningsMessageProcessorFactory(serviceProvider.Object);

        // Act
        var calculateResult = factory.Create($"{typeof(CalculateGrowthAndSkillsPayments).FullName}, Source.Assembly");
        var withholdResult = factory.Create($"{typeof(WithholdPaymentsForLearningEvent).FullName}, Source.Assembly");

        //Included this to basically check that the factory can handle the full assembly-qualified name, which is what NServiceBus will actually send in the EnclosedMessageTypes header.
        var calculateResultWithAssemblyDetails = factory.Create(
            "SFA.DAS.Payments.EarningEvents.Messages.External.Commands.CalculateGrowthAndSkillsPayments, " +
            "SFA.DAS.Payments.EarningEvents.Messages.External, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
        var withholdResultWithAssemblyDetails = factory.Create(
            "SFA.DAS.Funding.ApprenticeshipEarnings.Types.WithholdPaymentsForLearningEvent, " +
            "SFA.DAS.Funding.ApprenticeshipEarnings.Types, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

        var calculateResultWithBaseType = factory.Create(
            "SFA.DAS.Payments.EarningEvents.Messages.External.BaseCommand, Source.Assembly;" +
            $"{typeof(CalculateGrowthAndSkillsPayments).FullName}, Source.Assembly");
        var withholdResultWithBaseType = factory.Create(
            "SFA.DAS.Funding.ApprenticeshipEarnings.Types.BaseEvent, Source.Assembly;" +
            $"{typeof(WithholdPaymentsForLearningEvent).FullName}, Source.Assembly");

        Action createUnsupportedMessage = () => factory.Create("Unknown.Event, Source.Assembly");

        // Assert
        calculateResult.Should().BeSameAs(calculateProcessor);
        withholdResult.Should().BeSameAs(withholdProcessor);
        calculateResultWithAssemblyDetails.Should().BeSameAs(calculateProcessor);
        withholdResultWithAssemblyDetails.Should().BeSameAs(withholdProcessor);
        calculateResultWithBaseType.Should().BeSameAs(calculateProcessor);
        withholdResultWithBaseType.Should().BeSameAs(withholdProcessor);
        createUnsupportedMessage.Should().Throw<InvalidOperationException>();
    }
}
