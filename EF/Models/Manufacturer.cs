using System.ComponentModel.DataAnnotations;

namespace ADO.Net.Models
{
    public class Manufacturer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public bool IsAChildCompany { get; set; }
        public virtual ICollection<Tank> Tanks { get; set; } = new List<Tank>();

        public Manufacturer() { }

        public Manufacturer(string name, string address, bool isAChildCompany)
        {
            Id = Guid.NewGuid();
            Name = name;
            Address = address;
            IsAChildCompany = isAChildCompany;
        }
    }
} 