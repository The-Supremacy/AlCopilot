using AlCopilot.CustomerProfile;
using AlCopilot.CustomerProfile.Data;
using AlCopilot.DrinkCatalog;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.Recommendation;
using AlCopilot.Recommendation.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDrinkCatalogModule(builder.Configuration);
builder.Services.AddCustomerProfileModule(builder.Configuration);
builder.Services.AddRecommendationModule(builder.Configuration);

using var host = builder.Build();
using var scope = host.Services.CreateScope();

var logger = scope.ServiceProvider
    .GetRequiredService<ILoggerFactory>()
    .CreateLogger("DatabaseMigrator");

logger.LogInformation("Applying database migrations for Drink Catalog.");

var dbContext = scope.ServiceProvider.GetRequiredService<DrinkCatalogDbContext>();
await dbContext.Database.MigrateAsync();

logger.LogInformation("Applying database migrations for Customer Profile.");

var customerProfileDbContext = scope.ServiceProvider.GetRequiredService<CustomerProfileDbContext>();
await customerProfileDbContext.Database.MigrateAsync();

logger.LogInformation("Applying database migrations for Recommendation.");

var recommendationDbContext = scope.ServiceProvider.GetRequiredService<RecommendationDbContext>();
await recommendationDbContext.Database.MigrateAsync();

logger.LogInformation("Database migrations applied successfully.");
