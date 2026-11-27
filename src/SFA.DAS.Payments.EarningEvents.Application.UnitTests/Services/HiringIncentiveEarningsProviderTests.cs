using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.EarningEvents.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.Application.Services;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Model;
using SFA.DAS.Payments.Model.Core.Incentives;
using Common = SFA.DAS.Payments.Model.Core;

namespace SFA.DAS.Payments.EarningEvents.Application.UnitTests.Services
{
    [TestFixture]
    public class HiringIncentiveEarningsProviderTests
    {
        private const long Ukprn = 10001234;
        private const long Uln = 5602622828;
        private const int StandardCode = 91;
        private const short AcademicYear = 2627;

        private Mock<IHiringPaymentEarningsRepository> repository;
        private HiringIncentiveEarningsProvider provider;

        [SetUp]
        public void SetUp()
        {
            repository = new Mock<IHiringPaymentEarningsRepository>();
            provider = new HiringIncentiveEarningsProvider(repository.Object);
        }

        [Test]
        public void Returns_Nothing_When_No_Earnings_Are_Stored()
        {
            repository.Setup(x => x.GetLatestHiringPaymentEarnings(Ukprn, Uln, StandardCode.ToString()))
                .Returns((GrowthAndSkillsEarningModel)null);

            var result = Provide(CreateAct1Event());

            result.Should().BeEmpty();
        }

        [Test]
        public void Returns_Nothing_When_The_Stored_Record_Has_No_Price_Periods()
        {
            SetupStoredEarnings();

            var result = Provide(CreateAct1Event());

            result.Should().BeEmpty();
        }

        [Test]
        public void Returns_The_Candidate_Event_With_Its_Hiring_Incentive_Earnings()
        {
            SetupStoredEarnings(
                CreateStoredPeriod(EarningType.FirstEmployerHiringPayment, 3, 1000m),
                CreateStoredPeriod(EarningType.SecondEmployerHiringPayment, 5, 2000m));
            var act1Event = CreateAct1Event();

            var result = Provide(act1Event);

            var (resultEvent, incentives) = result.Single();
            resultEvent.Should().BeSameAs(act1Event);
            incentives.Select(x => x.Type).Should().BeEquivalentTo(new[] { (IncentiveEarningType)18, (IncentiveEarningType)19 });
            var first = incentives.Single(x => x.Type == (IncentiveEarningType)18).Periods.Single();
            first.Period.Should().Be(3);
            first.Amount.Should().Be(1000m);
            first.AccountId.Should().Be(310074);
            first.ApprenticeshipId.Should().Be(589568);
            first.SfaContributionPercentage.Should().Be(1m);
            first.TransferSenderAccountId.Should().BeNull();
            incentives.Single(x => x.Type == (IncentiveEarningType)19).Periods.Single().Amount.Should().Be(2000m);
        }

        [Test]
        public void Does_Not_Change_The_Event_It_Is_Given()
        {
            SetupStoredEarnings(CreateStoredPeriod(EarningType.FirstEmployerHiringPayment, 3));
            var act1Event = CreateAct1Event();
            var incentivesBefore = act1Event.IncentiveEarnings;

            Provide(act1Event);

            act1Event.IncentiveEarnings.Should().BeSameAs(incentivesBefore);
            act1Event.IncentiveEarnings.Select(x => x.Type).Should().Equal(IncentiveEarningType.LearningSupport);
        }

        [Test]
        public void Only_Non_Levy_Price_Periods_Are_Returned()
        {
            SetupStoredEarnings(
                CreateStoredPeriod(EarningType.FirstEmployerHiringPayment, 3, 1000m),
                CreateStoredPeriod(EarningType.FirstEmployerHiringPayment, 4, 1000m, employerType: EmployerType.Levy),
                CreateStoredPeriod(EarningType.SecondEmployerHiringPayment, 5, 2000m, employerType: EmployerType.Levy));

            var result = Provide(CreateAct1Event());

            var incentives = result.Single().IncentiveEarnings;
            incentives.Should().ContainSingle().Which.Type.Should().Be((IncentiveEarningType)18);
            incentives.Single().Periods.Select(x => x.Period).Should().Equal(3);
        }

