namespace Asynchrony.Models;

public class Manufacturer(string name, string address, bool isAChildCompany)
{
    public string Name { get; set; } = name;
    public string Address { get; set; } = address;
    public bool IsAChildCompany { get; set; } = isAChildCompany;
}
