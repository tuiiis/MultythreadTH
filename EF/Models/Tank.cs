using System.ComponentModel.DataAnnotations;

namespace EF.Models
{
    /// <summary>
    /// Represents a tank entity.
    /// </summary>
    public class Tank
    {
        /// <summary>
        /// Gets or sets the unique identifier for the tank.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the model name of the tank.
        /// </summary>
        [Required]
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the serial number of the tank.
        /// </summary>
        [Required]
        public string SerialNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the tank.
        /// </summary>
        public TankType TankType { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer ID for the tank.
        /// </summary>
        public Guid ManufacturerId { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer associated with the tank.
        /// </summary>
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