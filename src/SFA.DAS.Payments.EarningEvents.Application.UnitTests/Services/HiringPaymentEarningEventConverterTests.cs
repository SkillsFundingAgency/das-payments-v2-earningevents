using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.EarningEvents.Application.Mapping;
using SFA.DAS.Payments.EarningEvents.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.Application.Services;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Model;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Model.Core.Incentives;
using Common = SFA.DAS.Payments.Model.Core;

namespace SFA.DAS.Payments.EarningEvents.Application.UnitTests.Services
{
    [TestFixture]
    public class HiringPaymentEarningEventConverterTests
    {
        private const long Ukprn = 10001234;
        private const long Uln = 5602622828;
        private const int StandardCode = 91;
        private const short AcademicYear = 2627;

        private Mock<IHiringPaymentEarningsRepository> repository;
        private Mock<IApprenticeshipEmployerTypeRepository> employerTypeRepository;
        private HiringPaymentEarningEventConverter converter;
        private Guid storedEarningsId;

        [SetUp]
        public void SetUp()
        {
            storedEarningsId = Guid.NewGuid();
            repository = new Mock<IHiringPaymentEarningsRepository>();
            employerTypeRepository = new Mock<IApprenticeshipEmployerTypeRepository>();
            employerTypeRepository.Setup(x => x.GetEmployerType(Ukprn, Uln, StandardCode))
                .Returns(ApprenticeshipEmployerType.NonLevy);
            var mapperConfiguration = new MapperConfiguration(config => config.AddProfile<HiringPaymentEarningEventProfile>());
            converter = new HiringPaymentEarningEventConverter(repository.Object, employerTypeRepository.Object, mapperConfiguration.CreateMapper());
        }

        [Test]
        public void Mapping_Configuration_Is_Valid()
        {
            var mapperConfiguration = new MapperConfiguration(config => config.AddProfile<HiringPaymentEarningEventProfile>());

            mapperConfiguration.AssertConfigurationIsValid();
        }

