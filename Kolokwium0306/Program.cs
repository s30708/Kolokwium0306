
using Kolokwium0306.Services;

var builder = WebApplication.CreateBuilder();
builder.Services.AddControllers();
builder.Services.AddScoped<IClientService, ClientService>();
var app = builder.Build();
app.UseAuthorization();
app.MapControllers();
app.Run();