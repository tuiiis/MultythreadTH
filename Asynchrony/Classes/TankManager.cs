using Asynchrony.Models;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Asynchrony.Classes
{
    public static class TankManager
    {
        public static List<List<Tank>> SplitIntoGroups(List<Tank> tanks, int numberOfGroups)
        {
            var groups = new List<List<Tank>>();
            int tanksPerGroup = tanks.Count / numberOfGroups;
            for (int i = 0; i < numberOfGroups; i++)
            {
                int start = i * tanksPerGroup;
                int count = (i == numberOfGroups - 1) ? tanks.Count - start : tanksPerGroup;
                groups.Add(tanks.Skip(start).Take(count).ToList());
            }
            return groups;
        }

        public static async Task<ConcurrentDictionary<string, ConcurrentBag<Tank>>> ProcessXmlFilesAsync(List<Tank> tanks)
        {
            if (tanks == null)
            {
                throw new InvalidOperationException("Please generate tanks first!");
            }

            Console.WriteLine("\nReading XML files...");
            var progress = new Progress<int>(p => Console.Write($"\rProgress: {p}% "));
            
            var dictionary = await ReadXmlFilesAsync(progress);
            Console.WriteLine("\n\nXML files have been read and stored in the dictionary.");
            DisplayHelper.OutputDictionaryContents(dictionary);
            
            return dictionary;
        }

        private static async Task<ConcurrentDictionary<string, ConcurrentBag<Tank>>> ReadXmlFilesAsync(IProgress<int> progress)
        {
            var xmlFiles = Directory.GetFiles(".", "tanks_*.xml");
            if (xmlFiles.Length == 0)
            {
                throw new FileNotFoundException("No XML files found. Please save tanks to XML files first!");
            }

            var dictionary = new ConcurrentDictionary<string, ConcurrentBag<Tank>>();
            var tasks = new List<Task>();

            foreach (var file in xmlFiles)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var tanks = await XMLFileManager.ReadFromXMLAsync(file, progress);
                    dictionary.TryAdd(Path.GetFileName(file), new ConcurrentBag<Tank>(tanks));
                }));
            }

            await Task.WhenAll(tasks);
            return dictionary;
        }

        public static async Task MergeTanksToFileAsync(ConcurrentDictionary<string, ConcurrentBag<Tank>> dictionary)
        {
            if (dictionary == null || dictionary.IsEmpty)
            {
                throw new InvalidOperationException("No tanks to merge. Please read XML files first!");
            }

            Console.WriteLine("\nMerging tanks into a single file...");
            var progress = new Progress<int>(p => Console.Write($"\rProgress: {p}% "));

            var allTanks = new List<Tank>();
            int totalGroups = dictionary.Count;
            int processedGroups = 0;

            foreach (var group in dictionary.Values)
            {
                allTanks.AddRange(group);
                processedGroups++;
                ((IProgress<int>)progress).Report((int)((double)processedGroups / totalGroups * 100));
                await Task.Delay(100); // Slow down for demo purposes
            }

            XMLFileManager.SaveToXML("merged_tanks.xml", allTanks);
            Console.WriteLine("\n\nTanks have been merged into merged_tanks.xml");
            DisplayHelper.DisplayTanks(allTanks);
        }
    }
} 