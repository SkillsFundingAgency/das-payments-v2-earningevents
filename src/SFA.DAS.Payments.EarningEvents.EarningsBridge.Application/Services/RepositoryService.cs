using Microsoft.Extensions.Logging;
using UUIDNext.Tools;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services
{
    public class RepositoryService : IRepositoryService
    {
        private readonly ILogger<RepositoryService> _logger;

        public RepositoryService(ILogger<RepositoryService> logger)
        {
            _logger = logger;
        }
        public bool CheckEarningsAreLatest(Guid messageTimestamp, Guid tableEntryTimestamp)
        {
            var messageDecode = UuidDecoder.TryDecodeTimestamp(messageTimestamp, out var messageTime);
            var tableDecode = UuidDecoder.TryDecodeTimestamp(tableEntryTimestamp, out var tableTime);

            if (messageDecode && tableDecode)
            {
                if (messageTime == tableTime)
                {
                    return false;
                }

                return messageTime > tableTime;
            }
            return false;
        }
    }
}
