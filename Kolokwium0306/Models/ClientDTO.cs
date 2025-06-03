namespace Kolokwium0306.Models;

public class ClientDTO
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null;
    public string LastName { get; set; } = null;
    public string Address { get; set; } = null;
    public List<RentalDTO> Rentals { get; set; } = new();
}

public class RentalDTO
{
    public string Vin { get; set; } = null;
    public string Color { get; set; } = null;
    public string Model { get; set; } = null;
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int TotalPrice { get; set; }
}



public class CreateClientRentalDTO
{
    public ClientInnerDTO Client { get; set; } = null;
    public int CarId { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
}

public class ClientInnerDTO
{
    public string FirstName { get; set; } = null;
    public string LastName { get; set; } = null;
    public string Address { get; set; } = null;
}
