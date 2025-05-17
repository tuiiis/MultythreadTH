namespace Asynchrony.Models;

public class Tank(int id, string model, string serialNumber, TankType tankType, Manufacturer manufacturer)
{
    public int ID { get; set; } = id;
    public string Model { get; set; } = model;
    public string SerialNumber { get; set; } = serialNumber;
    public TankType TankType { get; set; } = tankType;
    public Manufacturer Manufacturer { get; set; } = manufacturer;
}
