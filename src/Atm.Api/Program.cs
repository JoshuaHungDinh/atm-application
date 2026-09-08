using System.Text.Json.Serialization;
using Atm.Api.ErrorHandling;
using Atm.Application;
using Atm.Infrastructure;
using Atm.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

const string frontendCorsPolicy = "frontend";
builder.Services.AddCors(options =>
    options.AddPolicy(frontendCorsPolicy, policy => policy
        .WithOrigins("http://localhost:5173") // Vite dev server; frontend added in PR 5
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

// Create/upgrade the SQLite database and seed the two accounts before serving traffic.
await app.Services.InitializeAtmDatabaseAsync();

// --- HTTP pipeline ---
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // serves /openapi/v1.json
}

app.UseHttpsRedirection();
app.UseCors(frontendCorsPolicy);
app.MapControllers();

app.Run();

/// <summary>Exposed so <c>WebApplicationFactory&lt;Program&gt;</c> can bootstrap the API in tests.</summary>
public partial class Program;
