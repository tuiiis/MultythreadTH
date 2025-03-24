namespace reflection;

public class Tank
{
    private int ID { get; set; }
    public string Model { get; set; }
    public string SerialNumber { get; set; }
    public TankType? TankType { get; set; } // light, medium, heavy???

    public Tank(int iD, string model, string serialNumber, string tankType)
    {
        ID = iD;
        Model = model;
        SerialNumber = serialNumber;

        if (Enum.TryParse(tankType, true, out TankType parsedType))
        {
            TankType = parsedType;
        }
        else
        {
            throw new ArgumentException("Invalid tank type. Choose: Light, Medium, Heavy.");
        }
    }
}

public enum TankType
{
    Light,
    Medium,
    Heavy
}


