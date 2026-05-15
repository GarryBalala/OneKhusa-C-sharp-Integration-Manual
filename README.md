# OneKhusa SDK & API Master Integration Manual

> **Architecture Pattern:** Abstract Factory Pattern  
> A comprehensive technical reference for integrating the **OneKhusa SDK** and **Hosted Checkout API** into a .NET and React ecosystem using design patterns and best practices.

---

## 🏗️ Architecture Overview: Abstract Factory Pattern

This project implements the **Abstract Factory Pattern** to manage the creation of payment-related objects and services. The pattern ensures loose coupling, maintainability, and extensibility across the payment ecosystem.

### Factory Hierarchy

```
┌─────────────────────────────────────────────────────┐
│         IPaymentEcosystemFactory (Abstract)         │
│  Creates families of related payment objects        │
└──────────────────────┬──────────────────────────────┘
                       │
        ┌──────────────┼──────────────┐
        │              │              │
        ▼              ▼              ▼
  ┌──────────┐  ┌──────────┐  ┌──────────┐
  │Collections│  │Payouts   │  │WebhookSvc│
  │Factory    │  │Factory   │  │Factory   │
  └──────────┘  └──────────┘  └──────────┘
        │              │              │
        ▼              ▼              ▼
   Products:    Products:      Products:
   • Request    • Single        • EventParser
   • Handler    • Batch         • Handler
   • Response   • Handler       • Notifier
```

---

## 📁 Project Structure & Factory Mapping

### Root Level Structure

```
OneTicket-Ecosystem/
├── OneTicket.API/                  # ASP.NET Core Backend (Factory Implementation Layer)
│   ├── Controllers/                # Abstract Controllers → Concrete Implementations
│   ├── DTOs/                       # Data Transfer Objects (Factory Products)
│   ├── Services/                   # Concrete Factories & Business Logic
│   ├── Program.cs                  # Factory Registration & DI Container
│   ├── appsettings.json            # Factory Configuration
│   └── OneTicket.API.csproj        # Project Manifest
│
├── OneTicket.UI/                   # React Frontend (Client-Side Factory)
│   ├── src/
│   │   ├── components/             # UI Components (Factories for UI elements)
│   │   ├── services/               # API Service Factory
│   │   ├── App.jsx                 # Main Factory Orchestrator
│   │   └── main.jsx                # React Entry Point
│   ├── public/                     # Static Assets
│   ├── package.json                # Dependencies
│   └── vite.config.js              # Build Configuration
│
├── ABSTRACT_FACTORY_GUIDE.md       # Detailed Pattern Documentation
├── C#__Onekhusa_Integration_Manual.pdf # Integration Reference
└── OneTicket-Ecosystem.slnx        # Solution Configuration
```

---

## 🎯 Abstract Factory Pattern Implementation

### 1. **Core Factory Interface (Abstract Layer)**

The pattern is built on abstract factory interfaces that define contracts for creating families of related objects:

```csharp
// IPaymentEcosystemFactory.cs - Abstract Factory Contract
public interface IPaymentEcosystemFactory
{
    // Collections (Hosted Checkout) Factory Products
    ICollectionsRequestBuilder CreateCollectionsRequest();
    IPaymentHandler CreateCollectionsPaymentHandler();
    IResponseProcessor CreateResponseProcessor();

    // Payouts (Disbursements) Factory Products
    ISinglePayoutBuilder CreateSinglePayoutRequest();
    IBatchPayoutBuilder CreateBatchPayoutRequest();
    IPayoutHandler CreatePayoutHandler();

    // Webhook Management Factory Products
    IWebhookEventParser CreateEventParser();
    IWebhookHandler CreateWebhookHandler();
    INotificationService CreateNotificationService();
}
```

### 2. **Concrete Factories (Implementation Layer)**

#### **Collections Factory** - Handles Payment Collection
```
Location: OneTicket.API/Services/Factories/CollectionsPaymentFactory.cs

Products Created:
├─ CollectionsRequestBuilder      → Initiates hosted checkout sessions
├─ CollectionsPaymentHandler      → Processes payment logic
└─ CollectionsResponseProcessor   → Handles OneKhusa API responses
```