        [Test]
        public void Returns_Nothing_When_All_The_Stored_Price_Periods_Are_Levy()
        {
            SetupStoredEarnings(
                CreateStoredPeriod(EarningType.FirstEmployerHiringPayment, 3, employerType: EmployerType.Levy),
                CreateStoredPeriod(EarningType.SecondEmployerHiringPayment, 5, employerType: EmployerType.Levy));

            var result = Provide(CreateAct1Event());

            result.Should().BeEmpty();
        }

        [Test]
        public void Hiring_Payments_From_Another_Academic_Year_Are_Not_Returned()
        {
            SetupStoredEarnings(CreateStoredPeriod(EarningType.FirstEmployerHiringPayment, 3, academicYear: 2526));

            var result = Provide(CreateAct1Event());

            result.Should().BeEmpty();
        }

        [Test]
        public void All_Stored_Hiring_Payments_In_The_Academic_Year_Are_Returned_Leaving_Period_Trimming_To_The_Processor()
        {
            SetupStoredEarnings(
                CreateStoredPeriod(EarningType.FirstEmployerHiringPayment, 4),
                CreateStoredPeriod(EarningType.SecondEmployerHiringPayment, 6));

            var result = Provide(CreateAct1Event(collectionPeriod: 5));

            result.Single().IncentiveEarnings.Select(x => x.Type).Should()
                .BeEquivalentTo(new[] { (IncentiveEarningType)18, (IncentiveEarningType)19 });
        }

        [Test]
        public void Stored_Hiring_Payments_Are_Looked_Up_Once_And_Returned_For_Only_One_Of_Several_Events_Of_The_Same_Apprenticeship()
        {
            SetupStoredEarnings(CreateStoredPeriod(EarningType.FirstEmployerHiringPayment, 3));

            var result = Provide(CreateAct1Event(), CreateAct1Event());

            repository.Verify(x => x.GetLatestHiringPaymentEarnings(Ukprn, Uln, StandardCode.ToString()), Times.Once);
            result.Should().ContainSingle();
        }

        [Test]
        public void Hiring_Payments_Are_Returned_For_The_Event_With_The_Earliest_Start_Date_Only()
        {
            SetupStoredEarnings(CreateStoredPeriod(EarningType.FirstEmployerHiringPayment, 3));
            var later = CreateAct1Event(new DateTime(2026, 12, 1));
            var earliest = CreateAct1Event(new DateTime(2026, 10, 1));

            var result = Provide(later, earliest);

            result.Should().ContainSingle().Which.Event.Should().BeSameAs(earliest);
        }

        [Test]
        public void Hiring_Payments_Are_Returned_Once_For_Each_Different_Apprenticeship()
        {
            const int otherStandardCode = 92;
            SetupStoredEarnings(CreateStoredPeriod(EarningType.FirstEmployerHiringPayment, 3));
            repository.Setup(x => x.GetLatestHiringPaymentEarnings(Ukprn, Uln, otherStandardCode.ToString()))
                .Returns(new GrowthAndSkillsEarningModel
                {
                    EarningsId = Guid.NewGuid(),
                    PricePeriods = new List<GrowthAndSkillsEarningPricePeriodModel> { CreateStoredPeriod(EarningType.FirstEmployerHiringPayment, 3) }
                });
            var first = CreateAct1Event();
            var second = CreateAct1Event();
            second.LearningAim.StandardCode = otherStandardCode;

            var result = Provide(first, second);

            result.Should().HaveCount(2);
            result.Should().Contain(x => ReferenceEquals(x.Event, first));
            result.Should().Contain(x => ReferenceEquals(x.Event, second));
            repository.Verify(x => x.GetLatestHiringPaymentEarnings(Ukprn, Uln, StandardCode.ToString()), Times.Once);
            repository.Verify(x => x.GetLatestHiringPaymentEarnings(Ukprn, Uln, otherStandardCode.ToString()), Times.Once);
        }

