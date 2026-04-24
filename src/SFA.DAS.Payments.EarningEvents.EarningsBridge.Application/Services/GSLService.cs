using SFA.DAS.Payments.EarningEvents.Model;
using UUIDNext.Tools;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;

public class GSLService : IGSLService
{
    public bool CheckEarningsAreLatest(List<GrowthAndSkillsEarningModel> earnings, Guid messageEarningsId)
    {
        // If there are no earnings for the given UKPRN, ULN, and CourseCode, we can consider the current earnings as the latest.
        if (earnings.Count == 0) return true;

        foreach (var earning in earnings)
        {
            var tableEarningsId = earning.EarningsId;
            var isLatest = CompareTimestamps(messageEarningsId,
                tableEarningsId);

            if (!isLatest) return false;
        }

        return true;
    }

    private bool CompareTimestamps(Guid messageTimestamp, Guid tableEntryTimestamp)
    {
        var messageDecode = UuidDecoder.TryDecodeTimestamp(messageTimestamp, out var messageTime);
        var tableDecode = UuidDecoder.TryDecodeTimestamp(tableEntryTimestamp, out var tableTime);

        if (messageDecode && tableDecode)
        {
            if (messageTime == tableTime) return false;

            return messageTime > tableTime;
        }

        return false;
    }
}