#### **Payouts Factory** - Handles Disbursements
```
Location: OneTicket.API/Services/Factories/PayoutsFactory.cs

Products Created:
├─ SinglePayoutBuilder            → Single transfer requests
├─ BatchPayoutBuilder             → Batch file processing (.csv, .xlsx)
└─ PayoutHandler                  → Executes fund transfers
```

#### **Webhook Factory** - Handles Asynchronous Events
```
Location: OneTicket.API/Services/Factories/WebhookFactory.cs

Products Created:
├─ WebhookEventParser             → Parses incoming webhook events
├─ WebhookHandler                 → Routes events to handlers
└─ NotificationService            → Sends back to OneKhusa API
```

### 3. **Data Transfer Objects (Factory Products)**

Located in: `OneTicket.API/DTOs/`

```
DTOs/
├─ Collections/
│  ├─ InitiateCollectionRequest.cs      # DTO for session initiation
│  ├─ CollectionResponse.cs              # Response wrapper
│  └─ PaymentRedirectModel.cs            # Redirection data
│
├─ Payouts/
│  ├─ SinglePayoutRequest.cs             # Single transfer DTO
│  ├─ BatchPayoutRequest.cs              # Batch upload DTO
│  └─ PayoutResponse.cs                  # Payout result wrapper
│
└─ Webhooks/
   ├─ OneKhusaWebhook.cs                 # Webhook payload model
   ├─ WebhookEventModel.cs               # Event envelope
   └─ CallbackResponse.cs                # Callback envelope
```

### 4. **Controllers (Factory Consumers)**

Located in: `OneTicket.API/Controllers/`

Each controller depends on factories injected via Dependency Injection:

```csharp
// TicketsController.cs - Collections Controller
public class TicketsController : ControllerBase
{
    private readonly IPaymentEcosystemFactory _factory;

    [HttpPost("initiate")]
    public async Task<IActionResult> InitiatePayment(InitiateCollectionRequest request)
    {
        // Factory creates payment handler
        var handler = _factory.CreateCollectionsPaymentHandler();
        return Ok(await handler.ProcessAsync(request));
    }
}

// PayoutsController.cs - Disbursements Controller
public class PayoutsController : ControllerBase
{
    private readonly IPaymentEcosystemFactory _factory;

    [HttpPost("single")]
    public async Task<IActionResult> SinglePayout(SinglePayoutRequest request)
    {
        // Factory creates payout builder & handler
        var builder = _factory.CreateSinglePayoutBuilder();
        var handler = _factory.CreatePayoutHandler();
        return Ok(await handler.ProcessAsync(builder.Build(request)));
    }
}

// WebhooksController.cs - Webhook Consumer
public class WebhooksController : ControllerBase
{
    private readonly IPaymentEcosystemFactory _factory;

    [HttpPost("webhook")]
    public async Task<IActionResult> HandleWebhook([FromBody] OneKhusaWebhook webhook)
    {
        // Factory creates event parser & handler
        var parser = _factory.CreateEventParser();
        var handler = _factory.CreateWebhookHandler();
        return Ok(await handler.ProcessAsync(parser.Parse(webhook)));
    }
}
```

### 5. **Service Layer (Factory Implementation)**

Located in: `OneTicket.API/Services/`

```
Services/
├─ Factories/
│  ├─ OneKhusaPaymentEcosystemFactory.cs    # Main factory implementation
│  ├─ CollectionsPaymentFactory.cs          # Collections products
│  ├─ PayoutsFactory.cs                     # Payout products
│  └─ WebhookFactory.cs                     # Webhook products
│
├─ Handlers/
│  ├─ CollectionsPaymentHandler.cs          # Collections business logic
│  ├─ PayoutHandler.cs                      # Payout business logic
│  └─ WebhookHandler.cs                     # Webhook processing
│
├─ Builders/
│  ├─ CollectionsRequestBuilder.cs          # Request construction
│  ├─ SinglePayoutBuilder.cs                # Single payout construction
│  └─ BatchPayoutBuilder.cs                 # Batch processing
│
├─ Parsers/
│  └─ WebhookEventParser.cs                 # Webhook event parsing
│
├─ Processors/
│  ├─ CollectionsResponseProcessor.cs       # Response handling
│  └─ NotificationService.cs                # OneKhusa notifications
│
└─ TicketTracker.cs                         # In-Memory State Management
```

