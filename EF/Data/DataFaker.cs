using Bogus;
using ADO.Net.Models;

namespace EF.Data
{
    /// <summary>
    /// A utility class for generating fake data for testing and development purposes.
    /// </summary>
    public static class DataFaker
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
        /// Creates a specified number of fake tank objects associated with provided manufacturers.
        /// </summary>
        /// <param name="manufacturers">The list of manufacturers to associate tanks with.</param>
        /// <param name="count">The number of tank objects to create. Default is 10.</param>
        /// <returns>A list of fake tank objects with random data.</returns>
        public static List<Tank> CreateTanks(List<Manufacturer> manufacturers, int count = 10)
        {
            var tankFaker = new Faker<Tank>()
                .CustomInstantiator(f => new Tank(
                    f.Commerce.ProductName(),
                    f.Random.AlphaNumeric(10).ToUpper(),
                    f.PickRandom<TankType>(),
                    f.PickRandom(manufacturers).Id
                ));

            return tankFaker.Generate(count);
        }
    }
} 