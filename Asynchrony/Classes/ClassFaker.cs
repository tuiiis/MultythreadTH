using Asynchrony.Models;
using Bogus;

namespace Asynchrony.Classes;

/// <summary>
/// A utility class for generating fake tank data.
/// </summary>
public class ClassFaker
{
    /// <summary>
    /// Creates a specified number of fake tank objects.
    /// </summary>
    /// <param name="count">The number of tank objects to create.</param>
    /// <returns>A list of fake tank objects.</returns>
    public static List<Tank> CreateTanks(int count = 50)
    {
        var manufacturerFaker = new Faker<Manufacturer>()
        .CustomInstantiator(f => new Manufacturer(
            f.Company.CompanyName(),
            f.Address.FullAddress(),
            f.Random.Bool()
        ));

        var manufacturers = manufacturerFaker.Generate(10);

        var tankFaker = new Faker<Tank>()
        .CustomInstantiator(f => new Tank(
            f.IndexFaker,
            f.Commerce.ProductName(),
            f.Random.AlphaNumeric(10).ToUpper(),
            f.PickRandom<TankType>(),
            f.PickRandom(manufacturers)
        ));

        var tanks = tankFaker.Generate(count);

        return tanks;
    }
}
