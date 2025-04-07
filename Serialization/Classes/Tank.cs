namespace Serialization.Classes;

public class Tank
{
    public int ID { get; set; }
    public string Model { get; set; }
    public string SerialNumber { get; set; }
    public TankType? TankType { get; set; }

    public Tank(int id, string model, string serialNumber, TankType tankType)
    {
        ID = id;
        Model = model;
        SerialNumber = serialNumber;
        TankType = tankType;
    }
}


