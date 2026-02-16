using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Model;
using Microsoft.Extensions.DependencyInjection;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;

public interface IGSLEarningsMapper
{
    ShortCourseEarningModel MapToShortCourseEarningModel(CalculateGSLPayments source);
    List<ReceivedDASEarningsMessageModel> MapToReceivedDASEarningsMessageModel(CalculateGSLPayments source);
}