using Asynchrony.Models;
using System.Collections.Concurrent;

namespace Asynchrony.Classes
{
    public class TankSorter
    {
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _sortingTask;
        private readonly ConcurrentDictionary<string, ConcurrentBag<Tank>> _dictionary;

        public TankSorter(ConcurrentDictionary<string, ConcurrentBag<Tank>> dictionary)
        {
            _dictionary = dictionary;
        }

        public void StartSorting()
        {
            if (_sortingTask != null && !_sortingTask.IsCompleted)
            {
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _sortingTask = Task.Run(async () =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    foreach (var kvp in _dictionary)
                    {
                        var sortedTanks = kvp.Value.OrderBy(t => t.ID).ToList();
                        _dictionary[kvp.Key] = new ConcurrentBag<Tank>(sortedTanks);
                    }
                    await Task.Delay(5000, _cancellationTokenSource.Token);
                }
            }, _cancellationTokenSource.Token);
        }

        public void StopSorting()
        {
            _cancellationTokenSource?.Cancel();
            _sortingTask?.Wait();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            _sortingTask = null;
        }
    }
} 