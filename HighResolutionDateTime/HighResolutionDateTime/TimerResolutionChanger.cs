// helper class from https://aakinshin.net/posts/datetime/

using System.Runtime.InteropServices;

namespace HighResolutionDateTime
{
    public static class TimerResolutionChanger
    {
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern uint NtQueryTimerResolution(out uint min, out uint max, out uint current);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern uint NtSetTimerResolution(uint desiredResolution, bool setResolution, ref uint currentResolution);

        // in 100 ns
        public static readonly uint maxTimerResolution;
        private static uint currentTimerResolution;
        public static bool isTimerResolutionSetToMax { get; private set; }

        static TimerResolutionChanger()
        {
            var TimerResolutionInfo = TimerResolutionChanger.QueryTimerResolution();
            maxTimerResolution = TimerResolutionInfo.Max;
            currentTimerResolution = TimerResolutionInfo.Current;
        }

        private static TimerResolutionInfo QueryTimerResolution()
        {
            var info = new TimerResolutionInfo();
            NtQueryTimerResolution(out info.Min, out info.Max, out info.Current);
            return info;
        }

        public static void SetTimerResolutionToMax()
        {
            // https://programtalk.com/vs4/csharp/ACEmulator/ACE/Source/ACE.Server/Program.cs/
            try
            {
                if (isTimerResolutionSetToMax || (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)))
                {
                    return;
                }
                NtSetTimerResolution(maxTimerResolution, true, ref currentTimerResolution);
                isTimerResolutionSetToMax = true;
            }
            catch { }
        }

        public static void ResetTimerResolution()
        {
            try
            {
                if ((!isTimerResolutionSetToMax) || (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)))
                {
                    return;
                }
                NtSetTimerResolution(maxTimerResolution, false, ref currentTimerResolution);
                isTimerResolutionSetToMax = false;
            }
            catch { }
        }
    }
}