// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// https://github.com/dotnet/corert/blob/master/src/System.Private.CoreLib/shared/System/DateTime.Windows.cs

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static HighResolutionDateTime.DateTime;

namespace HighResolutionDateTime
{
    internal readonly partial struct CoreLib
    {
        private static bool PrivateFileTimeToSystemTime(ref long fileTime, out FullSystemTime time)
        {
            // Interop.Kernel32.FileTimeToSystemTime
            if (!FileTimeToSystemTime(ref fileTime, out time))
            {
                // to keep the time precision
                time.hundredNanoSecond = fileTime % TicksPerMillisecond;
                if (time.wSecond > 59)
                {
                    // we have a leap second, force it to last second in the minute as DateTime doesn't account for leap seconds in its calculation.
                    // we use the maxvalue from the milliseconds and the 100-nano seconds to avoid reporting two out of order 59 seconds
                    time.wSecond = 59;
                    time.wMillisecond = 999;
                    time.hundredNanoSecond = 9999;
                }
                return true;
            }
            return false;
        }

        internal static void GetSystemTimeWithLeapSecondsHandling(out FullSystemTime time, ref long fileTime)
        {
            if (!PrivateFileTimeToSystemTime(ref fileTime, out time))
            {
                // Interop.Kernel32.GetSystemTime(&time)
                throw new ArgumentOutOfRangeException("fileTime", "ArgumentOutOfRange_DateTimeBadTicks");
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        internal static extern bool FileTimeToSystemTime(ref long fileTime, out FullSystemTime time);
    }
}