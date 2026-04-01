using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var drinkCatalogDb = postgres.AddDatabase("drink-catalog");

builder.AddProject<AlCopilot_Host>("alcopilot-host")
    .WithReference(drinkCatalogDb)
    .WaitFor(drinkCatalogDb);

builder.Build().Run();
