using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.Payments.EarningEvents.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.Data;
using SFA.DAS.Payments.EarningEvents.Model;
using UUIDNext.Tools;

namespace SFA.DAS.Payments.EarningEvents.Application.UnitTests.Repositories
{
    [TestFixture]
    public class HiringPaymentEarningsRepositoryTests
    {
        private const long Ukprn = 10001234;
        private const long Uln = 5602622828;
        private const string CourseCode = "91";

        private EarningsDataContext dataContext;
        private EarningsDataContext queryContext;
        private HiringPaymentEarningsRepository repository;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<EarningsDataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            dataContext = new EarningsDataContext(options);
            queryContext = new EarningsDataContext(options);
            repository = new HiringPaymentEarningsRepository(queryContext);
        }

        [TearDown]
        public void TearDown()
        {
            dataContext.Database.EnsureDeleted();
            queryContext.Dispose();
            dataContext.Dispose();
        }

        [Test]
        public void Returns_Null_When_No_Earnings_Are_Stored()
        {
            var result = repository.GetLatestHiringPaymentEarnings(Ukprn, Uln, CourseCode);

            result.Should().BeNull();
        }

        [TestCase(10009999, Uln, CourseCode)]
        [TestCase(Ukprn, 1234567890, CourseCode)]
        [TestCase(Ukprn, Uln, "92")]
        public async Task Returns_Null_When_Stored_Earnings_Are_For_A_Different_Learner_Or_Course(long ukprn, long uln, string courseCode)
        {
            await Store(CreateEarning(ukprn, uln, courseCode, EarningType.FirstEmployerHiringPayment));

            var result = repository.GetLatestHiringPaymentEarnings(Ukprn, Uln, CourseCode);

            result.Should().BeNull();
        }

        [TestCase(CourseType.ShortCourse)]
        [TestCase(CourseType.FunctionalSkill)]
        public async Task Returns_Null_When_The_Stored_Earnings_Are_Not_For_An_Apprenticeship(CourseType courseType)
        {
            var earning = CreateEarning(Ukprn, Uln, CourseCode, EarningType.FirstEmployerHiringPayment);
            earning.CourseType = courseType;
            await Store(earning);

            var result = repository.GetLatestHiringPaymentEarnings(Ukprn, Uln, CourseCode);

            result.Should().BeNull();
        }

        [Test]
        public async Task Returns_Null_When_The_Stored_Earnings_Start_Before_The_Eligibility_Date()
        {
            var earning = CreateEarning(Ukprn, Uln, CourseCode, EarningType.FirstEmployerHiringPayment);
            earning.StartDate = new DateTime(2026, 9, 30);
            await Store(earning);

            var result = repository.GetLatestHiringPaymentEarnings(Ukprn, Uln, CourseCode);

            result.Should().BeNull();
        }

        [Test]
        public async Task Returns_The_Stored_Earnings_When_They_Start_On_The_Eligibility_Date()
        {
            var earning = CreateEarning(Ukprn, Uln, CourseCode, EarningType.FirstEmployerHiringPayment);
            earning.StartDate = new DateTime(2026, 10, 1);
            await Store(earning);

            var result = repository.GetLatestHiringPaymentEarnings(Ukprn, Uln, CourseCode);

            result.EarningsId.Should().Be(earning.EarningsId);
        }

        [Test]
        public async Task Returns_Only_The_Hiring_Payment_Price_Periods()
        {
            await Store(CreateEarning(Ukprn, Uln, CourseCode,
                EarningType.Learning,
                EarningType.FirstEmployerHiringPayment,
                EarningType.SecondEmployerHiringPayment,
                EarningType.Completion));

            var result = repository.GetLatestHiringPaymentEarnings(Ukprn, Uln, CourseCode);

            result.PricePeriods.Select(x => x.EarningType).Should()
                .BeEquivalentTo(new[] { EarningType.FirstEmployerHiringPayment, EarningType.SecondEmployerHiringPayment });
        }

        [Test]
        public async Task Returns_The_Latest_Stored_Earnings_When_Several_Are_Stored()
        {
            var now = DateTime.UtcNow;
            var older = CreateEarning(Ukprn, Uln, CourseCode, UuidToolkit.CreateUuidV7FromSpecificDate(now.AddDays(-1)), EarningType.FirstEmployerHiringPayment);
            var latest = CreateEarning(Ukprn, Uln, CourseCode, UuidToolkit.CreateUuidV7FromSpecificDate(now), EarningType.SecondEmployerHiringPayment);
            await Store(older);
            await Store(latest);

            var result = repository.GetLatestHiringPaymentEarnings(Ukprn, Uln, CourseCode);

            result.EarningsId.Should().Be(latest.EarningsId);
            result.PricePeriods.Should().ContainSingle()
                .Which.EarningType.Should().Be(EarningType.SecondEmployerHiringPayment);
        }

        [Test]
        public async Task Returns_The_Earnings_With_The_Newest_Timestamp_Even_When_Stored_Earlier()
        {
            var now = DateTime.UtcNow;
            var newest = CreateEarning(Ukprn, Uln, CourseCode, UuidToolkit.CreateUuidV7FromSpecificDate(now), EarningType.SecondEmployerHiringPayment);
            var oldest = CreateEarning(Ukprn, Uln, CourseCode, UuidToolkit.CreateUuidV7FromSpecificDate(now.AddDays(-1)), EarningType.FirstEmployerHiringPayment);
            await Store(newest);
            await Store(oldest);

            var result = repository.GetLatestHiringPaymentEarnings(Ukprn, Uln, CourseCode);

            result.EarningsId.Should().Be(newest.EarningsId);
        }

