using System;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Payments.EarningEvents.Application.Mapping;
using SFA.DAS.Payments.EarningEvents.Messages.Events;

namespace SFA.DAS.Payments.EarningEvents.Application.UnitTests.Mapping
{
    [TestFixture]
    public class HiringPaymentEligibilityTests
    {
        [Test]
        public void Event_Starting_On_The_Eligibility_Date_For_A_Learner_Under_25_Meets_The_Criteria()
        {
            var earningEvent = CreateAct1Event(new DateTime(2026, 10, 1), 24);

            HiringPaymentEligibility.MeetsStartDateAndAge(earningEvent).Should().BeTrue();
        }

        [Test]
        public void Event_Starting_After_The_Eligibility_Date_For_A_Learner_Under_25_Meets_The_Criteria()
        {
            var earningEvent = CreateAct1Event(new DateTime(2027, 2, 15), 18);

            HiringPaymentEligibility.MeetsStartDateAndAge(earningEvent).Should().BeTrue();
        }

        [Test]
        public void Event_Starting_Before_The_Eligibility_Date_Does_Not_Meet_The_Criteria()
        {
            var earningEvent = CreateAct1Event(new DateTime(2026, 9, 30), 18);

            HiringPaymentEligibility.MeetsStartDateAndAge(earningEvent).Should().BeFalse();
        }

        [TestCase(25)]
        [TestCase(40)]
        public void Event_For_A_Learner_Aged_25_Or_Over_Does_Not_Meet_The_Criteria(int age)
        {
            var earningEvent = CreateAct1Event(new DateTime(2026, 11, 1), age);

            HiringPaymentEligibility.MeetsStartDateAndAge(earningEvent).Should().BeFalse();
        }

        [Test]
        public void Event_Without_An_Age_Does_Not_Meet_The_Criteria()
        {
            var earningEvent = CreateAct1Event(new DateTime(2026, 11, 1), null);

            HiringPaymentEligibility.MeetsStartDateAndAge(earningEvent).Should().BeFalse();
        }

        private static ApprenticeshipContractType1EarningEvent CreateAct1Event(DateTime startDate, int? age)
        {
            return new ApprenticeshipContractType1EarningEvent
            {
                StartDate = startDate,
                AgeAtStartOfLearning = age
            };
        }
    }
}
