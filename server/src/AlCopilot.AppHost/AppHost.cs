using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<AlCopilot_Host>("alcopilot-host");

builder.Build().Run();
