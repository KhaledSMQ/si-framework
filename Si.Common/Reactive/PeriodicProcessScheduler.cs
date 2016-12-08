using System;
using System.ComponentModel.Composition;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;

namespace Si.Common.Reactive
{
    public interface IPeriodicProcessScheduler
    {
        void ScheduleProcessAsync(EventHandler<EventArgs> handler, CancellationToken cancellationToken, TimeSpan interval);
        void ScheduleProcessAsync(EventHandler<EventArgs> handler, CancellationToken cancellationToken, TimeSpan interval, IScheduler scheduler);
        void ScheduleProcessAsync(EventHandler<EventArgs> handler, CancellationToken cancellationToken, TimeSpan interval, TimeSpan firstRun);
        void ScheduleProcessAsync(EventHandler<EventArgs> handler, CancellationToken cancellationToken, TimeSpan interval, TimeSpan firstRun, IScheduler scheduler);
    }

    [Export(typeof(IPeriodicProcessScheduler))]
    public class PeriodicProcessScheduler : IPeriodicProcessScheduler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PeriodicProcessScheduler));
        
        public void ScheduleProcessAsync
        (
            EventHandler<EventArgs> handler,
            CancellationToken cancellationToken,
            TimeSpan interval
        )
        {
            ScheduleProcessAsync(handler, cancellationToken, interval, DefaultScheduler.Instance);
        }

        public void ScheduleProcessAsync
        (
            EventHandler<EventArgs> handler,
            CancellationToken cancellationToken,
            TimeSpan interval,
            IScheduler scheduler
        )
        {
            ScheduleProcessAsync(handler, cancellationToken, interval, interval, DefaultScheduler.Instance);
        }

        public void ScheduleProcessAsync
        (
            EventHandler<EventArgs> handler,
            CancellationToken cancellationToken,
            TimeSpan interval,
            TimeSpan firstRun
        )
        {
            ScheduleProcessAsync(handler, cancellationToken, interval, firstRun, DefaultScheduler.Instance);
        }

        public async void ScheduleProcessAsync
        (
            EventHandler<EventArgs> handler,
            CancellationToken cancellationToken,
            TimeSpan interval,
            TimeSpan firstRun,
            IScheduler scheduler
        )
        {
            try
            {
                TimeSpan nextRun = firstRun;

                while (!cancellationToken.IsCancellationRequested)
                {
                    // Await the next scheduled run.
                    await Observable.Timer(nextRun, scheduler);

                    nextRun = interval;

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        // Run process on new thread. We don't care about the result, and we don't want our scheduling to be delayed.
                        Task.Run(() => handler(this, EventArgs.Empty));
                    }
                }
            }
            catch (Exception e)
            {
                Log.Warn("PeriodicProcessScheduler experienced an unexpected exception.", e);
            }
        }

    }
}
