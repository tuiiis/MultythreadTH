using System.ComponentModel.DataAnnotations;

namespace EF.Models
{
    /// <summary>
    /// Represents a tank entity.
    /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="Tank"/> class.
        /// </summary>
        public Tank() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tank"/> class with specified values.
        /// </summary>
        /// <param name="model">The model name.</param>
        /// <param name="serialNumber">The serial number.</param>
        /// <param name="tankType">The tank type.</param>
        /// <param name="manufacturerId">The manufacturer ID.</param>
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