using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.Payments.EarningEvents.Model;

namespace SFA.DAS.Payments.EarningEvents.Data.Configuration
{
    internal class GrowthAndSkillsEarningModelConfiguration : IEntityTypeConfiguration<GrowthAndSkillsEarningModel>
    {
        public void Configure(EntityTypeBuilder<GrowthAndSkillsEarningModel> builder)
        {
            builder.ToTable("GrowthAndSkillsEarning", "Payments2");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("Id");
            builder.Property(x => x.EarningsId).HasColumnName("EarningsId").IsRequired();
            builder.Property(x => x.UKPRN).HasColumnName("UKPRN").IsRequired();
            builder.Property(x => x.LearnerKey).HasColumnName("LearnerKey").IsRequired();
            builder.Property(x => x.LearnerUln).HasColumnName("LearnerUln").IsRequired();
            builder.Property(x => x.LearnerReference).HasColumnName("LearnerReference").IsRequired();
            builder.Property(x => x.LearningType).HasColumnName("LearningType").IsRequired();
            builder.Property(x => x.CourseCode).HasColumnName("CourseCode").IsRequired();
            builder.Property(x => x.CourseReference).HasColumnName("CourseReference").IsRequired();
            builder.Property(x => x.StartDate).HasColumnName("StartDate").IsRequired();
            builder.Property(x => x.AgeAtStartOfTraining).HasColumnName("AgeAtStartOfTraining").IsRequired();
            builder.Property(x => x.PlannedEndDate).HasColumnName("PlannedEndDate").IsRequired();
            builder.Property(x => x.ActualEndDate).HasColumnName("ActualEndDate");
            builder.Property(x => x.TrainingStatus).HasColumnName("TrainingStatus").IsRequired();
            builder.Property(x => x.EmployerContribution).HasColumnName("EmployerContribution").IsRequired().HasColumnType("decimal(15,5)");
            builder.Property(x => x.CourseType).HasColumnName("CourseType").IsRequired();
            builder.Property(x => x.LearningKey).HasColumnName("LearningKey").IsRequired();
        }
    }
}
