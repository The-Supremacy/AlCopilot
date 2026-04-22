using Projects;

var builder = DistributedApplication.CreateBuilder(args);

const string managementClientSecret = "alcopilot-management-dev-secret";
const string customerClientSecret = "alcopilot-customer-dev-secret";
const string managementAuthority = "http://localhost:8080/realms/alcopilot";
const string defaultRecommendationProvider = "ollama";
const string defaultRecommendationOllamaModelId = "gemma4:e4b";

var recommendationProvider = builder.Configuration["Recommendation:Llm:Provider"] ?? defaultRecommendationProvider;
var recommendationOllamaEndpoint = builder.Configuration["Recommendation:Ollama:Endpoint"]
    ?? throw new InvalidOperationException(
        "AppHost configuration value 'Recommendation:Ollama:Endpoint' is required.");
var recommendationOllamaModelId = builder.Configuration["Recommendation:Ollama:ModelId"] ?? defaultRecommendationOllamaModelId;
var recommendationOllamaMaxHistoryMessages = builder.Configuration["Recommendation:Ollama:MaxHistoryMessages"];

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
    .WithEnvironment("Recommendation__Llm__Provider", recommendationProvider)
    .WithEnvironment("Recommendation__Ollama__Endpoint", recommendationOllamaEndpoint)
    .WithEnvironment("Recommendation__Ollama__ModelId", recommendationOllamaModelId)
    .WithEnvironment("Recommendation__Ollama__MaxHistoryMessages", recommendationOllamaMaxHistoryMessages)
    .WithReference(drinkCatalogDb)
    .WithReference(customerProfileDb)
    .WithReference(recommendationDb)
    .WithReference(qdrant)
    .WaitFor(keycloak)
    .WaitFor(drinkCatalogDb)
    .WaitFor(customerProfileDb)
    .WaitFor(recommendationDb)
    .WaitFor(qdrant)
    .WaitForCompletion(migrator);

builder.Build().Run();
