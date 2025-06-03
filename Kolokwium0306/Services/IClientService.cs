using Kolokwium0306.Models;

namespace Kolokwium0306.Services;

public interface IClientService
{
    public Task<ClientDTO> GetClient(int id);
    public Task<bool> CreateClientWithRental(CreateClientRentalDTO dto);
   public Task<bool> ClientExists(int clientId);
}