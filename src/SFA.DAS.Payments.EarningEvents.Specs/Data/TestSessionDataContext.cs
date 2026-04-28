using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.EarningEvents.Specs.Data.Configurations;
using SFA.DAS.Payments.EarningEvents.Specs.Models;
using SFA.DAS.Payments.EarningEvents.Specs.Data.Configurations;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.EarningEvents.Specs.Data;

public class TestSessionDataContext : DbContext
{
    private readonly string connectionString;

    public virtual DbSet<Provider> Providers { get; set; }
    public virtual DbSet<PaymentModel> Payment { get; set; }
    public virtual DbSet<CollectionPeriodModel> CollectionPeriods { get; set; }

    public TestSessionDataContext(string connectionString)
    {
        this.connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(connectionString, options => options.CommandTimeout(600));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("Payments2");
        modelBuilder.ApplyConfiguration(new ProviderConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentModelConfiguration());
        modelBuilder.ApplyConfiguration(new CollectionPeriodModelConfiguration());
    }

    public Provider LeastRecentlyUsed() =>
        Providers.OrderBy(x => x.LastUsed).FirstOrDefault()
        ?? throw new InvalidOperationException("There are no UKPRNs available in the well-known Providers pool.");


    private const string DeleteCollectionPeriodsByYear = @"
            delete from Payments2.CollectionPeriod where CollectionYear = {0}
        ";

    private const string DeleteCollectionPeriods = @"
            delete from Payments2.CollectionPeriod 
        ";

    public async Task ClearCollectionPeriodsData(int collectionYear)
    {
        await Database.ExecuteSqlRawAsync(DeleteCollectionPeriodsByYear, collectionYear);
    }

    public async Task ClearCollectionPeriodsData()
    {
        await Database.ExecuteSqlRawAsync(DeleteCollectionPeriods);
    }

    //private const string DeleteUkprnData = @"
    //        delete from Payments2.LevyAccount where AccountId in
    //         (select AccountId from Payments2.Apprenticeship where Ukprn = {0})

    //        delete from Payments2.ApprenticeshipPriceEpisode where ApprenticeshipId in 
    //         (select Id from Payments2.Apprenticeship where Ukprn = {0})

    //        delete from Payments2.ApprenticeshipPause where ApprenticeshipId in 
    //         (select Id from Payments2.Apprenticeship where Ukprn = {0})

    //        delete from Payments2.ApprenticeshipDuplicate where ApprenticeshipId in
    //         (select Id from Payments2.Apprenticeship where Ukprn = {0} )

    //        delete from Payments2.DataLockEventNonPayablePeriodFailures where ApprenticeshipId in
    //         (select Id from Payments2.Apprenticeship where Ukprn = {0} )

    //        delete from Payments2.Apprenticeship where Ukprn = {0}

    //        delete from Payments2.DataLockEventNonPayablePeriod where DataLockEventId in 
    //         (select EventId from Payments2.DataLockEvent where Ukprn = {0})

    //        delete from Payments2.DataLockEventPayablePeriod where DataLockEventId in 
    //         (select EventId from Payments2.DataLockEvent where Ukprn = {0})

    //        delete from Payments2.DataLockEventPriceEpisode where DataLockEventId in 
    //         (select EventId from Payments2.DataLockEvent where Ukprn = {0})

    //        delete from Payments2.DataLockFailure where Ukprn = {0}

    //        delete from Payments2.DataLockEvent where Ukprn = {0}

    //        delete from Payments2.EarningEventPeriod where EarningEventId in 
    //         (select EventId from Payments2.EarningEvent where Ukprn = {0})

    //        delete from Payments2.EarningEventPriceEpisode where EarningEventId in 
    //         (select EventId from Payments2.EarningEvent where Ukprn = {0})

    //        delete from Payments2.EarningEvent where Ukprn = {0}

    //        delete from Payments2.EmployerProviderPriority where Ukprn = {0}

    //        delete from Payments2.FundingSourceEvent where Ukprn = {0}

    //        delete from Payments2.RequiredPaymentEvent where Ukprn = {0}

    //        delete from Payments2.Payment where Ukprn = {0}

    //        delete from Payments2.SubmittedLearnerAim where Ukprn = {0}

    //        delete from Payments2.FundingSourceLevyTransaction where Ukprn = {0}
    //    ";




}