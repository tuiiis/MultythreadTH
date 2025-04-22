﻿using Bogus;
using Multithreading.Models;


namespace Multithreading.Classes;
public static class ClassFaker
{
    public static Faker<Manufacturer> ManufacturerFaker => new Faker<Manufacturer>()
        .CustomInstantiator(f => new Manufacturer(
            f.Company.CompanyName(),
            f.Address.FullAddress(),
            f.Random.Bool()
        ));

    public static Faker<Tank> TankFaker => new Faker<Tank>()
        .CustomInstantiator(f => new Tank(
            f.IndexFaker,
            f.Commerce.ProductName(),
            f.Random.AlphaNumeric(10).ToUpper(),
            f.PickRandom<TankType>()
        ));

    public static (List<Tank>, List<Manufacturer>) CreateInstances()
    {
        var tanks = ClassFaker.TankFaker.Generate(10);
        var manufacturers = ClassFaker.ManufacturerFaker.Generate(10);

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


