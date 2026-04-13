using Projects;

var builder = DistributedApplication.CreateBuilder(args);

const string managementClientSecret = "alcopilot-management-dev-secret";
const string managementAuthority = "http://localhost:8080/realms/alcopilot";

var keycloak = builder.AddContainer("keycloak", "quay.io/keycloak/keycloak", "26.2")
    .WithHttpEndpoint(port: 8080, targetPort: 8080)
    .WithBindMount("./Keycloak/Realms", "/opt/keycloak/data/import")
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_USERNAME", "admin")
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_PASSWORD", "admin")
    .WithEnvironment("KC_HTTP_ENABLED", "true")
    .WithArgs("start-dev", "--import-realm");

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("postgres-data")
    .WithPgAdmin();
var drinkCatalogDb = postgres.AddDatabase("drink-catalog");

var migrator = builder.AddProject<AlCopilot_Migrator>("alcopilot-migrator")
    .WithReference(drinkCatalogDb)
    .WaitFor(drinkCatalogDb);

builder.AddProject<AlCopilot_Host>("alcopilot-host")
    .WithEnvironment("Authentication__Management__Authority", managementAuthority)
    .WithEnvironment("Authentication__Management__ClientId", "alcopilot-management-portal")
    .WithEnvironment("Authentication__Management__ClientSecret", managementClientSecret)
    .WithReference(drinkCatalogDb)
    .WaitFor(keycloak)
    .WaitFor(drinkCatalogDb)
    .WaitForCompletion(migrator);

builder.Build().Run();
