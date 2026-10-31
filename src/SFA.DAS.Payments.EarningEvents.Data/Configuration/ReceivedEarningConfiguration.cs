using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.Payments.EarningEvents.Model;

namespace SFA.DAS.Payments.EarningEvents.Data.Configuration
{
    internal class ReceivedEarningConfiguration : IEntityTypeConfiguration<ReceivedEarning>
    {
        public void Configure(EntityTypeBuilder<ReceivedEarning> builder)
        {
            builder.ToTable("ReceivedEarning", "Payments2");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("Id");
            builder.Property(x => x.EarningsId).HasColumnName("EarningsId").IsRequired();
            builder.Property(x => x.UKPRN).HasColumnName("UKPRN").IsRequired();
            builder.Property(x => x.LearnerUln).HasColumnName("LearnerUln").IsRequired();
            builder.Property(x => x.LearnerReference).HasColumnName("LearnerReference").IsRequired();
            builder.Property(x => x.LearnerKey).HasColumnName("LearnerKey").IsRequired();
            builder.Property(x => x.CourseType).HasColumnName("CourseType").IsRequired();
            builder.Property(x => x.LearningType).HasColumnName("LearningType").IsRequired();
            builder.Property(x => x.LearningKey).HasColumnName("LearningKey").IsRequired(); ;
            builder.Property(x => x.CourseCode).HasColumnName("CourseCode").IsRequired();
            builder.Property(x => x.CourseReference).HasColumnName("CourseReference").IsRequired();
            builder.Property(x => x.StartDate).HasColumnName("StartDate").IsRequired();
            builder.Property(x => x.Payload).HasColumnName("Payload").IsRequired();
            builder.HasMany(x => x.ProcessedCollectionPeriods)
                .WithOne(x => x.ReceivedEarning)
                .HasForeignKey(x => x.ReceivedEarningId);
        }
    }
}
