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

var qdrantApiKey = builder.AddParameter("qdrant-api-key", "QDRANT_API_KEY");
var qdrant = builder.AddQdrant("qdrant", qdrantApiKey)
    .WithDataVolume();

var migrator = builder.AddProject<AlCopilot_Migrator>("alcopilot-migrator")
    .WithReference(drinkCatalogDb)
    .WithReference(customerProfileDb)
    .WithReference(recommendationDb)
    .WaitFor(drinkCatalogDb)
    .WaitFor(customerProfileDb)
    .WaitFor(recommendationDb);

var host = builder.AddProject<AlCopilot_Host>("alcopilot-host")
    .WithEnvironment("Authentication__Management__Authority", managementAuthority)
    .WithEnvironment("Authentication__Management__ClientId", "alcopilot-management-portal")
    .WithEnvironment("Authentication__Management__ClientSecret", managementClientSecret)
    .WithEnvironment("Authentication__Customer__Authority", managementAuthority)
    .WithEnvironment("Authentication__Customer__ClientId", "alcopilot-web-portal")
    .WithEnvironment("Authentication__Customer__ClientSecret", customerClientSecret)
    .WithEnvironment("Recommendation__Semantic__QdrantEndpoint", qdrant.GetEndpoint("grpc"))
    .WithEnvironment("Recommendation__Semantic__QdrantApiKey", qdrantApiKey)
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

builder.AddViteApp("management-portal", "../../../web/apps/management-portal")
    .WithPnpm()
    .WithEndpoint("http", endpoint => endpoint.Port = 4173)
    .WithEnvironment("MANAGEMENT_API_PROXY_TARGET", host.GetEndpoint("http"))
    .WithReference(host)
    .WaitFor(host);

builder.AddViteApp("web-portal", "../../../web/apps/web-portal")
    .WithPnpm()
    .WithEndpoint("http", endpoint => endpoint.Port = 4174)
    .WithEnvironment("WEB_PORTAL_API_PROXY_TARGET", host.GetEndpoint("http"))
    .WithReference(host)
    .WaitFor(host);

builder.Build().Run();
