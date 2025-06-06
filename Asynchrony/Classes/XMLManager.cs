using System.Xml.Linq;
using Asynchrony.Models;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Bogus;
using System.Threading;
using System.Linq;

namespace Asynchrony.Classes
{
    /// <summary>
    /// A class for managing XML operations related to tank data.
    /// </summary>
    public class XMLManager
    {
        private static readonly Random _random = new();
        private static readonly Faker _faker = new();
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _sortingTask;
        private bool _isSortingEnabled;

        /// <summary>
        /// Creates a specified number of tank objects.
        /// </summary>
        /// <param name="count">The number of tank objects to create.</param>
        /// <returns>A list of tank objects.</returns>
        public static List<Tank> CreateTanks(int count)
        {
            var tanks = new List<Tank>();
            for (int i = 0; i < count; i++)
            {
                tanks.Add(new Tank
                {
                    ID = i + 1,
                    Model = $"{nameof(Tank.Model)}_{_random.Next(1, 100)}",
                    SerialNumber = $"SN{_random.Next(1000, 9999)}",
                    TankType = (TankType)_random.Next(0, 3),
                    Manufacturer = new Manufacturer(
                        $"{nameof(Manufacturer)}_{_random.Next(1, 10)}",
                        $"{nameof(Manufacturer.Address)}_{_random.Next(1, 100)}",
                        _faker.Random.Bool()
                    )
                });
            }
            return tanks;
        }

