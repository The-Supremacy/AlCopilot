var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapGet("/", () => "Hello World!");

app.Run();

// Make the implicit Program class accessible to WebApplicationFactory
public partial class Program;
