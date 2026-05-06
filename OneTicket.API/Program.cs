using OneTicket.API.Services;
using OneKhusa.SDK;
using OneKhusa.SDK.Extensions;
using Swashbuckle.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Register core ASP.NET services for MVC controllers, API exploration, and HTTP client factory
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();


// Register in-memory ticket status tracker for real-time payment state management
builder.Services.AddSingleton<TicketTracker>();

// Configure CORS policy to allow requests from React frontend at localhost:5173
builder.Services.AddCors(options => {
    options.AddPolicy("AllowReactApp", policy => {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure OneKhusa SDK client with merchant credentials from configuration
// Credentials are loaded from appsettings.json for sandbox environment
builder.Services.AddOneKhusaClient(options => {
    options.ApiKey = builder.Configuration["OneKhusa:ApiKey"] ?? "";
    options.ApiSecret = builder.Configuration["OneKhusa:ApiSecret"] ?? "";
    options.OrganisationId = builder.Configuration["OneKhusa:OrganizationId"] ?? "";

    if (int.TryParse(builder.Configuration["OneKhusa:MerchantAccountNumber"], out int accNo))
        options.MerchantAccountNumber = accNo;

    options.IsSandbox = true;
});

var app = builder.Build();

// Enable API documentation with Swagger UI for interactive endpoint testing
app.UseSwagger();
app.UseSwaggerUI();

// Apply CORS policy to all endpoints
app.UseCors("AllowReactApp");
app.UseAuthorization();
app.MapControllers();

app.Run();