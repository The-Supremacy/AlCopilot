using AlCopilot.DrinkCatalog;
using AlCopilot.Shared.Errors;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
builder.Services.AddDrinkCatalogModule(builder.Configuration);
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        if (context.Exception is ApiException apiException)
        {
            context.ProblemDetails.Status = apiException.StatusCode;
            context.ProblemDetails.Title = apiException.Title;
            context.ProblemDetails.Detail = apiException.Message;
            context.HttpContext.Response.StatusCode = apiException.StatusCode;
        }

        if (builder.Environment.IsDevelopment() && context.Exception is not null)
        {
            context.ProblemDetails.Extensions["exception"] = context.Exception.ToString();
        }
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler();
}

app.MapDefaultEndpoints();

app.MapDrinkCatalogEndpoints();
app.MapGet("/", () => "Hello World!");

app.Run();

// Make the implicit Program class accessible to WebApplicationFactory
public partial class Program;
