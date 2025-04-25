#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;

namespace RE_Editor.Util;

public class ThreadHandler {
    private readonly List<Thread> threads = [];
    private          int          doCount;
    private          int          doneCount;
    private          bool         anyWorking;
    private readonly Mutex        mutex = new();

    public delegate void OnUpdateHandler(int doCount, int doneCount, string? threadName);

    public event OnUpdateHandler? OnUpdate;

    public int DoCount {
        get {
            lock (mutex) {
                return doCount;
            }
        }
    }
    public int DoneCount {
        get {
            lock (mutex) {
                return doneCount;
            }
        }
    }

    /// <summary>
    /// If this thread is part of your estimated do count, skip the add!
    /// </summary>
    /// <param name="name">The thread name.</param>
    /// <param name="go">The action to run on a thread.</param>
    /// <param name="addToCount">Set to false if you accounted for this in the estimated thread count.</param>
    /// <returns></returns>
    public Thread AddWorker(string name, Action go, bool addToCount = true) {
        var thread = new Thread(() => {
            try {
                go();
            } finally {
                lock (mutex) {
                    doneCount++;
                    if (doCount == doneCount) {
                        anyWorking = false;
                    }
                }
                OnUpdate?.Invoke(DoCount, DoneCount, Thread.CurrentThread.Name);
            }
        }) {
            Name = name
        };
        lock (mutex) {
            threads.Add(thread);
            if (addToCount) doCount++;
            anyWorking = true;
            thread.Start();
        }
        OnUpdate?.Invoke(DoCount, DoneCount, Thread.CurrentThread.Name);
        return thread;
    }

    public void WaitAll() {
        List<Thread> threadsCopy;
        do {
            lock (mutex) {
                threadsCopy = [..threads];
            }
            foreach (var thread in threadsCopy) {
                thread.Join();
                lock (mutex) {
                    threads.Remove(thread);
                }
            }
        } while (threadsCopy.Count > 0);
        lock (mutex) {
            var threadCount = threads.Count;
            if (threadCount != 0) {
                throw new($"Something went horribly wrong, thread count ({threadCount}) != 0.");
            }
            if (doCount != doneCount) {
                throw new($"Something went horribly wrong, doCount ({doCount}) != doneCount ({doneCount}).");
            }
        }
    }

    public void Reset() {
        lock (mutex) {
            if (anyWorking) {
                throw new("Unable to reset thread handler when there's active threads.");
            }
            doCount   = 0;
            doneCount = 0;
        }
        OnUpdate?.Invoke(0, 0, Thread.CurrentThread.Name);
    }

    /// <summary>
    /// Adds the given number to the `doCount`.
    /// Be careful with this! When you add a worker thread, make sure to skip it from the count if it's one of the ones here.
    /// If it gets out of sync, bad things will happen!
    /// </summary>
    /// <param name="estimatedThreadCount"></param>
    public void AddEstimatedDoCount(int estimatedThreadCount) {
        lock (mutex) {
            doCount += estimatedThreadCount;
        }
        OnUpdate?.Invoke(DoCount, DoneCount, Thread.CurrentThread.Name);
    }
}