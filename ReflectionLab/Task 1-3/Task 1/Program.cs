using System;

Console.WriteLine("Select the task to run:");
Console.WriteLine("1. Task 1 (Create instance and invoke selected method)");
Console.WriteLine("2. Task 2/3 (Load DLL, call PrintObject)");

string? choice = Console.ReadLine();

switch (choice)
{
    case "1":
        Task1.Run();
        break;
    case "2":
        Task2_3.Run();
        break;
    default:
        Console.WriteLine("Invalid choice.");
        break;
}
