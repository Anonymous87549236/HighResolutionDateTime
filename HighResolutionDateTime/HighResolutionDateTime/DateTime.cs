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
    using System.Security;
    using static CoreLib;

    // https://referencesource.microsoft.com/#system/services/monitoring/system/diagnosticts/Stopwatch.cs
    // This class uses high-resolution performance counter if installed hardware
    // supports it. Otherwise, the class will fall back to System.DateTime class.

    public static class DateTime
    {
        // https://programtalk.com/vs4/csharp/Abc-Arbitrage/ZeroLog/src/ZeroLog/Utils/HighResolutionDateTime.cs/
#if !NETCOREAPP
        // Number of 100ns ticks per time unit
        internal const long TicksPerMillisecond = 10000;
        private const long TicksPerSecond = TicksPerMillisecond * 1000;
        private const long TicksPerMinute = TicksPerSecond * 60;
        private const long TicksPerHour = TicksPerMinute * 60;
        private const long TicksPerDay = TicksPerHour * 24;

        // Number of days in a non-leap year
        private const int DaysPerYear = 365;
        // Number of days in 4 years
        private const int DaysPer4Years = DaysPerYear * 4 + 1;       // 1461
        // Number of days in 100 years
        private const int DaysPer100Years = DaysPer4Years * 25 - 1;  // 36524
        // Number of days in 400 years
        private const int DaysPer400Years = DaysPer100Years * 4 + 1; // 146097

        // Number of days from 1/1/0001 to 12/31/1600
        private const int DaysTo1601 = DaysPer400Years * 4;          // 584388

        private const long FileTimeOffset = DaysTo1601 * TicksPerDay;

        internal static readonly bool s_isLeapSecondsSupportedSystem = SystemSupportLeapSeconds();

        private static readonly int[] DaysToMonth365 = {
            0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334, 365};
        private static readonly int[] DaysToMonth366 = {
            0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335, 366};

        // A value indicating whether high resolution DateTime is supported
        public static readonly HighResolutionDateTimeSource source;
        private static StopwatchDateTime StopwatchDateTime;
        public static StopwatchDateTimeDisposalState StopwatchDateTimeDisposalState { get; private set; }
        // full name is waitForSystemDateTimeToChangeIfGetSystemTimePreciseAsFileTimeIsNotAvailable
        public static bool isAccurateButSlow = true;
        public static long? StopwatchDateTimeMaxIdleTime
        {
            get
            {
                if (source != HighResolutionDateTimeSource.Stopwatch)
                {
                    return null;
                }
                return StopwatchDateTime._maxIdleTime;
            }
            set
            {
                if ((source == HighResolutionDateTimeSource.Stopwatch) && value.HasValue)
                {
                    StopwatchDateTime._maxIdleTime = value.Value;
                }
            }
        }

#if FEATURE_NETCORE
        [SecuritySafeCritical]
#endif
        static DateTime()
        {
            // https://manski.net/2014/07/high-resolution-clock-in-csharp/#high-resolution-clock
            long ticks = 0;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    GetSystemTimePreciseAsFileTime(out ticks);
                    source = HighResolutionDateTimeSource.GetSystemTimePreciseAsFileTime;
                }
                // Not running Windows 8, Windows Server 2012 or higher.
                catch (DllNotFoundException) { }
                catch (EntryPointNotFoundException) { }
            }
            if (source == HighResolutionDateTimeSource.GetSystemTimePreciseAsFileTime)
            {
                CompatibilitySwitches.InitializeSwitches();
                if (s_isLeapSecondsSupportedSystem)
                {
                    FullSystemTime time = new FullSystemTime();
                    try
                    {
                        GetSystemTimeWithLeapSecondsHandling(out time, ref ticks);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        source = HighResolutionDateTimeSource.None;
                    }
                }
            }
            if ((source < HighResolutionDateTimeSource.Stopwatch) && System.Diagnostics.Stopwatch.IsHighResolution)
            {
                source = HighResolutionDateTimeSource.Stopwatch;
            };
            switch (source)
            {
                case HighResolutionDateTimeSource.GetSystemTimePreciseAsFileTime:
                    break;
                case HighResolutionDateTimeSource.Stopwatch:
                    StopwatchDateTime = new StopwatchDateTime();
                    StopwatchDateTimeDisposalState = StopwatchDateTimeDisposalState.NotDisposed;
                    goto default;
                default:
                    // including that of the system clock timer
                    TimerResolutionChanger.SetTimerResolutionToMax();
                    break;
            }
        }

        // Returns the tick count corresponding to the given year, month, and day.
        // Will check the if the parameters are valid.
        private static long DateToTicks(int year, int month, int day)
        {
            if (year >= 1 && year <= 9999 && month >= 1 && month <= 12)
            {
                int[] days = IsLeapYear(year) ? DaysToMonth366 : DaysToMonth365;
                if (day >= 1 && day <= days[month] - days[month - 1])
                {
                    int y = year - 1;
                    int n = y * 365 + y / 4 - y / 100 + y / 400 + days[month - 1] + day - 1;
                    return n * TicksPerDay;
                }
            }
            throw new ArgumentOutOfRangeException(null, "ArgumentOutOfRange_BadYearMonthDay");
        }

        // Return the tick count corresponding to the given hour, minute, second.
        // Will check the if the parameters are valid.
        private static long TimeToTicks(int hour, int minute, int second)
        {
            //TimeSpan.TimeToTicks is a family access function which does no error checking, so
            //we need to put some error checking out here.
            if (hour >= 0 && hour < 24 && minute >= 0 && minute < 60 && second >= 0 && second < 60)
            {
                return (TimeSpan.TimeToTicks(hour, minute, second));
            }
            throw new ArgumentOutOfRangeException(null, "ArgumentOutOfRange_BadHourMinuteSecond");
        }
