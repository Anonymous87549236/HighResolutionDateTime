// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
// https://referencesource.microsoft.com/#mscorlib/system/timespan.cs
namespace HighResolutionDateTime
{
    using System;

    [System.Runtime.InteropServices.ComVisible(true)]
    [Serializable]
    internal struct TimeSpan
    {
        public const long TicksPerMillisecond = 10000;

        public const long TicksPerSecond = TicksPerMillisecond * 1000;   // 10,000,000

        internal const long MaxSeconds = Int64.MaxValue / TicksPerSecond;
        internal const long MinSeconds = Int64.MinValue / TicksPerSecond;

        internal static long TimeToTicks(int hour, int minute, int second)
        {
            // totalSeconds is bounded by 2^31 * 2^12 + 2^31 * 2^8 + 2^31,
            // which is less than 2^44, meaning we won't overflow totalSeconds.
            long totalSeconds = (long)hour * 3600 + (long)minute * 60 + (long)second;
            if (totalSeconds > MaxSeconds || totalSeconds < MinSeconds)
                throw new ArgumentOutOfRangeException(null, "Overflow_TimeSpanTooLong");
            return totalSeconds * TicksPerSecond;
        }
    }
}