namespace Asynchrony.Models;

/// <summary>
/// Represents a tank with properties such as ID, model, serial number, type, and manufacturer.
/// </summary>
public class Tank
{
    /// <summary>
    /// Gets or sets the unique identifier for the tank.
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// Gets or sets the model of the tank.
    /// </summary>
    public string Model { get; set; }

    /// <summary>
    /// Gets or sets the serial number of the tank.
    /// </summary>
    public string SerialNumber { get; set; }

    /// <summary>
    /// Gets or sets the type of the tank.
    /// </summary>
    public TankType TankType { get; set; }

    /// <summary>
    /// Gets or sets the manufacturer of the tank.
    /// </summary>
    public Manufacturer Manufacturer { get; set; }

    /// <summary>
    /// Initializes a new instance of the Tank class.
    /// </summary>
    public Tank() { }

    /// <summary>
    /// Initializes a new instance of the Tank class with specified properties.
    /// </summary>
    /// <param name="id">The unique identifier for the tank.</param>
    /// <param name="model">The model of the tank.</param>
    /// <param name="serialNumber">The serial number of the tank.</param>
    /// <param name="tankType">The type of the tank.</param>
    /// <param name="manufacturer">The manufacturer of the tank.</param>
    public Tank(int id, string model, string serialNumber, TankType tankType, Manufacturer manufacturer)
    {
        ID = id;
        Model = model;
        SerialNumber = serialNumber;
        TankType = tankType;
        Manufacturer = manufacturer;
    }
}
