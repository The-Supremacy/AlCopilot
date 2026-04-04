using AlCopilot.DrinkCatalog;
using AlCopilot.Host.Messaging;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
builder.Services.AddDrinkCatalogModule(builder.Configuration);
builder.Services.AddDurableMessaging(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapDrinkCatalogEndpoints();
app.MapGet("/", () => "Hello World!");

app.Run();

// Make the implicit Program class accessible to WebApplicationFactory
public partial class Program;
