using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Model;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;

public interface IGSLearningsMapper
{
    GrowthAndSkillsEarningModel MapToGrowthAndSkillsEarningModel(CalculateGrowthAndSkillsPayments source);    
    GSLShortCourseEarningsEvent MapToShortCourseEarningEvent(CalculateGrowthAndSkillsPayments source, short academicYear, byte collectionPeriod);

    List<ReceivedDASEarningsMessageModel> MapToReceivedDASEarningsMessageModel(CalculateGrowthAndSkillsPayments source);
}
