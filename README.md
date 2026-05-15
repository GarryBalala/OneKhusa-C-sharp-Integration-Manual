# OneKhusa SDK & API Master Integration Manual

> **Architecture Pattern:** Abstract Factory Pattern  
> A comprehensive technical reference for integrating the **OneKhusa SDK** and **Hosted Checkout API** into a .NET and React ecosystem using design patterns and best practices.

---

## 🏗️ Architecture Overview: Abstract Factory Pattern

This project implements the **Abstract Factory Pattern** to manage the creation of payment-related objects and services. The pattern ensures loose coupling, maintainability, and extensibility across the entire payment ecosystem.

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

## 📁 Actual Project Structure & Folder Organization

### Root Level Structure

```
OneKhusa-C-sharp-Integration-Manual/
├── OneTicket.API/                      # ASP.NET Core Backend
│   ├── Controllers/                    # API Endpoints (Collections, Payouts, Webhooks)
│   ├── DTOs/                           # Data Transfer Objects
│   │   ├── Collections/                # Collection request/response DTOs
│   │   ├── Payouts/                    # Payout request/response DTOs
│   │   └── Webhooks/                   # Webhook event DTOs
│   ├── Services/                       # Business Logic Layer (to be created)
│   │   ├── Factories/                  # Factory implementations (to create)
│   │   ├── Handlers/                   # Request/Response handlers (to create)
│   │   ├── Builders/                   # Request builders (to create)
│   │   ├── Parsers/                    # Event parsers (to create)
│   │   └── TicketTracker.cs            # In-memory state management (to create)
│   ├── Properties/                     # Project properties
│   ├── Program.cs                      # DI Container & Factory Registration
│   ├── appsettings.json                # Configuration & API Keys
│   ├── OneTicket.API.csproj            # Project manifest
│   ├── OneTicket.API.http              # REST Client tests
│   └── wwwroot/                        # Static files (optional)
│
├── OneTicket.UI/                       # React Frontend (Vite)
│   ├── src/
│   │   ├── components/                 # React Components
│   │   ├── services/                   # API Service Factory (to create)
│   │   ├── pages/                      # Page components (to create)
│   │   ├── App.jsx                     # Main component
│   │   ├── main.jsx                    # React entry point
│   │   └── styles/                     # Global styles (optional)
│   ├── public/                         # Static assets
│   ├── package.json                    # NPM dependencies
│   ├── package-lock.json               # Dependency lock file
│   ├── vite.config.js                  # Vite build config
│   ├── tailwind.config.js              # Tailwind CSS config
│   ├── postcss.config.js               # PostCSS config
│   ├── eslint.config.js                # ESLint config
│   ├── index.html                      # HTML entry point
│   └── .gitignore                      # Git ignore rules
│
├── README.md                           # This file
├── ABSTRACT_FACTORY_GUIDE.md           # Detailed pattern documentation
├── C#__Onekhusa_Integration_Manual.pdf # Official OneKhusa documentation
├── OneTicket-Ecosystem.slnx            # Visual Studio solution file
└── .gitignore                          # Root .gitignore
```

---

## ⚙️ Setup & Installation (START HERE!)

### Prerequisites

Before you begin, ensure you have:

- **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download)
- **Node.js 18+** - [Download here](https://nodejs.org/)
- **Visual Studio 2022** or **VS Code** with C# extension
- **Ngrok** (for local webhook testing)
- **OneKhusa Account** with API credentials

### 1️⃣ Backend Setup (ASP.NET Core API)

#### Step 1: Navigate to Backend Directory

```bash
cd OneTicket.API
```

#### Step 2: Restore NuGet Packages

```bash
dotnet restore
```

#### Step 3: Update Configuration

Edit `appsettings.json` and add your OneKhusa API credentials:

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
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

**Where to find these credentials:**
- Log into **OneKhusa Merchant Portal**
- Navigate to **Settings → API Keys**
- Copy your **API Key** and **API Secret**

#### Step 4: Build the Project

```bash
dotnet build
```

Expected output: `Build succeeded`

#### Step 5: Run the API

```bash
dotnet run
```

Expected output:
```
Building...
info: Microsoft.AspNetCore.Hosting.Diagnostics
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime
      Now listening on: http://localhost:5005
```

✅ **Backend is running at:** `http://localhost:5005`

---

### 2️⃣ Frontend Setup (React with Vite)

#### Step 1: Navigate to Frontend Directory

```bash
cd ../OneTicket.UI
```

#### Step 2: Install Dependencies

```bash
npm install
```

Expected output: `added X packages`

#### Step 3: Configure Backend URL

Create a `.env` file (or update if exists):

```env
VITE_API_BASE_URL=http://localhost:5005/api
```

Or edit `src/services/api.js` with:

```javascript
const API_BASE_URL = 'http://localhost:5005/api';
```

#### Step 4: Run Development Server

```bash
npm run dev
```

Expected output:
```
  VITE v5.0.0  ready in XXX ms
  ➜  Local:   http://localhost:5173/
  ➜  press h to show help
```

✅ **Frontend is running at:** `http://localhost:5173`

---

### 3️⃣ Webhook Testing with Ngrok

#### Step 1: Install Ngrok

```bash
# macOS (using Homebrew)
brew install ngrok

# Windows (using Chocolatey)
choco install ngrok

# Or download from: https://ngrok.com/download
```

#### Step 2: Authenticate Ngrok (One-time)

```bash
ngrok config add-authtoken YOUR_NGROK_TOKEN
```

Get your token from: https://dashboard.ngrok.com/auth

#### Step 3: Start Ngrok Tunnel

```bash
ngrok http 5005
```

Expected output:
```
Forwarding                    https://abc123def456.ngrok-free.dev -> http://localhost:5005
```

Copy your **Ngrok URL** (e.g., `https://abc123def456.ngrok-free.dev`)

#### Step 4: Update Webhook URL in OneKhusa Portal

1. Log into **OneKhusa Merchant Portal**
2. Go to **Settings → Webhook Configuration**
3. Set **Webhook URL** to:
   ```
   https://your-ngrok-url.ngrok-free.dev/api/webhooks/handle
   ```
4. Save

---

## ✅ Quick Verification Checklist

After completing all three installations:

```bash
# ✓ Backend running
curl http://localhost:5005/health

# ✓ Frontend accessible
open http://localhost:5173

# ✓ Ngrok tunnel active
curl https://your-ngrok-url.ngrok-free.dev/health
```

---

## 🎯 Abstract Factory Pattern Implementation

### 1. **Core Factory Interface (Abstract Layer)**

The pattern is built on an abstract factory interface that defines contracts for creating families of related objects.

Create: `OneTicket.API/Services/Factories/IPaymentEcosystemFactory.cs`

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

### 2. **Main Factory Implementation**

Create: `OneTicket.API/Services/Factories/OneKhusaPaymentEcosystemFactory.cs`

```csharp
// OneKhusaPaymentEcosystemFactory.cs - Concrete Factory
public class OneKhusaPaymentEcosystemFactory : IPaymentEcosystemFactory
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public OneKhusaPaymentEcosystemFactory(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }

    // Collections Products
    public ICollectionsRequestBuilder CreateCollectionsRequest() 
        => new CollectionsRequestBuilder(_configuration);

    public IPaymentHandler CreateCollectionsPaymentHandler() 
        => new CollectionsPaymentHandler(_httpClient, _configuration);

    public IResponseProcessor CreateResponseProcessor() 
        => new CollectionsResponseProcessor();

    // Payouts Products
    public ISinglePayoutBuilder CreateSinglePayoutRequest() 
        => new SinglePayoutBuilder();

    public IBatchPayoutBuilder CreateBatchPayoutRequest() 
        => new BatchPayoutBuilder();

    public IPayoutHandler CreatePayoutHandler() 
        => new PayoutHandler(_httpClient, _configuration);

    // Webhook Products
    public IWebhookEventParser CreateEventParser() 
        => new WebhookEventParser();

    public IWebhookHandler CreateWebhookHandler() 
        => new WebhookHandler();

    public INotificationService CreateNotificationService() 
        => new NotificationService(_httpClient, _configuration);
}
```

### 3. **Service Layer Structure**

Your `OneTicket.API/Services/` directory should contain:

```
Services/
├── Factories/
│   ├── IPaymentEcosystemFactory.cs         # Abstract interface
│   ├── OneKhusaPaymentEcosystemFactory.cs  # Main factory
│   ├── ICollectionsRequestBuilder.cs       # Collections builder interface
│   ├── ISinglePayoutBuilder.cs             # Single payout builder interface
│   ├── IBatchPayoutBuilder.cs              # Batch payout builder interface
│   ├── IPaymentHandler.cs                  # Handler interface
│   ├── IResponseProcessor.cs               # Response processor interface
│   ├── IWebhookEventParser.cs              # Parser interface
│   ├── IWebhookHandler.cs                  # Webhook handler interface
│   └── INotificationService.cs             # Notification interface
│
├── Handlers/
│   ├── CollectionsPaymentHandler.cs        # Collections business logic
│   ├── PayoutHandler.cs                    # Payout business logic
│   └── WebhookHandler.cs                   # Webhook processing
│
├── Builders/
│   ├── CollectionsRequestBuilder.cs        # Request construction
│   ├── SinglePayoutBuilder.cs              # Single payout construction
│   └── BatchPayoutBuilder.cs               # Batch processing
│
├── Parsers/
│   └── WebhookEventParser.cs               # Webhook event parsing
│
├── Processors/
│   ├── CollectionsResponseProcessor.cs     # Response handling
│   └── NotificationService.cs              # OneKhusa notifications
│
└── TicketTracker.cs                        # In-Memory State Management
```

### 4. **Register Factory in Program.cs**

Update `OneTicket.API/Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register Abstract Factory
builder.Services.AddScoped<IPaymentEcosystemFactory, OneKhusaPaymentEcosystemFactory>();

// Register Services
builder.Services.AddScoped<TicketTracker>();
builder.Services.AddHttpClient();

// Add CORS
builder.Services.AddCors(options => options.AddPolicy("AllowReact",
    b => b.WithOrigins("http://localhost:5173")
          .AllowAnyMethod()
          .AllowAnyHeader()
));

builder.Services.AddControllers();

var app = builder.Build();
app.UseCors("AllowReact");
app.MapControllers();
app.Run("http://localhost:5005");
```

---

## 🔄 Implementation Flow: Collections (Hosted Checkout)

### How the Factory Pattern Works

```
1. Client Request (React UI)
   ↓
2. TicketsController receives request
   ↓
3. Controller uses IPaymentEcosystemFactory to create products:
   - CreateCollectionsRequest() → CollectionsRequestBuilder
   - CreateCollectionsPaymentHandler() → CollectionsPaymentHandler
   ↓
4. Builder constructs the request payload
   ↓
5. Handler calls OneKhusa API with payload
   ↓
6. OneKhusa returns hosted checkout URL
   ↓
7. User redirected to OneKhusa payment page
   ↓
8. After payment, OneKhusa sends webhook to your endpoint
   ↓
9. WebhooksController uses factory to create:
   - CreateEventParser() → WebhookEventParser
   - CreateWebhookHandler() → WebhookHandler
   ↓
10. Parser extracts payment status
    ↓
11. Handler updates TicketTracker
    ↓
12. NotificationService notifies OneKhusa
    ↓
13. User is redirected to success/failed page
```

---

## 📊 Payment Flow Diagram

### Collections (Hosted Checkout) Flow

```
┌────────────┐
│   User     │
└─────┬──────┘
      │ 1. Click "Pay"
      ▼
┌────────────────────────────────────────────────────┐
│ React UI (OneTicket.UI)                            │
│ POST /api/tickets/initiate                        │
└─────┬──────────────────────────────────────────────┘
      │
      ▼
┌────────────────────────────────────────────────────┐
│ ASP.NET API (OneTicket.API)                        │
│ TicketsController.InitiatePayment()               │
│ Factory creates: Builder + Handler                 │
└─────┬──────────────────────────────────────────────┘
      │
      ▼
┌────────────────────────────────────────────────────┐
│ OneKhusa Hosted Checkout Page                      │
│ User enters phone & completes payment             │
└─────┬──────────────────────────────────────────────┘
      │
      ▼
┌────────────────────────────────────────────────────┐
│ WebhooksController.HandleWebhook()                │
│ Factory creates: Parser + Handler                  │
│ Updates TicketTracker                              │
└─────┬──────────────────────────────────────────────┘
      │
      ▼
┌────────────────────────────────────────────────────┐
│ NotificationService (created by factory)          │
│ POST to OneKhusa webhook endpoint                 │
└─────┬──────────────────────────────────────────────┘
      │
      ▼
┌────────────────────────────────────────────────────┐
│ React UI Success/Failed Page                      │
│ User sees payment confirmation                    │
└────────────────────────────────────────────────────┘
```

---

## 🛠️ Tech Stack

| Layer | Technology | Role |
|-------|-----------|------|
| **Backend Runtime** | ASP.NET Core 8.0 | Web API Host |
| **Factory Container** | Microsoft.Extensions.DependencyInjection | DI + Factory Registration |
| **HTTP Client** | HttpClient | OneKhusa API calls |
| **Webhook Tunnel** | Ngrok | Local testing |
| **Frontend Framework** | React 18 (Vite) | UI Layer |
| **Frontend Styling** | Tailwind CSS | Component Styling |
| **Frontend Icons** | Lucide React | Icon Library |
| **Payment SDK** | OneKhusa .NET SDK | Official Integration |

---

## 🚀 Key Features (To Implement)

### 1. **Hosted Checkout (Collections)**
- **Factory:** `CollectionsPaymentFactory`
- **Flow:** Session → Hosted Page → Webhook → Confirmation
- **Handler:** `CollectionsPaymentHandler`

### 2. **Disbursements (Payouts)**
- **Factory:** `PayoutsFactory`
- **Single:** Real-time transfers
- **Batch:** CSV/Excel file processing

### 3. **Webhook Management**
- **Factory:** `WebhookFactory`
- **Parser:** Extracts events from OneKhusa
- **Notifier:** Confirms receipt to OneKhusa

---

## 🧪 Testing the Integration

### Test Endpoint Health

```bash
# Backend health check
curl http://localhost:5005/health

# Frontend is accessible
open http://localhost:5173
```

### Test Collections Flow

1. Start backend: `dotnet run` (in `OneTicket.API`)
2. Start frontend: `npm run dev` (in `OneTicket.UI`)
3. Open http://localhost:5173
4. Click "Pay" button
5. Enter test phone number from OneKhusa docs
6. Complete payment
7. Check webhook logs in Ngrok dashboard: http://localhost:4040

---

## 📚 Additional Resources

- **[ABSTRACT_FACTORY_GUIDE.md](./ABSTRACT_FACTORY_GUIDE.md)** - Deep dive into factory pattern
- **[C# Integration Manual PDF](./C%23__Onekhusa_Integration_Manual.pdf)** - Official OneKhusa documentation
- **[OneKhusa Developer Docs](https://developer.onekhusa.com)** - Official API reference

---

## 🔐 Security Best Practices

1. **Never hardcode credentials** - Use `appsettings.json` + user secrets
2. **HTTPS only in production** - HTTP only for local development
3. **CORS restriction** - Only allow your frontend domain
4. **Webhook validation** - Verify OneKhusa signatures
5. **Rate limiting** - Protect sensitive endpoints
6. **Logging** - Audit all payment transactions

---

## 📝 License

This integration manual is provided for OneKhusa SDK implementation purposes.

---

## 🤝 Support

For issues:
- Review [ABSTRACT_FACTORY_GUIDE.md](./ABSTRACT_FACTORY_GUIDE.md)
- Check official [OneKhusa API docs](https://developer.onekhusa.com)
- Contact support@onekhusa.com

---

**Last Updated:** May 2026  
**Repository:** [GarryBalala/OneKhusa-C-sharp-Integration-Manual](https://github.com/GarryBalala/OneKhusa-C-sharp-Integration-Manual)