        [Test]
        public void Transfer_Sender_Is_Set_When_The_Funding_Account_Differs_From_The_Employer()
        {
            var period = CreateStoredPeriod(EarningType.FirstEmployerHiringPayment, 3);
            period.FundingAccountId = 999;
            SetupStoredEarnings(period);

            var result = Provide(CreateAct1Event());

            result.Single().IncentiveEarnings.Single(x => x.Type == (IncentiveEarningType)18)
                .Periods.Single().TransferSenderAccountId.Should().Be(999);
        }

        [Test]
        public void Price_Episode_Identifier_Is_The_Latest_Episode_Starting_On_Or_Before_The_Delivery_Period()
        {
            SetupStoredEarnings(CreateStoredPeriod(EarningType.FirstEmployerHiringPayment, 6));
            var act1Event = CreateAct1Event();
            act1Event.PriceEpisodes = new List<Common.PriceEpisode>
            {
                new Common.PriceEpisode { Identifier = "91-2-01/10/2026", StartDate = new DateTime(2026, 10, 1) },
                new Common.PriceEpisode { Identifier = "91-2-01/01/2027", StartDate = new DateTime(2027, 1, 1) },
                new Common.PriceEpisode { Identifier = "91-2-01/03/2027", StartDate = new DateTime(2027, 3, 1) }
            };

            var result = Provide(act1Event);

            result.Single().IncentiveEarnings.Single(x => x.Type == (IncentiveEarningType)18)
                .Periods.Single().PriceEpisodeIdentifier.Should().Be("91-2-01/01/2027");
        }

        [Test]
        public void Price_Episode_Identifier_Falls_Back_To_The_First_Episode_When_None_Starts_Before_The_Delivery_Period()
        {
            SetupStoredEarnings(CreateStoredPeriod(EarningType.FirstEmployerHiringPayment, 1));
            var act1Event = CreateAct1Event();
            act1Event.PriceEpisodes = new List<Common.PriceEpisode>
            {
                new Common.PriceEpisode { Identifier = "91-2-01/10/2026", StartDate = new DateTime(2026, 10, 1) }
            };

            var result = Provide(act1Event);

            result.Single().IncentiveEarnings.Single(x => x.Type == (IncentiveEarningType)18)
                .Periods.Single().PriceEpisodeIdentifier.Should().Be("91-2-01/10/2026");
        }

        private List<(ApprenticeshipContractType1EarningEvent Event, List<IncentiveEarning> IncentiveEarnings)> Provide(params ApprenticeshipContractType1EarningEvent[] events)
        {
            return provider.GetHiringIncentiveEarnings(events.ToList());
        }

        private void SetupStoredEarnings(params GrowthAndSkillsEarningPricePeriodModel[] periods)
        {
            repository.Setup(x => x.GetLatestHiringPaymentEarnings(Ukprn, Uln, StandardCode.ToString()))
                .Returns(new GrowthAndSkillsEarningModel
                {
                    EarningsId = Guid.NewGuid(),
                    PricePeriods = periods.ToList()
                });
        }

        private static GrowthAndSkillsEarningPricePeriodModel CreateStoredPeriod(EarningType earningType, byte deliveryPeriod, decimal amount = 1000m, short academicYear = AcademicYear, EmployerType employerType = EmployerType.NonLevy)
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
                EmployerType = employerType
            };
        }

        private static ApprenticeshipContractType1EarningEvent CreateAct1Event(DateTime? startDate = null, byte collectionPeriod = 12)
        {
            return new ApprenticeshipContractType1EarningEvent
            {
                Ukprn = Ukprn,
                Learner = new Common.Learner { Uln = Uln, ReferenceNumber = "LEARNREF001" },
                LearningAim = new Common.LearningAim { StandardCode = StandardCode },
                StartDate = startDate ?? new DateTime(2026, 10, 1),
                AgeAtStartOfLearning = 18,
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
