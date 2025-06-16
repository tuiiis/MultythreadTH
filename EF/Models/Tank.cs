using System.ComponentModel.DataAnnotations;

namespace ADO.Net.Models
{
    public class Tank
    {
        public Guid Id { get; set; }
        [Required]
        public string Model { get; set; } = string.Empty;
        [Required]
        public string SerialNumber { get; set; } = string.Empty;
        public TankType TankType { get; set; }
        public Guid ManufacturerId { get; set; }
        public virtual Manufacturer? Manufacturer { get; set; }

        public Tank() { }

        public Tank(string model, string serialNumber, TankType tankType, Guid manufacturerId)
        {
            Id = Guid.NewGuid();
            Model = model;
            SerialNumber = serialNumber;
            TankType = tankType;
            ManufacturerId = manufacturerId;
        }
    }
} 