#endif

        // Returns a DateTime representing the current date and time.
        //
        public static System.DateTime Now
        {
#if NETCOREAPP
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => System.DateTime.Now;
#else
            get
            {
                Contract.Ensures(Contract.Result<System.DateTime>().Kind == DateTimeKind.Local);

                switch (source)
                {
                    case HighResolutionDateTimeSource.GetSystemTimePreciseAsFileTime:
                    case HighResolutionDateTimeSource.Stopwatch:
                        return DateTime.UtcNow.ToLocalTime();
                    default:
                        if (isAccurateButSlow)
                        {
                            WaitForIncrease();
                        }
                        return System.DateTime.Now;
                }
            }
#endif
        }

        public static System.DateTime UtcNow
        {
            [System.Security.SecuritySafeCritical]  // auto-generated
#if NETCOREAPP
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => System.DateTime.UtcNow;
#else
            get
            {
                Contract.Ensures(Contract.Result<System.DateTime>().Kind == DateTimeKind.Utc);
                switch (source)
                {
                    case HighResolutionDateTimeSource.GetSystemTimePreciseAsFileTime:
                        // following code is tuned for resolution at the expense of speed.
                        long ticks;

                        GetSystemTimePreciseAsFileTime(out ticks);

                        if (s_isLeapSecondsSupportedSystem)
                        {
                            FullSystemTime time = new FullSystemTime();
                            GetSystemTimeWithLeapSecondsHandling(out time, ref ticks);
                            return CreateDateTimeFromSystemTime(ref time);
                        }

#if FEATURE_LEGACYNETCF
                        // Windows Phone 7.0/7.1 return the ticks up to millisecond, not up to the 100th nanosecond.
                        if (CompatibilitySwitches.IsAppEarlierThanWindowsPhone8)
                        {
                            long ticksms = ticks / TicksPerMillisecond;
                            ticks = ticksms * TicksPerMillisecond;
                        }
#endif
                        return new System.DateTime(ticks + FileTimeOffset, DateTimeKind.Utc);
                    case HighResolutionDateTimeSource.Stopwatch:
                        return StopwatchDateTime.UtcNow;
                    default:
                        if (isAccurateButSlow)
                        {
                            WaitForIncrease();
                        }
                        return System.DateTime.UtcNow;
                }
            }
#endif
        }

