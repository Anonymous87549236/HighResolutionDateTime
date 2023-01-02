// https://www.nimaara.com/high-resolution-clock-in-net/

namespace HighResolutionDateTime
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// This class returns time by using a manually tuned and compensated <c>DateTime</c> which takes
    /// advantage of the high resolution available in <see cref = "Stopwatch" />.
    /// </summary>
    internal class StopwatchDateTime : IDisposable
    {
        private readonly long _maxIdleTime = TimeSpan.FromSeconds(10).Ticks;
        private const long TicksMultiplier = 1000 * TimeSpan.TicksPerMillisecond;

        private readonly ThreadLocal<double> _startTimestamp =
            new ThreadLocal<double>(() => Stopwatch.GetTimestamp(), false);

        private readonly ThreadLocal<System.DateTime> _startTime =
            new ThreadLocal<System.DateTime>(() => System.DateTime.UtcNow, false);

        /// <summary>
        /// Creates an instance of the <see cref="StopwatchDateTime"/>.
        /// </summary>
        public StopwatchDateTime()
        {

        }
        public System.DateTime UtcNow
        {
            get
            {
                double endTimestamp = Stopwatch.GetTimestamp();

                var durationInTicks = (endTimestamp - _startTimestamp.Value) / Stopwatch.Frequency * TicksMultiplier;
                if (durationInTicks >= _maxIdleTime)
                {
                    _startTimestamp.Value = Stopwatch.GetTimestamp();
                    _startTime.Value = System.DateTime.UtcNow;
                    return _startTime.Value;
                }

                return _startTime.Value.AddTicks((long)durationInTicks);
            }
        }

        /// <summary>
        /// Releases all resources used by the instance of <see cref="StopwatchDateTime"/>.
        /// </summary>
        public void Dispose()
        {
            _startTime.Dispose();
            _startTimestamp.Dispose();
        }
    }
}