using Asynchrony.Classes;

namespace Asynchrony;

class Program
{
    static void Main(string[] args)
    {
        while (true)
        {
            Console.WriteLine("\nChoose an option:");
            Console.WriteLine("1. Generate 50 random Tank objects");
            Console.WriteLine("2. Option 2");
            Console.WriteLine("3. Option 3");
            Console.WriteLine("Q. Quit");

            Console.Write("Enter your choice: ");
            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    var tanks = ClassFaker.CreateTanks();
                    DisplayHelper.DisplayTanks(tanks);
                    break;
                case "2":
                    // Implement Option 2 logic here
                    break;
                case "3":
                    // Implement Option 3 logic here
                    break;
                case "Q":
                case "q":
                    return;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }
}
