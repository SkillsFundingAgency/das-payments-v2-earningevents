using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.Payments.EarningEvents.Model;

namespace SFA.DAS.Payments.EarningEvents.Data.Configuration
{
    internal class ReceivedEarningProcessedCollectionPeriodConfiguration : IEntityTypeConfiguration<ReceivedEarningProcessedCollectionPeriod>
    {
        public void Configure(EntityTypeBuilder<ReceivedEarningProcessedCollectionPeriod> builder)
        {
            builder.ToTable("ReceivedEarningProcessedCollectionPeriod", "Payments2");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("Id");
            builder.Property(x => x.ReceivedEarningId).HasColumnName("ReceivedEarningId").IsRequired();
            builder.Property(x => x.AcademicYear).HasColumnName("AcademicYear").IsRequired();
            builder.Property(x => x.CollectionPeriod).HasColumnName("CollectionPeriod").IsRequired();
            builder.Property(x => x.InteranlEarningId).HasColumnName("InteranlEarningId").IsRequired();
            builder.Property(x => x.ProcessedOn).HasColumnName("ProcessedOn").IsRequired();
            builder.HasOne(x => x.ReceivedEarning)
                .WithMany(x => x.ProcessedCollectionPeriods)
                .HasForeignKey(x => x.ReceivedEarningId);
        }
    }
}
