using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.Application.Repositories;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.EarningEvents.Application.UnitTests.Repositories
{
    [TestFixture]
    public class ApprenticeshipEmployerTypeRepositoryTests
    {
        private const long Ukprn = 10001234;
        private const long Uln = 5602622828;
        private const long StandardCode = 91;

        private PaymentsDataContext dataContext;
        private PaymentsDataContext queryContext;
        private ApprenticeshipEmployerTypeRepository repository;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<PaymentsDataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            dataContext = new PaymentsDataContext(options);
            queryContext = new PaymentsDataContext(options);
            repository = new ApprenticeshipEmployerTypeRepository(queryContext);
        }

        [TearDown]
        public void TearDown()
        {
            dataContext.Database.EnsureDeleted();
            queryContext.Dispose();
            dataContext.Dispose();
        }

        [Test]
        public async Task Returns_Null_When_There_Is_No_Apprenticeship()
        {
            var result = await repository.GetEmployerType(Ukprn, Uln, StandardCode);

            result.Should().BeNull();
        }

        [TestCase(10009999, Uln, StandardCode)]
        [TestCase(Ukprn, 1234567890, StandardCode)]
        [TestCase(Ukprn, Uln, 92)]
        public async Task Returns_Null_When_The_Apprenticeship_Is_For_A_Different_Provider_Learner_Or_Standard(long ukprn, long uln, long standardCode)
        {
            await Store(CreateApprenticeship(ukprn, uln, standardCode, ApprenticeshipEmployerType.NonLevy));

            var result = await repository.GetEmployerType(Ukprn, Uln, StandardCode);

            result.Should().BeNull();
        }

        [TestCase(ApprenticeshipEmployerType.NonLevy)]
        [TestCase(ApprenticeshipEmployerType.Levy)]
        public async Task Returns_The_Employer_Type_Of_The_Matching_Apprenticeship(ApprenticeshipEmployerType employerType)
        {
            await Store(CreateApprenticeship(Ukprn, Uln, StandardCode, employerType));

            var result = await repository.GetEmployerType(Ukprn, Uln, StandardCode);

            result.Should().Be(employerType);
        }

        [Test]
        public async Task Prefers_An_Active_Apprenticeship_Over_A_Stopped_One_Agreed_More_Recently()
        {
            await Store(CreateApprenticeship(Ukprn, Uln, StandardCode, ApprenticeshipEmployerType.NonLevy, ApprenticeshipStatus.Active, new DateTime(2026, 9, 1)));
            await Store(CreateApprenticeship(Ukprn, Uln, StandardCode, ApprenticeshipEmployerType.Levy, ApprenticeshipStatus.Stopped, new DateTime(2026, 10, 1)));

            var result = await repository.GetEmployerType(Ukprn, Uln, StandardCode);

            result.Should().Be(ApprenticeshipEmployerType.NonLevy);
        }

        [Test]
        public async Task Returns_The_Most_Recently_Agreed_Apprenticeship_When_Several_Share_The_Same_Status()
        {
            await Store(CreateApprenticeship(Ukprn, Uln, StandardCode, ApprenticeshipEmployerType.Levy, ApprenticeshipStatus.Active, new DateTime(2026, 8, 1)));
            await Store(CreateApprenticeship(Ukprn, Uln, StandardCode, ApprenticeshipEmployerType.NonLevy, ApprenticeshipStatus.Active, new DateTime(2026, 10, 1)));

            var result = await repository.GetEmployerType(Ukprn, Uln, StandardCode);

            result.Should().Be(ApprenticeshipEmployerType.NonLevy);
        }

        private async Task Store(ApprenticeshipModel apprenticeship)
        {
            dataContext.Apprenticeship.Add(apprenticeship);
            await dataContext.SaveChangesAsync();
        }

        private static ApprenticeshipModel CreateApprenticeship(
            long ukprn,
            long uln,
            long standardCode,
            ApprenticeshipEmployerType employerType,
            ApprenticeshipStatus status = ApprenticeshipStatus.Active,
            DateTime? agreedOnDate = null)
        {
            return new ApprenticeshipModel
            {
                Ukprn = ukprn,
                Uln = uln,
                StandardCode = standardCode,
                AccountId = 310074,
                AgreedOnDate = agreedOnDate ?? new DateTime(2026, 9, 1),
                EstimatedStartDate = new DateTime(2026, 10, 1),
                EstimatedEndDate = new DateTime(2027, 10, 1),
                Priority = 1,
                Status = status,
                IsLevyPayer = employerType == ApprenticeshipEmployerType.Levy,
                ApprenticeshipEmployerType = employerType
            };
        }
    }
}
