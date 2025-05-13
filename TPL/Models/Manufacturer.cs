namespace TPLProject.Models;

public class Manufacturer
{
    public string Name { get; set; }
    public string Address { get; set; }
    public bool IsAChildCompany { get; set; }

    public Manufacturer(string name, string address, bool isAChildCompany)
    {
        Name = name;
        Address = address;
        IsAChildCompany = isAChildCompany;
    }
}
