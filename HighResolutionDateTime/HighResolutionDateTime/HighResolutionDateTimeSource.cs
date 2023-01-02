using System;

namespace HighResolutionDateTime
{
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    public enum HighResolutionDateTimeSource
    {
        None = 0,
        Stopwatch = 1,
        // This only available on <c>Windows 8</c>/<c>Windows Server 2012</c> and higher.
        GetSystemTimePreciseAsFileTime = 2,
    }
}