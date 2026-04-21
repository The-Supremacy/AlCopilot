using Projects;

var builder = DistributedApplication.CreateBuilder(args);

const string managementClientSecret = "alcopilot-management-dev-secret";
const string customerClientSecret = "alcopilot-customer-dev-secret";
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
var customerProfileDb = postgres.AddDatabase("customer-profile");
var recommendationDb = postgres.AddDatabase("recommendation");

var ollama = builder.AddOllamaLocal("ollama");
var recommendationModel = ollama.AddModel("gemma4", "gemma4:e4b");

var parameter = builder.AddParameter("quadrant-api-key", "QDRANT_API_KEY");
var qdrant = builder.AddQdrant("qdrant", parameter)
    .WithDataVolume();

var migrator = builder.AddProject<AlCopilot_Migrator>("alcopilot-migrator")
    .WithReference(drinkCatalogDb)
    .WithReference(customerProfileDb)
    .WithReference(recommendationDb)
    .WaitFor(drinkCatalogDb)
    .WaitFor(customerProfileDb)
    .WaitFor(recommendationDb);

builder.AddProject<AlCopilot_Host>("alcopilot-host")
    .WithEnvironment("Authentication__Management__Authority", managementAuthority)
    .WithEnvironment("Authentication__Management__ClientId", "alcopilot-management-portal")
    .WithEnvironment("Authentication__Management__ClientSecret", managementClientSecret)
    .WithEnvironment("Authentication__Customer__Authority", managementAuthority)
    .WithEnvironment("Authentication__Customer__ClientId", "alcopilot-web-portal")
    .WithEnvironment("Authentication__Customer__ClientSecret", customerClientSecret)
    .WithEnvironment("Recommendation__Llm__Provider", "ollama")
    .WithEnvironment("Recommendation__Ollama__Endpoint", "http://localhost:11434")
    .WithEnvironment("Recommendation__Ollama__ModelId", "gemma4:e4b")
    .WithReference(drinkCatalogDb)
    .WithReference(customerProfileDb)
    .WithReference(recommendationDb)
    .WithReference(qdrant)
    .WithReference(recommendationModel)
    .WaitFor(keycloak)
    .WaitFor(drinkCatalogDb)
    .WaitFor(customerProfileDb)
    .WaitFor(recommendationDb)
    .WaitFor(qdrant)
    .WaitFor(recommendationModel)
    .WaitForCompletion(migrator);

builder.Build().Run();
