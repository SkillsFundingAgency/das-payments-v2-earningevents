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
    
}