#if !NETCOREAPP
        // FullSystemTime struct matches Windows SYSTEMTIME struct, except we added the extra nanoSeconds field to store
        // more precise time.
        [StructLayout(LayoutKind.Sequential)]
        internal struct FullSystemTime
        {
            internal ushort wYear;
            internal ushort wMonth;
            internal ushort wDayOfWeek;
            internal ushort wDay;
            internal ushort wHour;
            internal ushort wMinute;
            internal ushort wSecond;
            internal ushort wMillisecond;
            internal long hundredNanoSecond;
        };

        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi), SuppressUnmanagedCodeSecurity]
        internal static extern void GetSystemTimePreciseAsFileTime(out long fileTime);

        // https://gist.github.com/tarekgh/4f038db754a621fb38b17cf9fc7c0b3d
        private const int SystemLeapSecondInformation = 206;

        [DllImport("ntdll.dll", EntryPoint = "NtQuerySystemInformation", SetLastError = true)]
        private static extern int NtQuerySystemInformation(int SystemInformationClass, ref SYSTEM_LEAP_SECOND_INFORMATION SystemInformation, int SystemInformationLength, IntPtr ReturnLength);

        internal static unsafe bool IsLeapSecondsSupportedSystem()
        {
            SYSTEM_LEAP_SECOND_INFORMATION slsi = new SYSTEM_LEAP_SECOND_INFORMATION();
            return NtQuerySystemInformation(SystemLeapSecondInformation, ref slsi, sizeof(SYSTEM_LEAP_SECOND_INFORMATION), IntPtr.Zero) == 0 && slsi.Enabled;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEM_LEAP_SECOND_INFORMATION
        {
            public bool Enabled;
            public uint Flags;
        }

        [System.Security.SecuritySafeCritical]
        internal static bool SystemSupportLeapSeconds()
        {
            return IsLeapSecondsSupportedSystem();
        }

        // Just in case for any reason CreateDateTimeFromSystemTime not get inlined,
        // we are passing time by ref to avoid copying the structure while calling the method.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static System.DateTime CreateDateTimeFromSystemTime(ref FullSystemTime time)
        {
            long ticks = DateToTicks(time.wYear, time.wMonth, time.wDay);
            ticks += TimeToTicks(time.wHour, time.wMinute, time.wSecond);
            ticks += time.wMillisecond * TicksPerMillisecond;
            ticks += time.hundredNanoSecond;
            return new System.DateTime(ticks, System.DateTimeKind.Utc);
        }
#endif

        // Returns a DateTime representing the current date. The date part
        // of the returned value is the current date, and the time-of-day part of
        // the returned value is zero (midnight).
        //
        public static System.DateTime Today
        {
#if NETCOREAPP
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => System.DateTime.Today;
#else
            get
            {
                return DateTime.Now.Date;
            }
#endif
        }

#if !NETCOREAPP
        // Checks whether a given year is a leap year. This method returns true if
        // year is a leap year, or false if not.
        //
        internal static bool IsLeapYear(int year)
        {
            if (year < 1 || year > 9999)
            {
                throw new ArgumentOutOfRangeException("year", "ArgumentOutOfRange_Year");
            }
            Contract.EndContractBlock();
            return year % 4 == 0 && (year % 100 != 0 || year % 400 == 0);
        }

        public static void StopwatchDateTimeDispose()
        {
            if (StopwatchDateTimeDisposalState != StopwatchDateTimeDisposalState.NotDisposed)
            {
                return;
            }
            StopwatchDateTime.Dispose();
            StopwatchDateTimeDisposalState = StopwatchDateTimeDisposalState.Disposed;
        }

        public static void StopwatchDateTimeCreate()
        {
            if (StopwatchDateTimeDisposalState != StopwatchDateTimeDisposalState.Disposed)
            {
                return;
            }
            StopwatchDateTime = new StopwatchDateTime();
            StopwatchDateTimeDisposalState = StopwatchDateTimeDisposalState.NotDisposed;
        }

        internal static void WaitForIncrease()
        {
            var previousDateTime = System.DateTime.UtcNow;
            while (previousDateTime == System.DateTime.UtcNow) { }
        }
#endif
    }
}