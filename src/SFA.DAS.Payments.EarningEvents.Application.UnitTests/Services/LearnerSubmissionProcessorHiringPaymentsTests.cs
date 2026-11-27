using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.Core.Configuration;
using SFA.DAS.Payments.Core.Validation;
using SFA.DAS.Payments.EarningEvents.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.Application.Services;
using SFA.DAS.Payments.EarningEvents.Domain;
using SFA.DAS.Payments.EarningEvents.Domain.Mapping;
using SFA.DAS.Payments.EarningEvents.Domain.Validation.Learner;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.Internal.Commands;
using SFA.DAS.Payments.EarningEvents.Model;
using SFA.DAS.Payments.Model.Core.Incentives;
using Common = SFA.DAS.Payments.Model.Core;

namespace SFA.DAS.Payments.EarningEvents.Application.UnitTests.Services
{
    [TestFixture]
    public class LearnerSubmissionProcessorHiringPaymentsTests
    {
        private const long Ukprn = 10001234;
        private const long Uln = 5602622828;
        private const int StandardCode = 91;
        private const short AcademicYear = 2627;
        private const byte CollectionPeriod = 3;

        private Mock<IHiringPaymentEarningsRepository> hiringRepository;
        private Mock<IApprenticeshipContractTypeEarningsEventBuilder> actBuilder;
        private Mock<IFunctionalSkillEarningsEventBuilder> functionalSkillBuilder;
        private LearnerSubmissionProcessor processor;
        private ProcessLearnerCommand command;

        [SetUp]
        public void SetUp()
        {
            hiringRepository = new Mock<IHiringPaymentEarningsRepository>();
            var validator = new Mock<ILearnerValidator>();
            validator.Setup(x => x.Validate(It.IsAny<FM36Learner>()))
                .Returns(new ValidationResult(new List<ValidationRuleResult>()));

            actBuilder = new Mock<IApprenticeshipContractTypeEarningsEventBuilder>();
            functionalSkillBuilder = new Mock<IFunctionalSkillEarningsEventBuilder>();

            command = new ProcessLearnerCommand
            {
                Learner = new FM36Learner(),
                CollectionYear = AcademicYear,
                CollectionPeriod = CollectionPeriod,
                Ukprn = Ukprn,
                JobId = 1
            };
            functionalSkillBuilder.Setup(x => x.Build(command)).Returns(new List<FunctionalSkillEarningsEvent>());

            var hiringProvider = new HiringIncentiveEarningsProvider(hiringRepository.Object);
            processor = new LearnerSubmissionProcessor(
                validator.Object,
                actBuilder.Object,
                functionalSkillBuilder.Object,
                hiringProvider,
                new Mock<IConfigurationHelper>().Object);
        }

        [Test]
        public void Hiring_Payments_Merged_Into_The_Eligible_Event_Are_Visible_In_The_Returned_Earning_Events()
        {
            var act1Event = CreateAct1Event();
            actBuilder.Setup(x => x.Build(command)).Returns(new List<ApprenticeshipContractTypeEarningsEvent> { act1Event });
            SetupStoredEarnings(
                CreateStoredPeriod(EarningType.FirstEmployerHiringPayment, 3, 500m),
                CreateStoredPeriod(EarningType.SecondEmployerHiringPayment, 3, 250m));

            var result = processor.GenerateEarnings(command);

            var published = result.EarningEvents.Should().ContainSingle().Which.Should().BeOfType<ApprenticeshipContractType1EarningEvent>().Subject;
            published.Should().BeSameAs(act1Event);
            published.IncentiveEarnings.Select(x => x.Type).Should()
                .BeEquivalentTo(new[] { IncentiveEarningType.LearningSupport, (IncentiveEarningType)18, (IncentiveEarningType)19 });
            published.IncentiveEarnings.Single(x => x.Type == (IncentiveEarningType)18).Periods.Single().Amount.Should().Be(500m);
            published.IncentiveEarnings.Single(x => x.Type == (IncentiveEarningType)19).Periods.Single().Amount.Should().Be(250m);
        }

