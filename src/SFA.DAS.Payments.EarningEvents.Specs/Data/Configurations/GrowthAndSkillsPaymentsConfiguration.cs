using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.Payments.EarningEvents.Model;

namespace SFA.DAS.Payments.EarningEvents.Specs.Data.Configurations;

public class GrowthAndSkillsPaymentsConfiguration
    : IEntityTypeConfiguration<GrowthAndSkillsEarningModel>
{
    public void Configure(EntityTypeBuilder<GrowthAndSkillsEarningModel> builder)
    {
        builder.ToTable("GrowthAndSkillsEarning", "Payments2");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("Id").IsRequired();

        builder.Property(x => x.EarningsId)
            .HasColumnName("EarningsId")
            .IsRequired();

        builder.Property(x => x.UKPRN)
            .HasColumnName("UKPRN")
            .IsRequired();

        builder.Property(x => x.LearnerKey)
            .HasColumnName("LearnerKey")
            .IsRequired();

        builder.Property(x => x.LearnerUln)
            .HasColumnName("LearnerUln")
            .IsRequired();

        builder.Property(x => x.LearnerReference)
            .HasColumnName("LearnerReference");

        builder.Property(x => x.LearningType)
            .HasColumnName("LearningType");

        builder.Property(x => x.CourseCode)
            .HasColumnName("CourseCode");

        builder.Property(x => x.CourseReference)
            .HasColumnName("CourseReference");

        builder.Property(x => x.StartDate)
            .HasColumnName("StartDate");

        builder.Property(x => x.AgeAtStartOfTraining)
            .HasColumnName("AgeAtStartOfTraining");

        builder.Property(x => x.PlannedEndDate)
            .HasColumnName("PlannedEndDate");

        builder.Property(x => x.ActualEndDate)
            .HasColumnName("ActualEndDate");

        builder.Property(x => x.TrainingStatus)
            .HasColumnName("TrainingStatus");

        builder.Property(x => x.EmployerContribution)
            .HasColumnName("EmployerContribution");

        builder.Property(x => x.CourseType)
            .HasColumnName("CourseType");

        builder.Property(x => x.LearningKey)
            .HasColumnName("LearningKey");
    }
}