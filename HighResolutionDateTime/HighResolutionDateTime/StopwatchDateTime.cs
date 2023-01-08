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
        // Time after which it is necessary to synchronize _startTimestamp with System.DateTime to avoid drifts
        internal long _maxIdleTime = System.TimeSpan.FromSeconds(10).Ticks;

        private readonly ThreadLocal<System.DateTime> _startTime;

        private readonly ThreadLocal<double> _startTimestamp;

        // https://referencesource.microsoft.com/#system/services/monitoring/system/diagnosticts/Stopwatch.cs
        // performance-counter frequency, in counts per ticks.
        // This can speed up conversion from high frequency performance-counter 
        // to ticks.
        private static readonly double tickFrequency = System.TimeSpan.TicksPerSecond / Stopwatch.Frequency;

        private readonly ThreadLocal<System.DateTime> previousTime;

        /// <summary>
        /// Creates an instance of the <see cref="StopwatchDateTime"/>.
        /// </summary>
        public StopwatchDateTime()
        {
            /* To minimize forward time jumps. Example
            System.DateTime.UtcNow update period = 15,625ms
            System.DateTime.UtcNow updated and became equal to 1
            _startTime updated after 15ms
            Shortly before _maxIdleTime StopwatchDateTime.UtcNow was called and returned almost 11
            System.DateTime.UtcNow updated after 625us
            Right after that StopwatchDateTime.UtcNow was called and returned 11,015625s
            So a 15ms delay in reading System.DateTime.UtcNow caused the return value to change by 15.625ms in just 625us */
            if (DateTime.isAccurateButSlow)
            {
                DateTime.WaitForIncrease();
            }
            _startTime = new ThreadLocal<System.DateTime>(() => System.DateTime.UtcNow, false);
            _startTimestamp = new ThreadLocal<double>(() => Stopwatch.GetTimestamp(), false);
            previousTime = new ThreadLocal<System.DateTime>(() => System.DateTime.MinValue, false);
        }
        public System.DateTime UtcNow
        {
            get
            {
                double endTimestamp = Stopwatch.GetTimestamp();

                // convert high resolution perf counter to DateTime ticks
                var durationInTicks = (endTimestamp - _startTimestamp.Value) * tickFrequency;
                if (durationInTicks >= _maxIdleTime)
                {
                    if (DateTime.isAccurateButSlow)
                    {
                        DateTime.WaitForIncrease();
                    }
                    _startTime.Value = System.DateTime.UtcNow;

                    // fixes https://github.com/Anonymous87549236/HighResolutionDateTime/issues/1
                    while (previousTime.Value > _startTime.Value)
                    {
                        DateTime.WaitForIncrease();
                        _startTime.Value = System.DateTime.UtcNow;
                    }
                    _startTimestamp.Value = Stopwatch.GetTimestamp();
                    previousTime.Value = _startTime.Value;

                    return _startTime.Value;
                }

                previousTime.Value = _startTime.Value.AddTicks(unchecked((long)durationInTicks));
                return previousTime.Value;
            }
        }

        /// <summary>
        /// Releases all resources used by the instance of <see cref="StopwatchDateTime"/>.
        /// </summary>
        public void Dispose()
        {
            _startTime.Dispose();
            _startTimestamp.Dispose();
            previousTime.Dispose();
        }
    }
}