using ADO.Net.Models;
using Bogus;

namespace ADO.Net;

/// <summary>
/// A utility class for generating fake data for testing and development purposes.
/// </summary>
public class ClassFaker
{
    /// <summary>
    /// Creates a specified number of fake manufacturer objects with random data.
    /// </summary>
    /// <param name="count">The number of manufacturer objects to create. Default is 10.</param>
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

    /// <summary>
    /// Creates a fake tank object associated with a specific manufacturer.
    /// </summary>
    /// <param name="manufacturer">The manufacturer to associate the tank with.</param>
    /// <returns>A fake tank object with random data.</returns>
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
