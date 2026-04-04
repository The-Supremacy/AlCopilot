using Projects;

var builder = DistributedApplication.CreateBuilder(args);
var emulatorConfigPath = Path.GetFullPath(Path.Combine(
    Directory.GetCurrentDirectory(),
    "..",
    "..",
    "tests",
    "AlCopilot.Host.Tests",
    "ServiceBusEmulator.Config.json"));
const string sqlPassword = "Your_strong_Password123";

var serviceBusSql = builder.AddContainer("servicebus-sql", "mcr.microsoft.com/mssql/server", "2022-latest")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("MSSQL_SA_PASSWORD", sqlPassword)
    .WithEnvironment("MSSQL_PID", "Developer")
    .WithEndpoint(name: "sql", targetPort: 1433);

var serviceBusEmulator = builder.AddContainer(
        "servicebus-emulator",
        "mcr.microsoft.com/azure-messaging/servicebus-emulator",
        "latest")
    .WithBindMount(emulatorConfigPath, "/ServiceBus_Emulator/ConfigFiles/Config.json", isReadOnly: true)
    .WithEnvironment("SQL_SERVER", "servicebus-sql")
    .WithEnvironment("MSSQL_SA_PASSWORD", sqlPassword)
    .WithEndpoint(name: "amqp", targetPort: 5672)
    .WithEndpoint(name: "management", targetPort: 9354)
    .WithEndpoint(name: "management-https", targetPort: 9355)
    .WaitFor(serviceBusSql);

var postgres = builder.AddPostgres("postgres");
var drinkCatalogDb = postgres.AddDatabase("drink-catalog");

var migrator = builder.AddProject<AlCopilot_Migrator>("alcopilot-migrator")
    .WithReference(drinkCatalogDb)
    .WaitFor(drinkCatalogDb);

builder.AddProject<AlCopilot_Host>("alcopilot-host")
    .WithReference(drinkCatalogDb)
    .WaitFor(drinkCatalogDb)
    .WaitForCompletion(migrator)
    .WaitFor(serviceBusEmulator)
    .WithEnvironment(
        "ConnectionStrings__messaging",
        "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;");

builder.Build().Run();
