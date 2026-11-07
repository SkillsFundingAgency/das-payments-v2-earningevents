using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Mapping
{
    public interface IReceivedEarningsMapper
    {
        ReceivedEarning Map(CalculateGrowthAndSkillsPayments calculateGrowthAndSkillsPayments);
    }

    public class ReceivedEarningsMapper
    {
        public ReceivedEarning Map(CalculateGrowthAndSkillsPayments calculateGrowthAndSkillsPayments)
        {
            var receivedEarning = new ReceivedEarning
            {
                EarningsId = calculateGrowthAndSkillsPayments.EarningsId,
                UKPRN = calculateGrowthAndSkillsPayments.UKPRN,
                LearnerUln = calculateGrowthAndSkillsPayments.Learner.ULN,
                LearnerReference = calculateGrowthAndSkillsPayments.Learner.Reference,
                LearnerKey = calculateGrowthAndSkillsPayments.Learner.LearnerKey,
                CourseType = calculateGrowthAndSkillsPayments.Training.CourseType.ToModelCourseType(),
                LearningKey = calculateGrowthAndSkillsPayments.Training.LearningKey,
                LearningType = calculateGrowthAndSkillsPayments.Training.LearningType.ToModelLearningType(),
                CourseCode = calculateGrowthAndSkillsPayments.Training.CourseCode,
                CourseReference = calculateGrowthAndSkillsPayments.Training.CourseReference,
                StartDate = calculateGrowthAndSkillsPayments.Training.StartDate,
                Payload = System.Text.Json.JsonSerializer.Serialize(calculateGrowthAndSkillsPayments)
            };
            return receivedEarning;
        }
    }
}
