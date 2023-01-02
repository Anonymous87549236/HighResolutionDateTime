// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
// https://referencesource.microsoft.com/#mscorlib/system/datetime.cs
namespace HighResolutionDateTime
{

    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using System.Diagnostics.Contracts;

    // https://referencesource.microsoft.com/#system/services/monitoring/system/diagnosticts/Stopwatch.cs
    // This class uses high-resolution performance counter if installed hardware
    // does not (mistake?) support it. Otherwise, the class will fall back to
    // System.DateTime class.

    public static class DateTime
    {
        // Number of 100ns ticks per time unit
        private const long TicksPerMillisecond = 10000;

        // A value indicating whether high resolution DateTime is supported
        public static readonly HighResolutionDateTimeSource source;
        private static StopwatchDateTime StopwatchDateTime;
        public static StopwatchDateTimeDisposalState StopwatchDateTimeDisposalState { get; private set; }

#if FEATURE_NETCORE
        [SecuritySafeCritical]
#endif
        static DateTime()
        {
            // https://manski.net/2014/07/high-resolution-clock-in-csharp/#high-resolution-clock
            try
            {
                long ticks;
                GetSystemTimePreciseAsFileTime(out ticks);
                source = HighResolutionDateTimeSource.GetSystemTimePreciseAsFileTime;
            }
            // Not running Windows 8, Windows Server 2012 or higher.
            catch (EntryPointNotFoundException) { }
            if ((source < HighResolutionDateTimeSource.Stopwatch) && System.Diagnostics.Stopwatch.IsHighResolution)
            {
                source = HighResolutionDateTimeSource.Stopwatch;
            };
            switch (source)
            {
                case HighResolutionDateTimeSource.GetSystemTimePreciseAsFileTime:
                    CompatibilitySwitches.InitializeSwitches();
                    break;
                case HighResolutionDateTimeSource.Stopwatch:
                    StopwatchDateTime = new StopwatchDateTime();
                    StopwatchDateTimeDisposalState = StopwatchDateTimeDisposalState.NotDisposed;
                    break;
            }
        }

        // Returns a DateTime representing the current date and time.
        //
        public static System.DateTime Now
        {
            get
            {
                Contract.Ensures(Contract.Result<System.DateTime>().Kind == DateTimeKind.Local);

                switch (source)
                {
                    case HighResolutionDateTimeSource.GetSystemTimePreciseAsFileTime:
                        // following code is tuned for resolution at the expense of speed.
                        long ticks;
                        GetSystemTimePreciseAsFileTime(out ticks);
#if FEATURE_LEGACYNETCF
                        // Windows Phone 7.0/7.1 return the ticks up to millisecond, not up to the 100th nanosecond.
                        if (CompatibilitySwitches.IsAppEarlierThanWindowsPhone8)
                        {
                            // ticks = ticks. Bug?
                            long ticksms = ticks / TicksPerMillisecond;
                            ticks = ticksms * TicksPerMillisecond;
                        }
#endif
                        return System.DateTime.FromFileTime(ticks);
                    case HighResolutionDateTimeSource.Stopwatch:
                        return StopwatchDateTime.UtcNow.ToLocalTime();
                    default:
                        return System.DateTime.Now;
                }
            }
        }

        public static System.DateTime UtcNow
        {
            [System.Security.SecuritySafeCritical]  // auto-generated
            get
            {
                Contract.Ensures(Contract.Result<System.DateTime>().Kind == DateTimeKind.Utc);
                switch (source)
                {
                    case HighResolutionDateTimeSource.GetSystemTimePreciseAsFileTime:
                        // following code is tuned for resolution at the expense of speed.
                        long ticks;

                        GetSystemTimePreciseAsFileTime(out ticks);

#if FEATURE_LEGACYNETCF
                        // Windows Phone 7.0/7.1 return the ticks up to millisecond, not up to the 100th nanosecond.
                        if (CompatibilitySwitches.IsAppEarlierThanWindowsPhone8)
                        {
                            // ticks = ticks. Bug?
                            long ticksms = ticks / TicksPerMillisecond;
                            ticks = ticksms * TicksPerMillisecond;
                        }
#endif
                        return System.DateTime.FromFileTimeUtc(ticks);
                    case HighResolutionDateTimeSource.Stopwatch:
                        return StopwatchDateTime.UtcNow;
                    default:
                        return System.DateTime.UtcNow;
                }
            }
        }

        // Returns a DateTime representing the current date. The date part
        // of the returned value is the current date, and the time-of-day part of
        // the returned value is zero (midnight).
        //
        public static System.DateTime Today
        {
            get
            {
                return DateTime.Now.Date;
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        // internal static extern long GetSystemTimePreciseAsFileTime(); // does not work
        internal static extern void GetSystemTimePreciseAsFileTime(out long lpSystemTimeAsFileTime);

        public static void StopwatchDateTimeDispose()
        {
            if ((StopwatchDateTimeDisposalState == StopwatchDateTimeDisposalState.NotDisposable) || (StopwatchDateTimeDisposalState == StopwatchDateTimeDisposalState.Disposed))
            {
                return;
            }
            StopwatchDateTime.Dispose();
            StopwatchDateTimeDisposalState = StopwatchDateTimeDisposalState.Disposed;
        }

        public static void StopwatchDateTimeCreate()
        {
            if ((StopwatchDateTimeDisposalState == StopwatchDateTimeDisposalState.NotDisposable) || (StopwatchDateTimeDisposalState == StopwatchDateTimeDisposalState.NotDisposed))
            {
                return;
            }
            StopwatchDateTime = new StopwatchDateTime();
            StopwatchDateTimeDisposalState = StopwatchDateTimeDisposalState.NotDisposed;
        }
    }
}