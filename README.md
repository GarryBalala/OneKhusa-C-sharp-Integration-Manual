# OneKhusa SDK & API Master Integration Manual

> **Architecture Pattern:** Abstract Factory Pattern  
> A comprehensive technical reference for integrating the **OneKhusa SDK** and **Hosted Checkout API** into a .NET and React ecosystem using design patterns and best practices.

---

## 🏗️ Architecture Overview:

This project implements the **Abstract Factory Pattern** to manage the creation of payment-related objects and services. The pattern ensures loose coupling, maintainability, and extensibility across the entire payment ecosystem.


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

- **[C# Integration Manual PDF](./C%23__Onekhusa_Integration_Manual.pdf)** - Official OneKhusa documentation
- **[OneKhusa Developer Docs](https://developer.onekhusa.com)** - Official API reference

---