---

## ⚡ Quick Developer Reference (Webhooks & Handshaking)

### 1. **Webhook Endpoint (Your API)**

This is the URL configured in the **OneKhusa Merchant Portal**:

- **Sample URL:** `https://your-tunnel-name.ngrok-free.dev/wc-api/onekhusa_webhook`
- **HTTP Method:** `POST`
- **Controller:** `WebhooksController.cs`
- **Factory Used:** `WebhookFactory`
- **Expected Success Code:** `S100`

**Request Flow:**
```
OneKhusa API
    ↓ [POST webhook]
WebhooksController
    ↓ [Factory creates parser & handler]
WebhookFactory (Creates EventParser + WebhookHandler)
    ↓ [Processes event]
TicketTracker / Database
    ↓ [Updates payment status]
NotificationService
    ↓ [Sends callback to OneKhusa]
OneKhusa API [Releases customer session]
```

### 2. **"Closing the Loop" Endpoint (OneKhusa API)**

After your Webhook receives a success signal, your backend **must** notify this URL:

- **URL:** `https://checkout.onekhusa.com/requestToPay/webhook`
- **Payload:** `{ "paymentTransactionId": "...", "status": "SUCCESS" }`
- **Handler:** `NotificationService` (Created by `WebhookFactory`)

### 3. **Redirection URLs**

These are the paths where OneKhusa redirects the user after payment:

- **Success:** `http://localhost:5173/success`
- **Failure:** `http://localhost:5173/failed`

---

## 🚀 Key Features

### 1. **Hosted Checkout (Collections)**
- **Factory:** `CollectionsPaymentFactory`
- **Pattern:** Three-Way Handshake
  - 1️⃣ Session Initiation (Client → API)
  - 2️⃣ User Redirection (API → OneKhusa Hosted Page)
  - 3️⃣ Async Webhook Synchronization (OneKhusa → API → UI)
- **Real-Time Updates:** `NotificationService` triggers UI refresh automatically

### 2. **Standard Disbursements (Payouts)**
- **Factory:** `PayoutsFactory`
- **Single Payouts:** Immediate, real-time fund transfers via `SinglePayoutBuilder`
- **Batch Processing:** High-volume transfers via `BatchPayoutBuilder` (.csv/.xlsx file upload)

### 3. **Webhook Management**
- **Factory:** `WebhookFactory`
- **Event Parsing:** `WebhookEventParser` extracts payment status
- **State Tracking:** `TicketTracker` maintains in-memory session state
- **Notification Loop:** `NotificationService` completes the handshake

---

## 🛠️ Tech Stack

| Layer | Technology | Role |
|-------|-----------|------|
| **Backend Runtime** | ASP.NET Core 8.0 | Web API Host |
| **Factory Container** | Microsoft.Extensions.DependencyInjection | Factory Registration & Lifetime |
| **HTTP Client** | HttpClient | Direct API Handshaking with OneKhusa |
| **Webhook Listener** | Ngrok | Local Tunnel for Testing |
| **Frontend Framework** | React 18 (Vite) | UI Layer |
| **Frontend Styling** | Tailwind CSS | Component Styling |
| **Frontend Icons** | Lucide React | Icon Library |
| **Payment SDK** | OneKhusa .NET SDK | Official Integration |

---

## ⚙️ Setup & Installation

### 1. **Backend Configuration**

#### Step 1: Update `appsettings.json`

```json
{
  "OneKhusa": {
    "OrganizationId": "YOUR_ORG_ID",
    "MerchantAccountNumber": 79619974,
    "ApiKey": "YOUR_API_KEY",
    "ApiSecret": "YOUR_API_SECRET"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

#### Step 2: Register Factories in `Program.cs`

```csharp
// Program.cs - Factory Registration via DI Container
var builder = WebApplication.CreateBuilder(args);

// Register Abstract Factory Interface
builder.Services.AddScoped<IPaymentEcosystemFactory, OneKhusaPaymentEcosystemFactory>();

// Register Sub-Factories (Optional - for fine-grained control)
builder.Services.AddScoped<CollectionsPaymentFactory>();
builder.Services.AddScoped<PayoutsFactory>();
builder.Services.AddScoped<WebhookFactory>();

