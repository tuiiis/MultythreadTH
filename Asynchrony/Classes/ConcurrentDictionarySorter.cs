using Asynchrony.Models;
using System.Collections.Concurrent;

namespace Asynchrony.Classes
{
    /// <summary>
    /// A class that sorts items in a concurrent dictionary based on a specified key selector.
    /// </summary>
    /// <typeparam name="T">The type of items in the dictionary.</typeparam>
    public class ConcurrentDictionarySorter<T> where T : class
    {
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _sortingTask;
        private readonly ConcurrentDictionary<string, ConcurrentBag<T>> _dictionary;
        private readonly Func<T, object> _sortKeySelector;

        /// <summary>
        /// Initializes a new instance of the ConcurrentDictionarySorter class.
        /// </summary>
        /// <param name="dictionary">The dictionary to sort.</param>
        /// <param name="sortKeySelector">The function to select the key for sorting.</param>
        public ConcurrentDictionarySorter(ConcurrentDictionary<string, ConcurrentBag<T>> dictionary, Func<T, object> sortKeySelector)
        {
            _dictionary = dictionary;
            _sortKeySelector = sortKeySelector;
        }

        /// <summary>
        /// Starts the sorting process.
        /// </summary>
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
                        var sortedItems = kvp.Value.OrderBy(_sortKeySelector).ToList();
                        _dictionary[kvp.Key] = new ConcurrentBag<T>(sortedItems);
                    }
                    await Task.Delay(5000, _cancellationTokenSource.Token);
                }
            }, _cancellationTokenSource.Token);
        }

        /// <summary>
        /// Stops the sorting process.
        /// </summary>
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