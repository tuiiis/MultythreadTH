namespace reflection
{
    public class Tank
    {
        private int ID { get; set; }
        public string Model { get; set; }
        public string SerialNumber { get; set; }
        public TankType? TankType { get; set; }

        public static Tank Create(int id, string model, string serialNumber, TankType tankType)
        {
            return new Tank { ID = id, Model = model, SerialNumber = serialNumber, TankType = tankType };
        }

        public void PrintObject()
        {
            Console.WriteLine($"Tank ID: {ID}");
            Console.WriteLine($"Model: {Model}");
            Console.WriteLine($"Serial Number: {SerialNumber}");
            Console.WriteLine($"Tank Type: {TankType}");
        }
    }
}
