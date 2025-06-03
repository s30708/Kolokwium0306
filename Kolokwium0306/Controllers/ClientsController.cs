using Kolokwium0306.Models;
using Kolokwium0306.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kolokwium0306.Controllers;


[Route("api/[controller]")]
[ApiController]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }
    
    
    
    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        if (!_clientService.ClientExists(id).Result)
        {
            return Conflict("Wskazany klient nie istnieje ");
        }
        
        var res = _clientService.GetClient(id);
           
        return Ok(res.Result);
    }
    
    
    
    
    [HttpPost]
    public async Task<IActionResult> CreateClient([FromBody] CreateClientRentalDTO dto)
    {
        bool result = await _clientService.CreateClientWithRental(dto);
        if (!result)
            return NotFound("nie ma takiego auta");

        return Ok("kilent zapisany");
    }

}