        [Test]
        public void Merged_Hiring_Payments_In_Future_Delivery_Periods_Are_Trimmed_From_The_Returned_Earning_Events()
        {
            var act1Event = CreateAct1Event();
            actBuilder.Setup(x => x.Build(command)).Returns(new List<ApprenticeshipContractTypeEarningsEvent> { act1Event });
            SetupStoredEarnings(
                CreateStoredPeriod(EarningType.FirstEmployerHiringPayment, 3, 500m),
                CreateStoredPeriod(EarningType.FirstEmployerHiringPayment, 5, 500m));

            var result = processor.GenerateEarnings(command);

            var published = result.EarningEvents.OfType<ApprenticeshipContractType1EarningEvent>().Single();
            published.IncentiveEarnings.Single(x => x.Type == (IncentiveEarningType)18)
                .Periods.Select(p => p.Period).Should().Equal(3);
        }

        [Test]
        public void Event_Is_Returned_Without_Hiring_Payments_When_Nothing_Is_Stored()
        {
            var act1Event = CreateAct1Event();
            actBuilder.Setup(x => x.Build(command)).Returns(new List<ApprenticeshipContractTypeEarningsEvent> { act1Event });

            var result = processor.GenerateEarnings(command);

            result.EarningEvents.Should().ContainSingle().Which.Should().BeSameAs(act1Event);
            act1Event.IncentiveEarnings.Select(x => x.Type).Should().Equal(IncentiveEarningType.LearningSupport);
        }

        private void SetupStoredEarnings(params GrowthAndSkillsEarningPricePeriodModel[] periods)
        {
            hiringRepository.Setup(x => x.GetLatestHiringPaymentEarnings(Ukprn, Uln, StandardCode.ToString()))
                .Returns(new GrowthAndSkillsEarningModel
                {
                    EarningsId = Guid.NewGuid(),
                    PricePeriods = periods.ToList()
                });
        }

        private static GrowthAndSkillsEarningPricePeriodModel CreateStoredPeriod(EarningType earningType, byte deliveryPeriod, decimal amount)
        {
            return new GrowthAndSkillsEarningPricePeriodModel
            {
                EarningType = earningType,
                DeliveryPeriod = deliveryPeriod,
                AcademicYear = AcademicYear,
                Amount = amount,
                EmployerAccountId = 310074,
                FundingAccountId = 310074,
                ApprenticeshipId = 589568,
                EmployerType = EmployerType.NonLevy
            };
        }

        private static ApprenticeshipContractType1EarningEvent CreateAct1Event()
        {
            return new ApprenticeshipContractType1EarningEvent
            {
                Ukprn = Ukprn,
                Learner = new Common.Learner { Uln = Uln, ReferenceNumber = "LEARNREF001" },
                LearningAim = new Common.LearningAim { StandardCode = StandardCode },
                StartDate = new DateTime(2026, 11, 1),
                AgeAtStartOfLearning = 18,
                CollectionPeriod = new Common.CollectionPeriod { AcademicYear = AcademicYear, Period = CollectionPeriod },
                OnProgrammeEarnings = new List<Common.OnProgramme.OnProgrammeEarning>
                {
                    new Common.OnProgramme.OnProgrammeEarning
                    {
                        Type = Common.OnProgramme.OnProgrammeEarningType.Learning,
                        Periods = new List<Common.EarningPeriod>().AsReadOnly()
                    }
                },
                IncentiveEarnings = new List<IncentiveEarning>
                {
                    new IncentiveEarning
                    {
                        Type = IncentiveEarningType.LearningSupport,
                        Periods = new List<Common.EarningPeriod> { new Common.EarningPeriod { Period = 1, Amount = 150m } }.AsReadOnly()
                    }
                },
                PriceEpisodes = new List<Common.PriceEpisode>()
            };
        }
    }
}
