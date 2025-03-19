using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reflection
{
    public class Manufacturer
    {
        public string Name { get; set; }
        public string Address { get; set; }
        private bool IsAChildCompany { get; set; }

        public static Manufacturer Create(string name, string address, bool isAChildCompany)
        {
            return new Manufacturer { Name = name, Address = address, IsAChildCompany = isAChildCompany };
        }

        public void PrintObject()
        {
            Console.WriteLine($"Manufacturer Name: {Name}");
            Console.WriteLine($"Address: {Address}");
            Console.WriteLine($"Is a Child Company: {IsAChildCompany}");
        }
    }
}
