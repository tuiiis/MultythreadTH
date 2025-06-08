using ADO.Net.Models;
using Bogus;

namespace ADO.Net;

/// <summary>
/// A utility class for generating fake data for the project.
/// </summary>
public class ClassFaker
{
    /// <summary>
    /// Creates a specified number of fake manufacturer objects.
    /// </summary>
    /// <param name="count">The number of manufacturer objects to create.</param>
    /// <returns>A list of fake manufacturer objects.</returns>
    public static List<Manufacturer> CreateManufacturers(int count = 10)
    {
        var manufacturerFaker = new Faker<Manufacturer>()
            .CustomInstantiator(f => new Manufacturer(
                f.Company.CompanyName(),
                f.Address.FullAddress(),
                f.Random.Bool()
            ));

        return manufacturerFaker.Generate(count);
    }

    // /// <summary>
    // /// Creates a specified number of fake tank objects for given manufacturers.
    // /// </summary>
    // /// <param name="manufacturers">List of manufacturers to associate tanks with.</param>
    // /// <param name="count">The number of tank objects to create.</param>
    // /// <returns>A list of fake tank objects.</returns>
    // public static List<Tank> CreateTanks(List<Manufacturer> manufacturers, int count = 30)
    // {
    //     var tankFaker = new Faker<Tank>()
    //         .CustomInstantiator(f => new Tank(
    //             f.Commerce.ProductName(),
    //             f.Random.AlphaNumeric(10).ToUpper(),
    //             f.PickRandom<TankType>(),
    //             f.PickRandom(manufacturers).Id
    //         ));

    //     return tankFaker.Generate(count);
    // }

    public static Tank CreateTank(Manufacturer manufacturer)
    {
        var tankFaker = new Faker<Tank>()
            .CustomInstantiator(f => new Tank(
                f.Commerce.ProductName(),
                f.Random.AlphaNumeric(10).ToUpper(),
                f.PickRandom<TankType>(),
                manufacturer.Id
            ));

        return tankFaker.Generate(1)[0];
    }
}
