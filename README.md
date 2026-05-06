# OneKhusa SDK & API Master Integration Manual

This repository is a comprehensive technical reference for integrating the **OneKhusa SDK** and **Hosted Checkout API** into a .NET and React ecosystem. It covers the standard implementation for high-fidelity financial collections and disbursements.

---

## ⚡ Quick Developer Reference (Webhooks & Handshaking)

If you are setting up the project for the first time, these are the critical communication points:

### 1. Webhook Endpoint (Your API)
This is the URL you must configure in the **OneKhusa Merchant Portal** or send in the `callbackApiUrl` field during initiation.
- **Sample URL:** `https://your-tunnel-name.ngrok-free.dev/wc-api/onekhusa_webhook`
- **Method:** `POST`
- **Controller:** `WebhooksController.cs`
- **Expected Success Code:** `S100`

### 2. "Closing the Loop" Endpoint (OneKhusa API)
After your Webhook receives a success signal, your backend **must** notify this URL to release the customer's browser session.
- **URL:** `https://checkout.onekhusa.com/requestToPay/webhook`
- **Payload:** `{ "paymentTransactionId": "...", "status": "SUCCESS" }`

### 3. Redirection URLs
These are the paths where OneKhusa will send the user *after* they finish the payment.
- **Success:** `http://localhost:5173/success`
- **Failure:** `http://localhost:5173/failed`

---

## 🚀 Key Features

### 1. Hosted Checkout (Collections)
- **Three-Way Handshake:** Implements secure session initiation, user redirection, and asynchronous webhook synchronization.
- **Real-Time UI Updates:** The backend automatically notifies the OneKhusa hosted page upon payment confirmation to trigger the "Success" screen and automatic redirection.

### 2. Standard Disbursements (Payouts)
- **Single Payouts:** pass-through logic for immediate, real-time fund transfers.
- **Batch Processing:** High-volume transfers via raw file upload (.csv and .xlsx).

---

## 🛠️ Tech Stack
- **Backend:** ASP.NET Core 8.0 (Web API)
- **Frontend:** React (Vite, Tailwind CSS, Lucide Icons)
- **SDK:** Official OneKhusa .NET SDK
- **Communication:** HttpClient (Direct API Handshaking)
- **Tunneling:** Ngrok (for Webhook listener)

---

## 📁 Project Structure
```text
OneTicket-Ecosystem/
|-- OneTicket.API/           # ASP.NET Core Backend
|   |-- Controllers/         # Disbursements, Tickets, and Webhooks
|   |-- DTOs/                # OneKhusaWebhook, SinglePayoutRequest
|   |-- Services/            # TicketTracker (In-Memory State)
|   +-- appsettings.json     # Configuration & Merchant Credentials
|-- OneTicket.UI/            # React Frontend
|   |-- src/App.jsx          # Redirection & Polling Logic
|   +-- src/services/api.js  # Axios Service
+-- docs/                    # Technical Manual (LaTeX Source)
⚙️ Setup & Installation
1. Backend Configuration
Update appsettings.json with your credentials:
code
JSON
{
  "OneKhusa": {
    "OrganizationId": "YOUR_ORG_ID",
    "MerchantAccountNumber": 79619974,
    "ApiKey": "YOUR_API_KEY",
    "ApiSecret": "YOUR_API_SECRET"
  }
}
Run the API:
code
Bash
dotnet run
2. Frontend Configuration
code
Bash
npm install
npm run dev
3. Webhook Testing (Ngrok)
Expose your local server to the internet:
code
Bash
ngrok http 5005
Important: Update the callbackApiUrl in TicketsController.cs to match the URL provided by Ngrok.
