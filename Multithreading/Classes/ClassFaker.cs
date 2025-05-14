﻿using Bogus;
using Multithreading.Models;


namespace Multithreading.Classes;

/// <summary>
/// Provides methods for generating fake data for Manufacturer and Tank classes.
/// </summary>
public static class ClassFaker
{
    /// <summary>
    /// Gets a Faker instance for generating Manufacturer objects.
    /// </summary>
    /// <returns>A Faker instance configured for Manufacturer.</returns>
    public static Faker<Manufacturer> ManufacturerFaker => new Faker<Manufacturer>()
        .CustomInstantiator(f => new Manufacturer(
            f.Company.CompanyName(),
            f.Address.FullAddress(),
            f.Random.Bool()
        ));

    /// <summary>
    /// Gets a Faker instance for generating Tank objects.
    /// </summary>
    /// <returns>A Faker instance configured for Tank.</returns>
    public static Faker<Tank> TankFaker => new Faker<Tank>()
        .CustomInstantiator(f => new Tank(
            f.IndexFaker,
            f.Commerce.ProductName(),
            f.Random.AlphaNumeric(10).ToUpper(),
            f.PickRandom<TankType>()
        ));

    /// <summary>
    /// Generates lists of Tank and Manufacturer objects and prints them to the console.
    /// </summary>
    /// <returns>A tuple containing lists of generated Tank and Manufacturer objects.</returns>
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
