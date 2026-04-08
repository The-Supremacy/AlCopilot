using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var drinkCatalogDb = postgres.AddDatabase("drink-catalog");

var migrator = builder.AddProject<AlCopilot_Migrator>("alcopilot-migrator")
    .WithReference(drinkCatalogDb)
    .WaitFor(drinkCatalogDb);

builder.AddProject<AlCopilot_Host>("alcopilot-host")
    .WithReference(drinkCatalogDb)
    .WaitFor(drinkCatalogDb)
    .WaitForCompletion(migrator);

builder.Build().Run();
