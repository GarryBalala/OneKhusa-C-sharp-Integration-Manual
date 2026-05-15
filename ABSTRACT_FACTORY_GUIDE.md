# Abstract Factory Pattern Implementation Guide
## OneTicket.API Integration Manual

This guide demonstrates how to implement the **Abstract Factory Pattern** in your OneTicket.API project to create payment processing abstractions. This pattern allows you to decouple payment service creation from business logic.

---

## 📋 Table of Contents

1. [What is Abstract Factory Pattern?](#what-is-abstract-factory-pattern)
2. [Project Structure](#project-structure)
3. [Installation & Setup](#installation--setup)
4. [Implementation Steps](#implementation-steps)
5. [Code Snippets](#code-snippets)
6. [Testing Guide](#testing-guide)

---

## What is Abstract Factory Pattern?

The **Abstract Factory Pattern** is a creational design pattern that:
- Provides an interface for creating families of related or dependent objects
- Encapsulates object creation logic
- Decouples client code from concrete implementations
- Makes it easy to add new payment providers without modifying existing code

**Benefits:**
✅ Loose coupling between payment services and controllers  
✅ Easy to switch payment providers (OneKhusa, PayPal, Stripe, etc.)  
✅ Testable code with dependency injection  
✅ Follows SOLID principles  

---

## Project Structure

Your enhanced project structure will look like this:

```
OneTicket-Ecosystem/
│
├── OneTicket.API/
│   ├── Controllers/
│   │   ├── PaymentController.cs          # NEW: Payment endpoint
│   │   ├── WebhooksController.cs         # Webhook handler
│   │   └── TicketsController.cs          # Existing
│   │
│   ├── Services/
│   │   ├── TicketTracker.cs              # Existing
│   │   └── Payments/                     # NEW: Payment services folder
│   │       ├── Abstractions/
│   │       │   ├── IPaymentProvider.cs
│   │       │   ├── IPaymentFactory.cs
│   │       │   └── PaymentDto.cs
│   │       ├── Providers/
│   │       │   ├── OneKhusaPaymentProvider.cs
│   │       │   ├── StripePaymentProvider.cs    # Optional: Example
│   │       │   └── PayPalPaymentProvider.cs    # Optional: Example
│   │       └── Factory/
│   │           └── PaymentProviderFactory.cs
│   │
│   ├── DTOs/
│   │   ├── OneKhusaWebhook.cs            # Existing
│   │   ├── SinglePayoutRequest.cs        # Existing
│   │   └── PaymentRequest.cs             # NEW
│   │
│   ├── Program.cs                        # Updated with DI
│   ├── appsettings.json                  # Updated config
│   └── OneTicket.API.csproj              # Updated dependencies
│
└── OneTicket.UI/
    └── src/
        └── services/
            └── paymentApi.js             # NEW: Frontend integration
```

---

## Installation & Setup

### Step 1: Update Project File

Update your `OneTicket.API.csproj` to include necessary dependencies:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="ClosedXML" Version="0.105.0" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.3" />
    <PackageReference Include="OneKhusa.SDK" Version="2026.1.5.2" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.5" />
  </ItemGroup>
</Project>
```

### Step 2: Create Required Directories

```bash
# Navigate to OneTicket.API
cd OneTicket.API

# Create Services/Payments structure
mkdir -p Services/Payments/Abstractions
mkdir -p Services/Payments/Providers
mkdir -p Services/Payments/Factory
```

### Step 3: Update appsettings.json

Add payment provider configuration:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "OneKhusa": {
    "BaseUrl": "https://api.onekhusa.com/sandbox/v1/",
    "OrganizationId": "YOUR_ORG_ID",
    "MerchantAccountNumber": 79619974,
    "ApiKey": "YOUR_API_KEY",
    "ApiSecret": "YOUR_API_SECRET",
    "WebhookSecret": "YOUR_WEBHOOK_SECRET",
    "PortalEmail": "balalagarry@gmail.com"
  },
  "PaymentProviders": {
    "Default": "OneKhusa",
    "OneKhusa": {
      "Enabled": true,
      "MerchantId": "79619974",
      "CallbackUrl": "https://your-tunnel.ngrok-free.dev/api/webhooks/payment"
    },
    "Stripe": {
      "Enabled": false,
      "ApiKey": "YOUR_STRIPE_KEY"
    },
    "PayPal": {
      "Enabled": false,
      "ClientId": "YOUR_PAYPAL_CLIENT_ID"
    }
  }
}
```

---

## Implementation Steps

### Step 1: Create Abstract Interfaces

These interfaces define the contract for all payment providers.

**File:** `Services/Payments/Abstractions/IPaymentProvider.cs`

```csharp
namespace OneTicket.API.Services.Payments.Abstractions;

/// <summary>
/// Interface for payment providers implementing Abstract Factory Pattern
/// Defines the contract that all payment providers must implement
/// </summary>
public interface IPaymentProvider
{
    /// <summary>
    /// Get the provider name (e.g., "OneKhusa", "Stripe", "PayPal")
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Initiate a payment transaction
    /// </summary>
    Task<PaymentInitiationResponse> InitiatePaymentAsync(PaymentRequest request);

    /// <summary>
    /// Verify payment status
    /// </summary>
    Task<PaymentVerificationResponse> VerifyPaymentAsync(string transactionId);

    /// <summary>
    /// Process refund
    /// </summary>
    Task<PaymentRefundResponse> RefundAsync(string transactionId, decimal amount);

    /// <summary>
    /// Get webhook signature validation
    /// </summary>
    bool ValidateWebhookSignature(string payload, string signature);
}

/// <summary>
/// Payment initiation response DTO
/// </summary>
public class PaymentInitiationResponse
{
    public bool Success { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string ProviderTransactionId { get; set; } = string.Empty;
    public string RedirectUrl { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Payment verification response DTO
/// </summary>
public class PaymentVerificationResponse
{
    public string TransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // SUCCESS, PENDING, FAILED
    public decimal Amount { get; set; }
    public DateTime ProcessedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Payment refund response DTO
/// </summary>
public class PaymentRefundResponse
{
    public bool Success { get; set; }
    public string RefundId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}
```

**File:** `Services/Payments/Abstractions/IPaymentFactory.cs`

```csharp
namespace OneTicket.API.Services.Payments.Abstractions;

/// <summary>
/// Abstract Factory for creating payment provider instances
/// Implements the Factory Pattern for loose coupling
/// </summary>
public interface IPaymentFactory
{
    /// <summary>
    /// Create a payment provider instance by name
    /// </summary>
    IPaymentProvider CreatePaymentProvider(string providerName);

    /// <summary>
    /// Get default payment provider
    /// </summary>
    IPaymentProvider GetDefaultProvider();

    /// <summary>
    /// Get all available payment providers
    /// </summary>
    IEnumerable<string> GetAvailableProviders();
}
```

**File:** `Services/Payments/Abstractions/PaymentDto.cs`

```csharp
namespace OneTicket.API.Services.Payments.Abstractions;

/// <summary>
/// Payment request DTO
/// </summary>
public class PaymentRequest
{
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Description { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string NotificationUrl { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Payment configuration DTO
/// </summary>
public class PaymentProviderConfig
{
    public bool Enabled { get; set; }
    public Dictionary<string, string> Settings { get; set; } = new();
}
```

---

### Step 2: Create Concrete Payment Providers

**File:** `Services/Payments/Providers/OneKhusaPaymentProvider.cs`

```csharp
using OneTicket.API.Services.Payments.Abstractions;
using OneKhusa.SDK;
using System.Security.Cryptography;
using System.Text;

namespace OneTicket.API.Services.Payments.Providers;

/// <summary>
/// OneKhusa payment provider implementation
/// Concrete implementation of IPaymentProvider for OneKhusa
/// </summary>
public class OneKhusaPaymentProvider : IPaymentProvider
{
    public string ProviderName => "OneKhusa";

    private readonly IOneKhusaClient _oneKhusaClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OneKhusaPaymentProvider> _logger;

    public OneKhusaPaymentProvider(
        IOneKhusaClient oneKhusaClient,
        IConfiguration configuration,
        ILogger<OneKhusaPaymentProvider> logger)
    {
        _oneKhusaClient = oneKhusaClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<PaymentInitiationResponse> InitiatePaymentAsync(PaymentRequest request)
    {
        try
        {
            _logger.LogInformation("Initiating OneKhusa payment for order {OrderId}", request.OrderId);

            // Build OneKhusa request
            var oneKhusaRequest = new
            {
                amount = request.Amount,
                currency = request.Currency,
                externalId = request.OrderId,
                description = request.Description,
                payer = new
                {
                    partyIdType = "MSISDN",
                    partyId = request.CustomerPhone
                },
                payerMessage = "Payment for ticket",
                payeeNote = request.Description
            };

            // Call OneKhusa API to initiate payment
            // This is a placeholder - adapt to actual SDK methods
            var result = await _oneKhusaClient.InitiatePaymentAsync(oneKhusaRequest);

            return new PaymentInitiationResponse
            {
                Success = true,
                TransactionId = request.OrderId,
                ProviderTransactionId = result?.TransactionId ?? string.Empty,
                RedirectUrl = result?.RedirectUrl ?? string.Empty,
                Message = "Payment initiated successfully",
                Metadata = new Dictionary<string, object>
                {
                    { "provider", "OneKhusa" },
                    { "timestamp", DateTime.UtcNow }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating OneKhusa payment for order {OrderId}", request.OrderId);
            return new PaymentInitiationResponse
            {
                Success = false,
                Message = $"Payment initiation failed: {ex.Message}"
            };
        }
    }

    public async Task<PaymentVerificationResponse> VerifyPaymentAsync(string transactionId)
    {
        try
        {
            _logger.LogInformation("Verifying OneKhusa payment for transaction {TransactionId}", transactionId);

            // Verify with OneKhusa API
            var result = await _oneKhusaClient.GetTransactionStatusAsync(transactionId);

            return new PaymentVerificationResponse
            {
                TransactionId = transactionId,
                Status = result?.Status ?? "UNKNOWN",
                Amount = result?.Amount ?? 0,
                ProcessedAt = DateTime.UtcNow,
                Message = "Verification complete"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying OneKhusa payment for transaction {TransactionId}", transactionId);
            return new PaymentVerificationResponse
            {
                TransactionId = transactionId,
                Status = "ERROR",
                Message = ex.Message
            };
        }
    }

    public async Task<PaymentRefundResponse> RefundAsync(string transactionId, decimal amount)
    {
        try
        {
            _logger.LogInformation("Processing OneKhusa refund for transaction {TransactionId}", transactionId);

            // Process refund via OneKhusa API
            var result = await _oneKhusaClient.RefundAsync(transactionId, amount);

            return new PaymentRefundResponse
            {
                Success = true,
                RefundId = result?.RefundId ?? Guid.NewGuid().ToString(),
                Message = "Refund processed successfully",
                ProcessedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OneKhusa refund for transaction {TransactionId}", transactionId);
            return new PaymentRefundResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public bool ValidateWebhookSignature(string payload, string signature)
    {
        try
        {
            var secret = _configuration["OneKhusa:WebhookSecret"];
            if (string.IsNullOrEmpty(secret)) return false;

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                var computedSignature = Convert.ToBase64String(
                    hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
                
                return computedSignature == signature;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating OneKhusa webhook signature");
            return false;
        }
    }
}
```

**File:** `Services/Payments/Providers/StripePaymentProvider.cs`

```csharp
using OneTicket.API.Services.Payments.Abstractions;

namespace OneTicket.API.Services.Payments.Providers;

/// <summary>
/// Stripe payment provider implementation
/// Concrete implementation of IPaymentProvider for Stripe
/// </summary>
public class StripePaymentProvider : IPaymentProvider
{
    public string ProviderName => "Stripe";

    private readonly IConfiguration _configuration;
    private readonly ILogger<StripePaymentProvider> _logger;

    public StripePaymentProvider(
        IConfiguration configuration,
        ILogger<StripePaymentProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task<PaymentInitiationResponse> InitiatePaymentAsync(PaymentRequest request)
    {
        _logger.LogInformation("Initiating Stripe payment for order {OrderId}", request.OrderId);
        
        // TODO: Implement Stripe integration
        return Task.FromResult(new PaymentInitiationResponse
        {
            Success = false,
            Message = "Stripe provider not yet implemented"
        });
    }

    public Task<PaymentVerificationResponse> VerifyPaymentAsync(string transactionId)
    {
        _logger.LogInformation("Verifying Stripe payment for transaction {TransactionId}", transactionId);
        
        // TODO: Implement Stripe verification
        return Task.FromResult(new PaymentVerificationResponse
        {
            TransactionId = transactionId,
            Status = "UNIMPLEMENTED",
            Message = "Stripe verification not yet implemented"
        });
    }

    public Task<PaymentRefundResponse> RefundAsync(string transactionId, decimal amount)
    {
        _logger.LogInformation("Processing Stripe refund for transaction {TransactionId}", transactionId);
        
        // TODO: Implement Stripe refund
        return Task.FromResult(new PaymentRefundResponse
        {
            Success = false,
            Message = "Stripe refund not yet implemented"
        });
    }

    public bool ValidateWebhookSignature(string payload, string signature)
    {
        _logger.LogInformation("Validating Stripe webhook signature");
        
        // TODO: Implement Stripe webhook validation
        return false;
    }
}
```

---

### Step 3: Create the Payment Factory

**File:** `Services/Payments/Factory/PaymentProviderFactory.cs`

```csharp
using OneTicket.API.Services.Payments.Abstractions;
using OneTicket.API.Services.Payments.Providers;

namespace OneTicket.API.Services.Payments.Factory;

/// <summary>
/// Payment provider factory implementation
/// Concrete implementation of IPaymentFactory using Abstract Factory Pattern
/// </summary>
public class PaymentProviderFactory : IPaymentFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentProviderFactory> _logger;
    private readonly Dictionary<string, Type> _providerRegistry;

    public PaymentProviderFactory(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<PaymentProviderFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;

        // Register available payment providers
        _providerRegistry = new Dictionary<string, Type>
        {
            { "OneKhusa", typeof(OneKhusaPaymentProvider) },
            { "Stripe", typeof(StripePaymentProvider) },
            // Add more providers here as needed
        };
    }

    public IPaymentProvider CreatePaymentProvider(string providerName)
    {
        _logger.LogInformation("Creating payment provider: {ProviderName}", providerName);

        if (!_providerRegistry.ContainsKey(providerName))
        {
            _logger.LogError("Payment provider not found: {ProviderName}", providerName);
            throw new InvalidOperationException($"Payment provider '{providerName}' not found");
        }

        var providerType = _providerRegistry[providerName];
        var provider = _serviceProvider.GetService(providerType) as IPaymentProvider;

        if (provider == null)
        {
            _logger.LogError("Failed to instantiate payment provider: {ProviderName}", providerName);
            throw new InvalidOperationException($"Failed to create payment provider '{providerName}'");
        }

        _logger.LogInformation("Successfully created payment provider: {ProviderName}", providerName);
        return provider;
    }

    public IPaymentProvider GetDefaultProvider()
    {
        var defaultProvider = _configuration["PaymentProviders:Default"] ?? "OneKhusa";
        _logger.LogInformation("Getting default payment provider: {DefaultProvider}", defaultProvider);
        return CreatePaymentProvider(defaultProvider);
    }

    public IEnumerable<string> GetAvailableProviders()
    {
        var availableProviders = _providerRegistry.Keys.ToList();
        _logger.LogInformation("Available payment providers: {Providers}", string.Join(", ", availableProviders));
        return availableProviders;
    }

    /// <summary>
    /// Register a new payment provider at runtime
    /// Allows for dynamic provider registration
    /// </summary>
    public void RegisterProvider(string name, Type providerType)
    {
        if (!typeof(IPaymentProvider).IsAssignableFrom(providerType))
        {
            throw new InvalidOperationException($"Type {providerType.Name} must implement IPaymentProvider");
        }

        _providerRegistry[name] = providerType;
        _logger.LogInformation("Registered payment provider: {ProviderName}", name);
    }
}
```

---

### Step 4: Create Payment Controller

**File:** `Controllers/PaymentController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using OneTicket.API.Services.Payments.Abstractions;
using OneTicket.API.Services.Payments.Factory;

namespace OneTicket.API.Controllers;

/// <summary>
/// Payment controller handling payment operations
/// Demonstrates usage of Abstract Factory Pattern through dependency injection
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentFactory _paymentFactory;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IPaymentFactory paymentFactory,
        ILogger<PaymentController> logger)
    {
        _paymentFactory = paymentFactory;
        _logger = logger;
    }

    /// <summary>
    /// Initiate a payment using the specified provider
    /// </summary>
    [HttpPost("initiate")]
    public async Task<IActionResult> InitiatePayment([FromBody] PaymentInitiateRequest request)
    {
        try
        {
            _logger.LogInformation("Initiating payment for order {OrderId} with provider {Provider}", 
                request.OrderId, request.Provider ?? "default");

            // Create payment provider instance using factory
            var provider = string.IsNullOrEmpty(request.Provider)
                ? _paymentFactory.GetDefaultProvider()
                : _paymentFactory.CreatePaymentProvider(request.Provider);

            // Build payment request
            var paymentRequest = new PaymentRequest
            {
                OrderId = request.OrderId,
                Amount = request.Amount,
                Currency = request.Currency ?? "USD",
                Description = request.Description,
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                CustomerPhone = request.CustomerPhone,
                ReturnUrl = request.ReturnUrl,
                NotificationUrl = request.NotificationUrl,
                Metadata = request.Metadata
            };

            // Initiate payment
            var response = await provider.InitiatePaymentAsync(paymentRequest);

            if (response.Success)
            {
                _logger.LogInformation("Payment initiated successfully for order {OrderId}", request.OrderId);
                return Ok(response);
            }
            else
            {
                _logger.LogWarning("Payment initiation failed for order {OrderId}: {Message}", 
                    request.OrderId, response.Message);
                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating payment for order {OrderId}", request.OrderId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Verify payment status
    /// </summary>
    [HttpGet("verify/{transactionId}")]
    public async Task<IActionResult> VerifyPayment(string transactionId, [FromQuery] string? provider = null)
    {
        try
        {
            _logger.LogInformation("Verifying payment for transaction {TransactionId}", transactionId);

            var paymentProvider = string.IsNullOrEmpty(provider)
                ? _paymentFactory.GetDefaultProvider()
                : _paymentFactory.CreatePaymentProvider(provider);

            var response = await paymentProvider.VerifyPaymentAsync(transactionId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying payment for transaction {TransactionId}", transactionId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Process refund
    /// </summary>
    [HttpPost("refund")]
    public async Task<IActionResult> RefundPayment([FromBody] PaymentRefundRequest request)
    {
        try
        {
            _logger.LogInformation("Processing refund for transaction {TransactionId}", request.TransactionId);

            var paymentProvider = string.IsNullOrEmpty(request.Provider)
                ? _paymentFactory.GetDefaultProvider()
                : _paymentFactory.CreatePaymentProvider(request.Provider);

            var response = await paymentProvider.RefundAsync(request.TransactionId, request.Amount);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for transaction {TransactionId}", request.TransactionId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get available payment providers
    /// </summary>
    [HttpGet("providers")]
    public IActionResult GetAvailableProviders()
    {
        try
        {
            var providers = _paymentFactory.GetAvailableProviders();
            return Ok(new { providers = providers.ToList() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available providers");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

/// <summary>
/// Payment initiation request DTO
/// </summary>
public class PaymentInitiateRequest
{
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public string? Description { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? ReturnUrl { get; set; }
    public string? NotificationUrl { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public string? Provider { get; set; } // Optional: specify payment provider
}

/// <summary>
/// Payment refund request DTO
/// </summary>
public class PaymentRefundRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Provider { get; set; }
}
```

---

### Step 5: Update Program.cs with Dependency Injection

**File:** `Program.cs`

```csharp
using OneTicket.API.Services;
using OneTicket.API.Services.Payments.Abstractions;
using OneTicket.API.Services.Payments.Factory;
using OneTicket.API.Services.Payments.Providers;
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

// ============================================
// Register Abstract Factory Pattern Services
// ============================================

// Register concrete payment providers
builder.Services.AddScoped<OneKhusaPaymentProvider>();
builder.Services.AddScoped<StripePaymentProvider>();

// Register the payment factory
builder.Services.AddScoped<IPaymentFactory, PaymentProviderFactory>();

// ============================================

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
```

---

## Testing Guide

### Unit Tests

**File:** `OneTicket.API.Tests/Services/Payments/PaymentFactoryTests.cs`

```csharp
using Xunit;
using Moq;
using OneTicket.API.Services.Payments.Abstractions;
using OneTicket.API.Services.Payments.Factory;
using OneTicket.API.Services.Payments.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OneTicket.API.Tests.Services.Payments;

public class PaymentFactoryTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<PaymentProviderFactory>> _mockLogger;
    private readonly PaymentProviderFactory _factory;

    public PaymentFactoryTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<PaymentProviderFactory>>();
        
        _factory = new PaymentProviderFactory(
            _mockServiceProvider.Object,
            _mockConfiguration.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void GetAvailableProviders_ReturnsNonEmptyList()
    {
        // Arrange & Act
        var providers = _factory.GetAvailableProviders();

        // Assert
        Assert.NotNull(providers);
        Assert.NotEmpty(providers);
        Assert.Contains("OneKhusa", providers);
        Assert.Contains("Stripe", providers);
    }

    [Fact]
    public void CreatePaymentProvider_WithValidProvider_ReturnsProvider()
    {
        // Arrange
        var mockProvider = new Mock<IPaymentProvider>();
        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(OneKhusaPaymentProvider)))
            .Returns(mockProvider.Object);

        // Act
        var provider = _factory.CreatePaymentProvider("OneKhusa");

        // Assert
        Assert.NotNull(provider);
        Assert.IsAssignableFrom<IPaymentProvider>(provider);
    }

    [Fact]
    public void CreatePaymentProvider_WithInvalidProvider_ThrowsException()
    {
        // Arrange & Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            _factory.CreatePaymentProvider("InvalidProvider"));
    }
}
```

### Integration Tests

**File:** `OneTicket.API.Tests/Controllers/PaymentControllerTests.cs`

```csharp
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using OneTicket.API.Controllers;
using OneTicket.API.Services.Payments.Abstractions;
using OneTicket.API.Services.Payments.Factory;
using Microsoft.Extensions.Logging;

namespace OneTicket.API.Tests.Controllers;

public class PaymentControllerTests
{
    private readonly Mock<IPaymentFactory> _mockPaymentFactory;
    private readonly Mock<ILogger<PaymentController>> _mockLogger;
    private readonly PaymentController _controller;

    public PaymentControllerTests()
    {
        _mockPaymentFactory = new Mock<IPaymentFactory>();
        _mockLogger = new Mock<ILogger<PaymentController>>();
        _controller = new PaymentController(_mockPaymentFactory.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task InitiatePayment_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new PaymentInitiateRequest
        {
            OrderId = "ORD-001",
            Amount = 100.00m,
            CustomerName = "John Doe",
            CustomerPhone = "+1234567890"
        };

        var mockProvider = new Mock<IPaymentProvider>();
        var expectedResponse = new PaymentInitiationResponse
        {
            Success = true,
            TransactionId = "ORD-001",
            ProviderTransactionId = "TXN-123456"
        };

        mockProvider
            .Setup(p => p.InitiatePaymentAsync(It.IsAny<PaymentRequest>()))
            .ReturnsAsync(expectedResponse);

        _mockPaymentFactory
            .Setup(f => f.GetDefaultProvider())
            .Returns(mockProvider.Object);

        // Act
        var result = await _controller.InitiatePayment(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task VerifyPayment_WithValidTransaction_ReturnsVerificationResponse()
    {
        // Arrange
        var transactionId = "TXN-123456";
        var mockProvider = new Mock<IPaymentProvider>();
        var expectedResponse = new PaymentVerificationResponse
        {
            TransactionId = transactionId,
            Status = "SUCCESS",
            Amount = 100.00m
        };

        mockProvider
            .Setup(p => p.VerifyPaymentAsync(transactionId))
            .ReturnsAsync(expectedResponse);

        _mockPaymentFactory
            .Setup(f => f.GetDefaultProvider())
            .Returns(mockProvider.Object);

        // Act
        var result = await _controller.VerifyPayment(transactionId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }
}
```

---

## Testing Instructions

### 1. Unit Test Execution

```bash
# Create test project
dotnet new xunit -n OneTicket.API.Tests

# Add reference to API project
cd OneTicket.API.Tests
dotnet add reference ../OneTicket.API/OneTicket.API.csproj

# Add Moq for mocking
dotnet add package Moq

# Run tests
dotnet test

# Run specific test
dotnet test --filter "PaymentFactoryTests"

# Run with verbose output
dotnet test -v n
```

### 2. Integration Test via REST API

Use the provided `.http` file or Postman:

**Initiate Payment:**
```http
POST http://localhost:5005/api/payment/initiate
Content-Type: application/json

{
  "orderId": "ORD-2026-001",
  "amount": 100.50,
  "currency": "USD",
  "description": "Ticket Purchase",
  "customerName": "John Doe",
  "customerEmail": "john@example.com",
  "customerPhone": "+254712345678",
  "returnUrl": "http://localhost:5173/success",
  "notificationUrl": "http://your-tunnel.ngrok-free.dev/api/webhooks/payment",
  "provider": "OneKhusa"
}
```

**Verify Payment:**
```http
GET http://localhost:5005/api/payment/verify/ORD-2026-001
```

**Get Available Providers:**
```http
GET http://localhost:5005/api/payment/providers
```

---

## Frontend Integration (React)

**File:** `OneTicket.UI/src/services/paymentApi.js`

```javascript
import axios from 'axios';

const API_BASE_URL = 'http://localhost:5005/api';

const paymentApi = {
  // Initiate payment
  initiatePayment: async (paymentData) => {
    try {
      const response = await axios.post(`${API_BASE_URL}/payment/initiate`, {
        orderId: paymentData.orderId,
        amount: paymentData.amount,
        currency: paymentData.currency || 'USD',
        description: paymentData.description,
        customerName: paymentData.customerName,
        customerEmail: paymentData.customerEmail,
        customerPhone: paymentData.customerPhone,
        returnUrl: `${window.location.origin}/success`,
        notificationUrl: paymentData.notificationUrl,
        provider: paymentData.provider || 'OneKhusa'
      });
      return response.data;
    } catch (error) {
      throw error.response?.data || error;
    }
  },

  // Verify payment status
  verifyPayment: async (transactionId, provider = 'OneKhusa') => {
    try {
      const response = await axios.get(
        `${API_BASE_URL}/payment/verify/${transactionId}`,
        { params: { provider } }
      );
      return response.data;
    } catch (error) {
      throw error.response?.data || error;
    }
  },

  // Get available providers
  getAvailableProviders: async () => {
    try {
      const response = await axios.get(`${API_BASE_URL}/payment/providers`);
      return response.data.providers;
    } catch (error) {
      throw error.response?.data || error;
    }
  },

  // Process refund
  refundPayment: async (transactionId, amount, provider = 'OneKhusa') => {
    try {
      const response = await axios.post(`${API_BASE_URL}/payment/refund`, {
        transactionId,
        amount,
        provider
      });
      return response.data;
    } catch (error) {
      throw error.response?.data || error;
    }
  }
};

export default paymentApi;
```

---

## Key Benefits Recap

✅ **Loose Coupling:** Controllers don't depend on concrete payment providers  
✅ **Easy to Extend:** Add new providers without modifying existing code  
✅ **Testable:** Mock providers easily in unit tests  
✅ **Maintainable:** Clear separation of concerns  
✅ **Scalable:** Handle multiple payment providers seamlessly  
✅ **SOLID Principles:** Follows Open/Closed, Dependency Inversion principles  

---

## Next Steps

1. Implement webhook handling in `WebhooksController`
2. Add database persistence for payment records
3. Implement retry logic and error handling
4. Add comprehensive logging and monitoring
5. Set up payment reconciliation jobs

---

**Happy Coding!** 🚀

