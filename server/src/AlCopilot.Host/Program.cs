using AlCopilot.DrinkCatalog;
using AlCopilot.Host.Authentication;
using AlCopilot.Shared.Errors;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
builder.Services.AddManagementAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddManagementAuthorization();
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
        else if (DatabaseExceptionMapper.TryMapDatabaseException(context.Exception, out var mappedException))
        {
            context.ProblemDetails.Status = mappedException.StatusCode;
            context.ProblemDetails.Title = mappedException.Title;
            context.ProblemDetails.Detail = mappedException.Message;
            context.HttpContext.Response.StatusCode = mappedException.StatusCode;
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

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

app.MapManagementAuthEndpoints();
app.MapDrinkCatalogEndpoints(ManagementAuthorizationPolicies.CanAccessManagementPortal);
app.MapGet("/", () => "Hello World!");

app.Run();

// Make the implicit Program class accessible to WebApplicationFactory
public partial class Program;

internal static class DatabaseExceptionMapper
{
    internal static bool TryMapDatabaseException(Exception? exception, out ApiException mappedException)
    {
        if (exception is DbUpdateException { InnerException: PostgresException postgresException } &&
            string.Equals(postgresException.SqlState, PostgresErrorCodes.UniqueViolation, StringComparison.Ordinal))
        {
            mappedException = new ConflictException("The requested change conflicts with an existing record.");
            return true;
        }

        mappedException = null!;
        return false;
    }
}
