using AlCopilot.DrinkCatalog;
using AlCopilot.DrinkCatalog.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDrinkCatalogModule(builder.Configuration);

using var host = builder.Build();
using var scope = host.Services.CreateScope();

var logger = scope.ServiceProvider
    .GetRequiredService<ILoggerFactory>()
    .CreateLogger("DatabaseMigrator");

logger.LogInformation("Applying database migrations for Drink Catalog.");

var dbContext = scope.ServiceProvider.GetRequiredService<DrinkCatalogDbContext>();
await dbContext.Database.MigrateAsync();

logger.LogInformation("Database migrations applied successfully.");
