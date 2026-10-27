using Microsoft.Extensions.Logging;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Logging
{
    public static class EarningsBridgeLogExtensions
    {
        public static void LogEarningsBridgeTrace(this ILogger logger, string eventName, object? properties)
        {
            logger.LogWarning("{EventName} {@Properties}", eventName, properties ?? new { });
        }

        public static void LogEarningsBridgeError(this ILogger logger, Exception ex, string eventName, object? properties)
        {
            logger.LogError(ex, "{EventName} {@Properties}", eventName, properties ?? new { });
        }
    }
}