        [Test]
        public async Task Ignores_Earnings_With_An_Id_That_Cannot_Be_Decoded()
        {
            var valid = CreateEarning(Ukprn, Uln, CourseCode, UuidToolkit.CreateUuidV7FromSpecificDate(DateTime.UtcNow.AddDays(-1)), EarningType.FirstEmployerHiringPayment);
            var undecodable = CreateEarning(Ukprn, Uln, CourseCode, Guid.Empty, EarningType.SecondEmployerHiringPayment);
            await Store(valid);
            await Store(undecodable);

            var result = repository.GetLatestHiringPaymentEarnings(Ukprn, Uln, CourseCode);

            result.EarningsId.Should().Be(valid.EarningsId);
        }

        [Test]
        public async Task Returns_Null_When_No_Stored_Earnings_Id_Can_Be_Decoded()
        {
            await Store(CreateEarning(Ukprn, Uln, CourseCode, Guid.NewGuid(), EarningType.FirstEmployerHiringPayment));

            var result = repository.GetLatestHiringPaymentEarnings(Ukprn, Uln, CourseCode);

            result.Should().BeNull();
        }

        [Test]
        public async Task Returns_The_Highest_Id_When_Timestamps_Are_Equal()
        {
            var now = DateTime.UtcNow;
            var first = CreateEarning(Ukprn, Uln, CourseCode, UuidToolkit.CreateUuidV7FromSpecificDate(now), EarningType.FirstEmployerHiringPayment);
            var second = CreateEarning(Ukprn, Uln, CourseCode, UuidToolkit.CreateUuidV7FromSpecificDate(now), EarningType.SecondEmployerHiringPayment);
            await Store(first);
            await Store(second);

            var result = repository.GetLatestHiringPaymentEarnings(Ukprn, Uln, CourseCode);

            result.PricePeriods.Should().ContainSingle()
                .Which.EarningType.Should().Be(EarningType.SecondEmployerHiringPayment);
        }

        [Test]
        public async Task Returns_No_Price_Periods_When_The_Latest_Earnings_Have_No_Hiring_Payments()
        {
            var now = DateTime.UtcNow;
            await Store(CreateEarning(Ukprn, Uln, CourseCode, UuidToolkit.CreateUuidV7FromSpecificDate(now.AddDays(-1)), EarningType.FirstEmployerHiringPayment));
            await Store(CreateEarning(Ukprn, Uln, CourseCode, UuidToolkit.CreateUuidV7FromSpecificDate(now), EarningType.Learning));

            var result = repository.GetLatestHiringPaymentEarnings(Ukprn, Uln, CourseCode);

            result.PricePeriods.Should().BeEmpty();
        }

        [Test]
        public async Task Returns_Hiring_Payments_Whether_Or_Not_They_Have_Been_Processed()
        {
            var earning = CreateEarning(Ukprn, Uln, CourseCode,
                EarningType.FirstEmployerHiringPayment,
                EarningType.SecondEmployerHiringPayment);
            earning.PricePeriods[0].ProcessedOn = DateTime.UtcNow;
            await Store(earning);

            var result = repository.GetLatestHiringPaymentEarnings(Ukprn, Uln, CourseCode);

            result.PricePeriods.Should().HaveCount(2);
        }

        private async Task Store(GrowthAndSkillsEarningModel earning)
        {
            dataContext.GrowthAndSkillsEarnings.Add(earning);
            await dataContext.SaveChangesAsync();
        }

        private static GrowthAndSkillsEarningModel CreateEarning(long ukprn, long uln, string courseCode, params EarningType[] earningTypes)
        {
            return CreateEarning(ukprn, uln, courseCode, UuidToolkit.CreateUuidV7FromSpecificDate(DateTime.UtcNow), earningTypes);
        }

        private static GrowthAndSkillsEarningModel CreateEarning(long ukprn, long uln, string courseCode, Guid earningsId, params EarningType[] earningTypes)
        {
            return new GrowthAndSkillsEarningModel
            {
                EarningsId = earningsId,
                UKPRN = ukprn,
                LearnerUln = uln,
                LearnerKey = Guid.NewGuid(),
                LearnerReference = "LEARNREF001",
                CourseCode = courseCode,
                CourseReference = "ZPROG001",
                CourseType = CourseType.Apprenticeship,
                StartDate = new DateTime(2026, 10, 1),
                PlannedEndDate = new DateTime(2027, 10, 1),
                AgeAtStartOfTraining = 18,
                PricePeriods = earningTypes.Select((earningType, index) => new GrowthAndSkillsEarningPricePeriodModel
                {
                    GrowthAndSkillsEarningsId = earningsId,
                    Price = 0,
                    StartDate = new DateTime(2026, 10, 1),
                    EndDate = new DateTime(2027, 10, 1),
                    DeliveryPeriod = (byte)(index + 1),
                    AcademicYear = 2627,
                    EarningType = earningType,
                    Amount = 1000m,
                    EmployerAccountId = 310074,
                    FundingAccountId = 310074,
                    ApprenticeshipId = 589568
                }).ToList()
            };
        }
    }
}
