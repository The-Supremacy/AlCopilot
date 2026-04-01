using AlCopilot.DrinkCatalog;
using AlCopilot.DrinkCatalog.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
builder.Services.AddDrinkCatalogModule(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();

// Run migrations on startup (dev — production uses explicit tooling)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DrinkCatalogDbContext>();
    await db.Database.MigrateAsync();
}

app.MapDrinkCatalogEndpoints();
app.MapGet("/", () => "Hello World!");

app.Run();

// Make the implicit Program class accessible to WebApplicationFactory
public partial class Program;
