namespace Asynchrony.Models;

public class Tank
{
    public int ID { get; set; }
    public string Model { get; set; }
    public string SerialNumber { get; set; }
    public TankType TankType { get; set; }
    public Manufacturer Manufacturer { get; set; }

    public Tank() { }

    public Tank(int id, string model, string serialNumber, TankType tankType, Manufacturer manufacturer)
    {
        ID = id;
        Model = model;
        SerialNumber = serialNumber;
        TankType = tankType;
        Manufacturer = manufacturer;
    }
}
