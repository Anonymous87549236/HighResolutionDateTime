using System;

namespace HighResolutionDateTime
{
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    public enum StopwatchDateTimeDisposalState
    {
        NotDisposable = 0,
        NotDisposed = 1,
        Disposed = 2,
    }
}
