using Atm.Application;
using Atm.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

const string frontendCorsPolicy = "frontend";
builder.Services.AddCors(options =>
    options.AddPolicy(frontendCorsPolicy, policy => policy
        .WithOrigins("http://localhost:5173") // Vite dev server; frontend added in PR 5
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

// --- HTTP pipeline ---
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // serves /openapi/v1.json
}

app.UseHttpsRedirection();
app.UseCors(frontendCorsPolicy);
app.MapControllers();

app.Run();
