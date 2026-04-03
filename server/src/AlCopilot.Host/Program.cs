using AlCopilot.DrinkCatalog;
using AlCopilot.DrinkCatalog.Contracts.Events;
using AlCopilot.Host.Messaging;
using AlCopilot.Shared.Domain;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddSingleton(DomainEventTypeRegistry.CreateFrom(typeof(DrinkCreatedEvent).Assembly));
builder.Services.AddDrinkCatalogModule(builder.Configuration);
builder.Services.AddDurableMessaging(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapGet("/", () => "Hello World!");

app.Run();

// Make the implicit Program class accessible to WebApplicationFactory
public partial class Program;
