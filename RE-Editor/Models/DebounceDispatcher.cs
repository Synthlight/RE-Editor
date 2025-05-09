using System;
using System.Windows.Threading;

namespace RE_Editor.Models;

// https://stackoverflow.com/a/44876143/3137362
public class DebounceDispatcher {
    private DispatcherTimer timer;
    private DateTime        timerStarted { get; set; } = DateTime.UtcNow.AddYears(-1);

    public void Debounce(int                intervalMs,
                         Action             action,
                         DispatcherPriority priority   = DispatcherPriority.ApplicationIdle,
                         Dispatcher         dispatcher = null) {
        // kill pending timer and pending ticks
        timer?.Stop();
        timer = null;

        dispatcher ??= Dispatcher.CurrentDispatcher;

        // timer is recreated for each event and effectively
        // resets the timeout. Action only fires after timeout has fully
        // elapsed without other events firing in between
        timer = new(TimeSpan.FromMilliseconds(intervalMs), priority, (s, e) => {
            if (timer == null) return;

            timer?.Stop();
            timer = null;
            action.Invoke();
        }, dispatcher);

        timer.Start();
    }
}