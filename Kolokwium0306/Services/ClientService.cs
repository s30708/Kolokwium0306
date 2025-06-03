using System.Net;
using Kolokwium0306.Models;
using Microsoft.Data.SqlClient;

namespace Kolokwium0306.Services;

public class ClientService : IClientService
{
    private readonly IConfiguration _configuration;

    public ClientService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<ClientDTO?> GetClient(int clientId)
{
    ClientDTO? clientDTO = null;

    string query = "SELECT ID, FirstName, LastName, Address FROM clients WHERE ID = @id";
    using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("Default")))
    using (SqlCommand cmd = new SqlCommand(query, conn))
    {
        await conn.OpenAsync();
        cmd.Parameters.AddWithValue("@id", clientId);

        using (var reader = await cmd.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                clientDTO = new ClientDTO
                {
                    Id = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Address = reader.GetString(3),
                    Rentals = new List<RentalDTO>()
                };
            }
            else return null;
        }
    }

    query = @"
        SELECT 
            c.VIN, 
            clr.Name AS Color, 
            m.Name AS Model, 
            r.DateFrom, 
            r.DateTo, 
            r.TotalPrice
        FROM car_rentals r
        JOIN cars c ON r.CarID = c.ID
        JOIN models m ON c.ModelID = m.ID
        JOIN colors clr ON c.ColorID = clr.ID
        WHERE r.ClientID = @id";

    using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("Default")))
    using (SqlCommand cmd = new SqlCommand(query, conn))
    {
        await conn.OpenAsync();
        cmd.Parameters.AddWithValue("@id", clientId);

        using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                clientDTO.Rentals.Add(new RentalDTO
                {
                    Vin = reader.GetString(0),
                    Color = reader.GetString(1),
                    Model = reader.GetString(2),
                    DateFrom = reader.GetDateTime(3),
                    DateTo = reader.GetDateTime(4),
                    TotalPrice = reader.GetInt32(5)
                });
            }
        }
    }

    return clientDTO;
}

    public async Task<bool> ClientExists(int clientId)
    {
        string command = "SELECT 1 FROM clients WHERE ID = @Id";
        
        using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("Default")))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@Id", clientId);

            await conn.OpenAsync();
            var client = await cmd.ExecuteScalarAsync();
            if (client is null)
                return false;
            return true;
        }
    }

    
    
    public async Task<bool> CreateClientWithRental(CreateClientRentalDTO dto)
{
    using var conn = new SqlConnection(_configuration.GetConnectionString("Default"));
    await conn.OpenAsync();
    using var transaction = conn.BeginTransaction();

    try
    {
        var getCarCmd = new SqlCommand("SELECT PricePerDay FROM cars WHERE ID = @carId", conn, transaction);
        getCarCmd.Parameters.AddWithValue("@carId", dto.CarId);
        var priceObj = await getCarCmd.ExecuteScalarAsync();

        if (priceObj == null)
            return false; 

        var pricePerDay = Convert.ToInt32(priceObj);
        int totalDays = (dto.DateTo.Date - dto.DateFrom.Date).Days;
        int totalPrice = totalDays * pricePerDay;

        var insertClientCmd = new SqlCommand("INSERT INTO clients (FirstName, LastName, Address) OUTPUT INSERTED.ID VALUES (@fn, @ln, @addr)", conn, transaction);
        insertClientCmd.Parameters.AddWithValue("@fn", dto.Client.FirstName);
        insertClientCmd.Parameters.AddWithValue("@ln", dto.Client.LastName);
        insertClientCmd.Parameters.AddWithValue("@addr", dto.Client.Address);

        int newClientId = (int)await insertClientCmd.ExecuteScalarAsync();

        var insertRentalCmd = new SqlCommand(@"INSERT INTO car_rentals (ClientID, CarID, DateFrom, DateTo, TotalPrice) VALUES (@clientId, @carId, @from, @to, @price)", conn, transaction);
        insertRentalCmd.Parameters.AddWithValue("@clientId", newClientId);
        insertRentalCmd.Parameters.AddWithValue("@carId", dto.CarId);
        insertRentalCmd.Parameters.AddWithValue("@from", dto.DateFrom);
        insertRentalCmd.Parameters.AddWithValue("@to", dto.DateTo);
        insertRentalCmd.Parameters.AddWithValue("@price", totalPrice);

        await insertRentalCmd.ExecuteNonQueryAsync();

        transaction.Commit();
        return true;
    }
    catch
    {
        transaction.Rollback();
        throw;
    }
}


}