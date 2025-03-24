namespace reflection
{
    public class Tank
    {
        private int ID { get; set; }
        public string Model { get; set; }
        public string SerialNumber { get; set; }
        public TankType? TankType { get; set; } // light, medium, heavy

        public static Tank Create(int id, string model, string serialNumber, string tankType)
        {
            if (Enum.TryParse(tankType, true, out TankType parsedType))
            {
                return new Tank { ID = id, Model = model, SerialNumber = serialNumber, TankType = parsedType };
            }
            else
            {
                throw new ArgumentException("Invalid tank type. Choose: Light, Medium, Heavy.");
            }
        }

        public void PrintObject()
        {
            Console.WriteLine($"Tank ID: {ID}");
            Console.WriteLine($"Model: {Model}");
            Console.WriteLine($"Serial Number: {SerialNumber}");
            Console.WriteLine($"Tank Type: {TankType}");
        }
    }

    public enum TankType
    {
        Light,
        Medium,
        Heavy
    }
}
