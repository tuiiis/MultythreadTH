using Asynchrony.Models;
using Bogus;

namespace Asynchrony.Classes;

public class ClassFaker
{
    public static List<Tank> CreateTanks()
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

        var tanks = tankFaker.Generate(50);

        return tanks;
    }
}