        [Test]
        public void Events_Not_Meeting_The_Start_Date_And_Age_Criteria_Are_Returned_Unchanged_Without_Any_Lookup()
        {
            var tooEarly = CreateAct1Event(new DateTime(2026, 9, 1), 18);
            var tooOld = CreateAct1Event(new DateTime(2026, 11, 1), 25);
            var act2 = new ApprenticeshipContractType2EarningEvent { StartDate = new DateTime(2026, 11, 1), AgeAtStartOfLearning = 18 };
            var events = new List<EarningEvent> { tooEarly, tooOld, act2 };

            var result = converter.Convert(events);

            result.Should().HaveCount(3);
            result[0].Should().BeSameAs(tooEarly);
            result[1].Should().BeSameAs(tooOld);
            result[2].Should().BeSameAs(act2);
            employerTypeRepository.Verify(x => x.GetEmployerType(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()), Times.Never);
            repository.Verify(x => x.GetLatestHiringPaymentEarnings(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Act1_Redundancy_Events_Are_Returned_Unchanged()
        {
            var redundancyEvent = new ApprenticeshipContractType1RedundancyEarningEvent
            {
                StartDate = new DateTime(2026, 11, 1),
                AgeAtStartOfLearning = 18
            };

            var result = converter.Convert(new List<EarningEvent> { redundancyEvent });

            result.Single().Should().BeSameAs(redundancyEvent);
            employerTypeRepository.Verify(x => x.GetEmployerType(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()), Times.Never);
        }

        [Test]
        public void Event_Is_Returned_Unchanged_When_The_Apprenticeship_Is_Levy()
        {
            employerTypeRepository.Setup(x => x.GetEmployerType(Ukprn, Uln, StandardCode))
                .Returns(ApprenticeshipEmployerType.Levy);
            var act1Event = CreateAct1Event(new DateTime(2026, 10, 1), 18);

            var result = converter.Convert(new List<EarningEvent> { act1Event });

            result.Single().Should().BeSameAs(act1Event);
            repository.Verify(x => x.GetLatestHiringPaymentEarnings(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Event_Is_Returned_Unchanged_When_No_Apprenticeship_Is_Found()
        {
            employerTypeRepository.Setup(x => x.GetEmployerType(Ukprn, Uln, StandardCode))
                .Returns((ApprenticeshipEmployerType?)null);
            var act1Event = CreateAct1Event(new DateTime(2026, 10, 1), 18);

            var result = converter.Convert(new List<EarningEvent> { act1Event });

            result.Single().Should().BeSameAs(act1Event);
            repository.Verify(x => x.GetLatestHiringPaymentEarnings(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Eligible_Act1_Event_Is_Replaced_By_A_Gsl_Event()
        {
            SetupStoredEarnings(CreateStoredPeriod(EarningType.FirstEmployerHiringPayment, 3));
            var act1Event = CreateAct1Event(new DateTime(2026, 10, 1), 18);
            var otherEvent = new ApprenticeshipContractType2EarningEvent();

            var result = converter.Convert(new List<EarningEvent> { act1Event, otherEvent });

            result.Should().HaveCount(2);
            result[0].Should().BeOfType<GSLApprenticeshipEarningsEvent>();
            result[1].Should().BeSameAs(otherEvent);
        }

        [Test]
        public void Gsl_Event_Keeps_The_Properties_Of_The_Act1_Event_And_Sets_The_Gsl_Properties()
        {
            SetupStoredEarnings(CreateStoredPeriod(EarningType.FirstEmployerHiringPayment, 3));
            var act1Event = CreateAct1Event(new DateTime(2026, 10, 1), 18);

            var gslEvent = (GSLApprenticeshipEarningsEvent)(converter.Convert(new List<EarningEvent> { act1Event })).Single();

            gslEvent.Ukprn.Should().Be(Ukprn);
            gslEvent.Learner.Uln.Should().Be(Uln);
            gslEvent.StartDate.Should().Be(act1Event.StartDate);
            gslEvent.AgeAtStartOfLearning.Should().Be(18);
            gslEvent.CollectionPeriod.Should().BeEquivalentTo(act1Event.CollectionPeriod);
            gslEvent.OnProgrammeEarnings.Should().BeEquivalentTo(act1Event.OnProgrammeEarnings);
            gslEvent.ContractType.Should().Be(ContractType.Act1);
            gslEvent.FundingPlatformType.Should().Be(FundingPlatformType.SubmitLearnerData);
            gslEvent.AgreementId.Should().BeNull();
            gslEvent.ExternalEarningsId.Should().Be(storedEarningsId);
        }

        [Test]
        public void Stored_Hiring_Payments_Are_Added_To_The_Incentive_Earnings()
        {
            SetupStoredEarnings(
                CreateStoredPeriod(EarningType.FirstEmployerHiringPayment, 3, 1000m),
                CreateStoredPeriod(EarningType.SecondEmployerHiringPayment, 5, 2000m));
            var act1Event = CreateAct1Event(new DateTime(2026, 10, 1), 18);

            var gslEvent = (GSLApprenticeshipEarningsEvent)(converter.Convert(new List<EarningEvent> { act1Event })).Single();

            gslEvent.IncentiveEarnings.Select(x => x.Type).Should()
                .BeEquivalentTo(new[] { IncentiveEarningType.LearningSupport, (IncentiveEarningType)18, (IncentiveEarningType)19 });
            var first = gslEvent.IncentiveEarnings.Single(x => x.Type == (IncentiveEarningType)18).Periods.Single();
            first.Period.Should().Be(3);
            first.Amount.Should().Be(1000m);
            first.AccountId.Should().Be(310074);
            first.ApprenticeshipId.Should().Be(589568);
            first.SfaContributionPercentage.Should().Be(1m);
            first.TransferSenderAccountId.Should().BeNull();
        }

        [Test]
        public void Existing_Incentive_Earnings_Are_Kept()
        {
            SetupStoredEarnings(CreateStoredPeriod(EarningType.FirstEmployerHiringPayment, 3));
            var act1Event = CreateAct1Event(new DateTime(2026, 10, 1), 18);

            var gslEvent = (GSLApprenticeshipEarningsEvent)(converter.Convert(new List<EarningEvent> { act1Event })).Single();

            gslEvent.IncentiveEarnings.Should().Contain(x => x.Type == IncentiveEarningType.LearningSupport);
        }

        [Test]
        public void All_Stored_Hiring_Payments_In_The_Academic_Year_Are_Added_Leaving_Period_Trimming_To_The_Processor()
        {
            SetupStoredEarnings(
                CreateStoredPeriod(EarningType.FirstEmployerHiringPayment, 4),
                CreateStoredPeriod(EarningType.SecondEmployerHiringPayment, 6));
            var act1Event = CreateAct1Event(new DateTime(2026, 10, 1), 18, collectionPeriod: 5);

            var gslEvent = (GSLApprenticeshipEarningsEvent)(converter.Convert(new List<EarningEvent> { act1Event })).Single();

            gslEvent.IncentiveEarnings.Should().Contain(x => x.Type == (IncentiveEarningType)18);
            gslEvent.IncentiveEarnings.Should().Contain(x => x.Type == (IncentiveEarningType)19);
        }

        [Test]
        public void Hiring_Payments_From_Another_Academic_Year_Are_Not_Added()
        {
            SetupStoredEarnings(CreateStoredPeriod(EarningType.FirstEmployerHiringPayment, 3, academicYear: 2526));
            var act1Event = CreateAct1Event(new DateTime(2026, 10, 1), 18);

            var gslEvent = (GSLApprenticeshipEarningsEvent)(converter.Convert(new List<EarningEvent> { act1Event })).Single();

            gslEvent.IncentiveEarnings.Should().NotContain(x => x.Type == (IncentiveEarningType)18);
        }

        [Test]
        public void Gsl_Event_Is_Published_Without_Hiring_Payments_When_None_Are_Stored()
        {
            repository.Setup(x => x.GetLatestHiringPaymentEarnings(Ukprn, Uln, StandardCode.ToString()))
                .Returns((GrowthAndSkillsEarningModel)null);
            var act1Event = CreateAct1Event(new DateTime(2026, 10, 1), 18);

            var gslEvent = (GSLApprenticeshipEarningsEvent)(converter.Convert(new List<EarningEvent> { act1Event })).Single();

            gslEvent.IncentiveEarnings.Select(x => x.Type).Should().BeEquivalentTo(new[] { IncentiveEarningType.LearningSupport });
            gslEvent.ExternalEarningsId.Should().Be(Guid.Empty);
        }

        [Test]
        public void Transfer_Sender_Is_Set_When_The_Funding_Account_Differs_From_The_Employer()
        {
            var period = CreateStoredPeriod(EarningType.FirstEmployerHiringPayment, 3);
            period.FundingAccountId = 999;
            SetupStoredEarnings(period);
            var act1Event = CreateAct1Event(new DateTime(2026, 10, 1), 18);

            var gslEvent = (GSLApprenticeshipEarningsEvent)(converter.Convert(new List<EarningEvent> { act1Event })).Single();

            gslEvent.IncentiveEarnings.Single(x => x.Type == (IncentiveEarningType)18)
                .Periods.Single().TransferSenderAccountId.Should().Be(999);
        }

        [Test]
        public void Price_Episode_Identifier_Is_The_Latest_Episode_Starting_On_Or_Before_The_Delivery_Period()
        {
            SetupStoredEarnings(CreateStoredPeriod(EarningType.FirstEmployerHiringPayment, 6));
            var act1Event = CreateAct1Event(new DateTime(2026, 10, 1), 18);
            act1Event.PriceEpisodes = new List<Common.PriceEpisode>
            {
                new Common.PriceEpisode { Identifier = "91-2-01/10/2026", StartDate = new DateTime(2026, 10, 1) },
                new Common.PriceEpisode { Identifier = "91-2-01/01/2027", StartDate = new DateTime(2027, 1, 1) },
                new Common.PriceEpisode { Identifier = "91-2-01/03/2027", StartDate = new DateTime(2027, 3, 1) }
            };

            var gslEvent = (GSLApprenticeshipEarningsEvent)(converter.Convert(new List<EarningEvent> { act1Event })).Single();

            gslEvent.IncentiveEarnings.Single(x => x.Type == (IncentiveEarningType)18)
                .Periods.Single().PriceEpisodeIdentifier.Should().Be("91-2-01/01/2027");
        }

        [Test]
        public void Price_Episode_Identifier_Falls_Back_To_The_First_Episode_When_None_Starts_Before_The_Delivery_Period()
        {
            SetupStoredEarnings(CreateStoredPeriod(EarningType.FirstEmployerHiringPayment, 1));
            var act1Event = CreateAct1Event(new DateTime(2026, 10, 1), 18);
            act1Event.PriceEpisodes = new List<Common.PriceEpisode>
            {
                new Common.PriceEpisode { Identifier = "91-2-01/10/2026", StartDate = new DateTime(2026, 10, 1) }
            };

            var gslEvent = (GSLApprenticeshipEarningsEvent)(converter.Convert(new List<EarningEvent> { act1Event })).Single();

            gslEvent.IncentiveEarnings.Single(x => x.Type == (IncentiveEarningType)18)
                .Periods.Single().PriceEpisodeIdentifier.Should().Be("91-2-01/10/2026");
        }

        private void SetupStoredEarnings(params GrowthAndSkillsEarningPricePeriodModel[] periods)
        {
            repository.Setup(x => x.GetLatestHiringPaymentEarnings(Ukprn, Uln, StandardCode.ToString()))
                .Returns(new GrowthAndSkillsEarningModel
                {
                    EarningsId = storedEarningsId,
                    PricePeriods = periods.ToList()
                });
        }

        private static GrowthAndSkillsEarningPricePeriodModel CreateStoredPeriod(EarningType earningType, byte deliveryPeriod, decimal amount = 1000m, short academicYear = AcademicYear)
        {
            return new GrowthAndSkillsEarningPricePeriodModel
            {
                EarningType = earningType,
                DeliveryPeriod = deliveryPeriod,
                AcademicYear = academicYear,
                Amount = amount,
                EmployerAccountId = 310074,
                FundingAccountId = 310074,
                ApprenticeshipId = 589568,
                EmployerType = EmployerType.NonLevy
            };
        }

        private static ApprenticeshipContractType1EarningEvent CreateAct1Event(DateTime startDate, int age, byte collectionPeriod = 12)
        {
            return new ApprenticeshipContractType1EarningEvent
            {
                Ukprn = Ukprn,
                Learner = new Common.Learner { Uln = Uln, ReferenceNumber = "LEARNREF001" },
                LearningAim = new Common.LearningAim { StandardCode = StandardCode },
                StartDate = startDate,
                AgeAtStartOfLearning = age,
                CollectionPeriod = new Common.CollectionPeriod { AcademicYear = AcademicYear, Period = collectionPeriod },
                OnProgrammeEarnings = new List<Common.OnProgramme.OnProgrammeEarning>
                {
                    new Common.OnProgramme.OnProgrammeEarning { Type = Common.OnProgramme.OnProgrammeEarningType.Learning }
                },
                IncentiveEarnings = new List<IncentiveEarning>
                {
                    new IncentiveEarning { Type = IncentiveEarningType.LearningSupport }
                },
                PriceEpisodes = new List<Common.PriceEpisode>()
            };
        }
    }
}
