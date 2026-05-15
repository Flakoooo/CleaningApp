using Timer = System.Timers.Timer;

namespace CleaningAppWeb.Components.Services
{
    public class DebounceHelper(int delayMs, Func<Task> action) : IDisposable
    {
        private Timer? _timer;

        public void Trigger()
        {
            _timer?.Stop();
            _timer?.Dispose();

            _timer = new Timer(delayMs);
            _timer.Elapsed += async (_, _) => await action();
            _timer.AutoReset = false;
            _timer.Start();
        }

        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
        }
    }
}