// Register Services
builder.Services.AddScoped<TicketTracker>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddControllers();
builder.Services.AddCors(options => options.AddPolicy("AllowReact",
    builder => builder
        .WithOrigins("http://localhost:5173")
        .AllowAnyMethod()
        .AllowAnyHeader()
));

var app = builder.Build();
app.UseCors("AllowReact");
app.MapControllers();
app.Run("http://localhost:5005");
```

#### Step 3: Run the API

```bash
cd OneTicket.API
dotnet restore
dotnet run
```

The API will start at: `http://localhost:5005`

### 2. **Frontend Configuration**

#### Step 1: Install Dependencies

```bash
cd OneTicket.UI
npm install
```

#### Step 2: Configure API Service Factory

Update `src/services/api.js` with your backend URL:

```javascript
// src/services/api.js - API Service Factory
const API_BASE_URL = 'http://localhost:5005/api';

export const apiServiceFactory = {
  collections: {
    initiate: (payload) => 
      axios.post(`${API_BASE_URL}/tickets/initiate`, payload),
    getStatus: (ticketId) =>
      axios.get(`${API_BASE_URL}/tickets/${ticketId}/status`),
  },
  
  payouts: {
    single: (payload) =>
      axios.post(`${API_BASE_URL}/payouts/single`, payload),
    batch: (formData) =>
      axios.post(`${API_BASE_URL}/payouts/batch`, formData),
  }
};
```

#### Step 3: Run Development Server

```bash
npm run dev
```

The UI will start at: `http://localhost:5173`

### 3. **Webhook Testing with Ngrok**

#### Step 1: Install & Start Ngrok

```bash
# Install ngrok (if not already installed)
brew install ngrok  # macOS
# or
choco install ngrok # Windows

# Start tunnel to your local API
ngrok http 5005
```

This outputs:
```
Session Status                online
Forwarding                    https://your-unique-code.ngrok-free.dev -> http://localhost:5005
```

#### Step 2: Update Webhook URL in Code

In `OneTicket.API/Controllers/TicketsController.cs`, update the `callbackApiUrl`:

```csharp
var initRequest = new InitiateCollectionRequest
{
    Amount = request.Amount,
    PhoneNumber = request.PhoneNumber,
    // Update with your Ngrok URL
    CallbackApiUrl = "https://your-unique-code.ngrok-free.dev/wc-api/onekhusa_webhook"
};
```

#### Step 3: Update Merchant Portal

Log into your **OneKhusa Merchant Portal** and set:
- **Webhook URL:** `https://your-unique-code.ngrok-free.dev/wc-api/onekhusa_webhook`

---

## 🔄 Payment Flow Diagram

### Collections (Hosted Checkout) Flow

```
┌────────────┐
│   User     │
└─────┬──────┘
      │ 1. Click "Pay"
      ▼
┌────────────────────────────────────────────────────┐
│ React UI (OneTicket.UI)                            │
│ Calls: apiServiceFactory.collections.initiate()  │
└─────┬──────────────────────────────────────────────┘
      │ 2. POST /api/tickets/initiate
      ▼
┌────────────────────────────────────────────────────┐
│ ASP.NET API (OneTicket.API)                        │
│ TicketsController.InitiatePayment()               │
│ Factory: CollectionsPaymentFactory                 │
└─────┬──────────────────────────────────────────────┘
      │ 3. Call OneKhusa Checkout API
      ▼
┌────────────────────────────────────────────────────┐
│ OneKhusa Hosted Checkout Page                      │
│ User enters phone number & completes payment      │
└─────┬──────────────────────────────────────────────┘
      │ 4. Payment success/failure
      ▼
┌────────────────────────────────────────────────────┐
│ WebhooksController.HandleWebhook()                │
│ Factory: WebhookFactory                            │
│ Parses event & updates TicketTracker              │
└─────┬──────────────────────────────────────────────┘
      │ 5. POST /webhook (notify OneKhusa)
      ▼
┌────────────────────────────────────────────────────┐
│ OneKhusa API                                       │
│ Releases customer session                          │
└────────────────────────────────────────────────────┘
      │ 6. Redirect to success/failed page
      ▼
┌────────────────────────────────────────────────────┐
│ React UI Success/Failed Page                      │
│ User sees payment confirmation                    │
└────────────────────────────────────────────────────┘
```

