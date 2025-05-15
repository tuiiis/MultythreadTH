using Bogus;
using TPLProject.Models;

/// <summary>
/// Provides fake data generation for Manufacturer and Tank classes.
/// </summary>
namespace TPL.Classes;

public static class ClassFaker
{
    /// <summary>
    /// Gets a Faker instance for generating Manufacturer objects.
    /// </summary>
    public static Faker<Manufacturer> ManufacturerFaker => new Faker<Manufacturer>()
        .CustomInstantiator(f => new Manufacturer(
            f.Company.CompanyName(),
            f.Address.FullAddress(),
            f.Random.Bool()
        ));

    /// <summary>
    /// Gets a Faker instance for generating Tank objects.
    /// </summary>
    public static Faker<Tank> TankFaker => new Faker<Tank>()
        .CustomInstantiator(f => new Tank(
            f.IndexFaker,
            f.Commerce.ProductName(),
            f.Random.AlphaNumeric(10).ToUpper(),
            f.PickRandom<TankType>()
        ));

    /// <summary>
    /// Creates instances of Tank and Manufacturer, and prints them to the console.
    /// </summary>
    /// <returns>A tuple containing a list of generated Tank objects and a list of generated Manufacturer objects.</returns>
    public static (List<Tank>, List<Manufacturer>) CreateInstances()
    {
        var tanks = TankFaker.Generate(10);
        var manufacturers = ManufacturerFaker.Generate(10);

        Console.WriteLine("10 tanks and 10 manufacturers created:");

        Console.WriteLine("Tanks:");
        foreach (var tank in tanks)
        {
            Console.WriteLine($"{nameof(tank.ID)}: {tank.ID}, {nameof(tank.Model)}: {tank.Model}, {nameof(tank.SerialNumber)}: {tank.SerialNumber}, {nameof(tank.TankType)}: {tank.TankType}");
        }

        Console.WriteLine("\nManufacturers:");
        foreach (var m in manufacturers)
        {
            Console.WriteLine($"{nameof(m.Name)}: {m.Name}, {nameof(m.Address)}: {m.Address}, {nameof(m.IsAChildCompany)}: {m.IsAChildCompany}");
        }

        return (tanks, manufacturers);
    }
}
