using Bogus;

namespace Serialization.Classes
{
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
    }
}