        /// <summary>
        /// Displays the details of a list of tanks.
        /// </summary>
        /// <param name="tanks">The list of tanks to display.</param>
        public static void DisplayTanks(List<Tank> tanks)
        {
            foreach (var tank in tanks)
            {
                Console.WriteLine($"{nameof(Tank)} {nameof(tank.ID)}: {tank.ID}, {nameof(tank.Model)}: {tank.Model}, {nameof(tank.SerialNumber)}: {tank.SerialNumber}, {nameof(tank.TankType)}: {tank.TankType}");
                Console.WriteLine($"{nameof(tank.Manufacturer)}: {tank.Manufacturer.Name}, {nameof(tank.Manufacturer.Address)}: {tank.Manufacturer.Address}, {nameof(tank.Manufacturer.IsAChildCompany)}: {tank.Manufacturer.IsAChildCompany}");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Outputs the contents of a dictionary containing tank data.
        /// </summary>
        /// <param name="dictionary">The dictionary to output.</param>
        public static void OutputDictionaryContents(ConcurrentDictionary<string, ConcurrentBag<Tank>> dictionary)
        {
            foreach (var kvp in dictionary)
            {
                Console.WriteLine($"{nameof(dictionary)}: {kvp.Key}");
                foreach (var tank in kvp.Value)
                {
                    Console.WriteLine($"{nameof(tank.ID)}: {tank.ID}, {nameof(tank.Model)}: {tank.Model}, {nameof(tank.SerialNumber)}: {tank.SerialNumber}, {nameof(tank.TankType)}: {tank.TankType}, {nameof(tank.Manufacturer)}: {tank.Manufacturer}");
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Saves a list of tanks into XML files, divided into groups.
        /// </summary>
        /// <param name="tanks">The list of tanks to save.</param>
        /// <param name="numberOfGroups">The number of groups to divide the tanks into.</param>
        public static void SaveGroupsToXML(List<Tank> tanks, int numberOfGroups)
        {
            var groups = SplitIntoGroups(tanks, numberOfGroups);
            for (int i = 0; i < groups.Count; i++)
            {
                SaveToXML(string.Format(FileConstants.TankFileFormat, i + 1), groups[i]);
            }
        }

        /// <summary>
        /// Splits a list of tanks into a specified number of groups.
        /// </summary>
        /// <param name="tanks">The list of tanks to split.</param>
        /// <param name="numberOfGroups">The number of groups to split the tanks into.</param>
        /// <returns>A list of lists, each containing a group of tanks.</returns>
        private static List<List<Tank>> SplitIntoGroups(List<Tank> tanks, int numberOfGroups)
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

        /// <summary>
        /// Saves a list of tanks to an XML file.
        /// </summary>
        /// <param name="filePath">The path of the XML file to save to.</param>
        /// <param name="tanks">The list of tanks to save.</param>
        public static void SaveToXML(string filePath, IEnumerable<Tank> tanks)
        {
            var xDoc = new XDocument(
                new XElement(nameof(Tank)+"s",
                    tanks.Select(t =>
                        new XElement(nameof(Tank),
                            new XElement(nameof(Tank.ID), t.ID),
                            new XElement(nameof(Tank.Model), t.Model),
                            new XElement(nameof(Tank.SerialNumber), t.SerialNumber),
                            new XElement(nameof(Tank.TankType), t.TankType),
                            new XElement(nameof(Tank.Manufacturer),
                                new XElement(nameof(Manufacturer.Name), t.Manufacturer.Name),
                                new XElement(nameof(Manufacturer.Address), t.Manufacturer.Address),
                                new XElement(nameof(Manufacturer.IsAChildCompany), t.Manufacturer.IsAChildCompany)
                            )
                        )
                    )
                )
            );

            xDoc.Save(filePath);
        }

        /// <summary>
        /// Reads tank data from an XML file.
        /// </summary>
        /// <param name="filePath">The path of the XML file to read from.</param>
        /// <param name="progress">Reports the progress of the operation.</param>
        /// <returns>A list of tank objects.</returns>
        public static async Task<List<Tank>> ReadFromXMLAsync(string filePath, IProgress<int> progress)
        {
            var tanks = new List<Tank>();
            var doc = XDocument.Load(filePath);
            var tankElements = doc.Descendants(nameof(Tank)).ToList();
            int totalTanks = tankElements.Count;
            int processedTanks = 0;

            foreach (var tankElement in tankElements)
            {
                var manufacturerElement = tankElement.Element(nameof(Tank.Manufacturer));
                var manufacturer = new Manufacturer(
                    manufacturerElement?.Element(nameof(Manufacturer.Name))?.Value ?? "Unknown",
                    manufacturerElement?.Element(nameof(Manufacturer.Address))?.Value ?? "Unknown",
                    bool.Parse(manufacturerElement?.Element(nameof(Manufacturer.IsAChildCompany))?.Value ?? "false")
                );

                var tank = new Tank
                {
                    ID = int.Parse(tankElement.Element(nameof(Tank.ID))?.Value ?? "0"),
                    Model = tankElement.Element(nameof(Tank.Model))?.Value ?? string.Empty,
                    SerialNumber = tankElement.Element(nameof(Tank.SerialNumber))?.Value ?? string.Empty,
                    TankType = (TankType)Enum.Parse(typeof(TankType), tankElement.Element(nameof(Tank.TankType))?.Value ?? nameof(TankType.Light)),
                    Manufacturer = manufacturer
                };

                tanks.Add(tank);
                processedTanks++;
                progress.Report((int)((double)processedTanks / totalTanks * 100));
                await Task.Delay(100); // Slow down for demo purposes
            }

            return tanks;
        }

        /// <summary>
        /// Processes XML files containing tank data and stores them in a dictionary.
        /// </summary>
        /// <param name="tanks">The list of tanks to process.</param>
        /// <returns>A concurrent dictionary containing tank data from XML files.</returns>
        /// <exception cref="InvalidOperationException">Thrown when tanks are not generated.</exception>
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
            OutputDictionaryContents(dictionary);
            
            return dictionary;
        }

        /// <summary>
        /// Reads tank data from XML files and stores them in a dictionary.
        /// </summary>
        /// <param name="progress">Reports the progress of the operation.</param>
        /// <returns>A concurrent dictionary containing tank data from XML files.</returns>
        /// <exception cref="FileNotFoundException">Thrown when no XML files are found.</exception>
        private static async Task<ConcurrentDictionary<string, ConcurrentBag<Tank>>> ReadXmlFilesAsync(IProgress<int> progress)
        {
            var xmlFiles = Directory.GetFiles(".", FileConstants.TankFilePattern);
            if (xmlFiles.Length == 0)
            {
                throw new FileNotFoundException($"No {nameof(FileConstants.TankFilePattern)} files found. Please save tanks to XML files first!");
            }

            var dictionary = new ConcurrentDictionary<string, ConcurrentBag<Tank>>();
            var tasks = new List<Task>();

            foreach (var file in xmlFiles)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var tanks = await ReadFromXMLAsync(file, progress);
                    dictionary.TryAdd(Path.GetFileName(file), new ConcurrentBag<Tank>(tanks));
                }));
            }

            await Task.WhenAll(tasks);
            return dictionary;
        }

        /// <summary>
        /// Merges tank data from a dictionary into a single XML file.
        /// </summary>
        /// <param name="dictionary">The dictionary containing tank data to merge.</param>
        /// <exception cref="InvalidOperationException">Thrown when no tanks are available to merge.</exception>
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

            SaveToXML(FileConstants.MergedTanksFile, allTanks);
            Console.WriteLine($"\n\nTanks have been merged into {nameof(FileConstants.MergedTanksFile)}");
            DisplayTanks(allTanks);
        }

        /// <summary>
        /// Starts the sorting process for a dictionary of tanks.
        /// </summary>
        /// <param name="dictionary">The dictionary to sort.</param>
        public void StartSorting(ConcurrentDictionary<string, ConcurrentBag<Tank>> dictionary)
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
                    foreach (var kvp in dictionary)
                    {
                        var sortedTanks = kvp.Value.OrderBy(t => t.ID).ToList();
                        dictionary[kvp.Key] = new ConcurrentBag<Tank>(sortedTanks);
                    }
                    await Task.Delay(5000, _cancellationTokenSource.Token);
                }
            }, _cancellationTokenSource.Token);
            _isSortingEnabled = true;
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
            _isSortingEnabled = false;
        }

        /// <summary>
        /// Gets a value indicating whether sorting is enabled.
        /// </summary>
        public bool IsSortingEnabled => _isSortingEnabled;
    }
}