### Payouts (Disbursements) Flow

```
┌────────────┐
│  Backend   │
│  (Admin)   │
└─────┬──────┘
      │ 1. POST /api/payouts/single
      ▼
┌────────────────────────────────────────────────────┐
│ PayoutsController.SinglePayout()                  │
│ Factory: PayoutsFactory                            │
│ Builder: SinglePayoutBuilder                       │
└─────┬──────────────────────────────────────────────┘
      │ 2. Execute transfer via OneKhusa API
      ▼
┌────────────────────────────────────────────────────┐
│ OneKhusa API                                       │
│ Processes fund transfer                            │
└─────┬──────────────────────────────────────────────┘
      │ 3. Return payout status
      ▼
┌────────────────────────────────────────────────────┐
│ Backend receives confirmation                      │
│ Updates database / logs transfer                  │
└────────────────────────────────────────────────────┘
```

---

## 📊 In-Memory State Management (TicketTracker)

The `TicketTracker` service maintains temporary session state:

```csharp
// OneTicket.API/Services/TicketTracker.cs
public class TicketTracker
{
    private static readonly Dictionary<string, PaymentSessionModel> _sessions = new();

    public void CreateSession(string ticketId, PaymentSessionModel session)
        => _sessions[ticketId] = session;

    public PaymentSessionModel GetSession(string ticketId)
        => _sessions.ContainsKey(ticketId) ? _sessions[ticketId] : null;

    public void UpdateSessionStatus(string ticketId, string status)
        => _sessions[ticketId].Status = status;
}
```

**Note:** For production, replace with persistent storage (SQL Server, PostgreSQL, Redis).

---

## 🧪 Testing the Integration

### Test Collections (Hosted Checkout)

```bash
# 1. Start both services
# Terminal 1: Backend
cd OneTicket.API && dotnet run

# Terminal 2: Frontend
cd OneTicket.UI && npm run dev

# 2. Navigate to http://localhost:5173
# 3. Click "Pay"
# 4. Use test credentials from OneKhusa
```

### Test Webhook Delivery

```bash
# Use ngrok dashboard to inspect webhook calls
# URL: http://localhost:4040

# Or send manual test webhook:
curl -X POST https://your-tunnel.ngrok-free.dev/wc-api/onekhusa_webhook \
  -H "Content-Type: application/json" \
  -d '{
    "paymentTransactionId": "test-123",
    "status": "SUCCESS",
    "amount": 100
  }'
```

---

## 📚 Additional Resources

- **[ABSTRACT_FACTORY_GUIDE.md](./ABSTRACT_FACTORY_GUIDE.md)** - Deep dive into factory pattern implementation
- **[C# Integration Manual PDF](./C%23__Onekhusa_Integration_Manual.pdf)** - Official OneKhusa documentation
- **[OneKhusa Developer Docs](https://developer.onekhusa.com)** - Official API reference

---

## 🔐 Security Best Practices

1. **Store Credentials:** Never hardcode API keys; use `appsettings.json` with user secrets
2. **HTTPS Only:** Always use HTTPS in production
3. **CORS Configuration:** Restrict to your frontend domain
4. **Webhook Validation:** Verify webhook signatures from OneKhusa
5. **Rate Limiting:** Implement rate limits on sensitive endpoints
6. **Logging:** Log all payment transactions for audit trails

---

## 📝 License

This integration manual is provided for OneKhusa SDK implementation purposes.

---

## 🤝 Support

For issues or questions:
- Check [ABSTRACT_FACTORY_GUIDE.md](./ABSTRACT_FACTORY_GUIDE.md) for detailed implementation patterns
- Review the PDF manual for OneKhusa API specifications
- Contact OneKhusa support at [support@onekhusa.com](mailto:support@onekhusa.com)

---

**Last Updated:** May 2026  
**Repository:** [GarryBalala/OneKhusa-C-sharp-Integration-Manual](https://github.com/GarryBalala/OneKhusa-C-sharp-Integration-Manual)
