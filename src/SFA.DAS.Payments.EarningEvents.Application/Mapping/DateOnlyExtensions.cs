using System;

namespace SFA.DAS.Payments.EarningEvents.Application.Mapping
{
    //TODO: move into SFA.DAS.Payments.Core

    public static class DateOnlyExtensions
    {
        public static byte GetPeriodFromDate(this DateOnly date)
        {
            byte period;
            var month = date.Month;

            if (month < 8)
            {
                period = (byte)(month + 5);
            }
            else
            {
                period = (byte)(month - 7);
            }
            return period;
        }
    }
}
