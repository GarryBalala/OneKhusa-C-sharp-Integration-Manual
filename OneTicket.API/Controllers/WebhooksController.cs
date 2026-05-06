using Microsoft.AspNetCore.Mvc;
using OneTicket.API.DTOs;
using OneTicket.API.Services;

namespace OneTicket.API.Controllers;

[ApiController]
[Route("wc-api")] // Matching the path seen in your Ngrok terminal
public class WebhooksController : ControllerBase
{
    private readonly TicketTracker _tracker;
    private readonly IHttpClientFactory _httpClientFactory;

    public WebhooksController(TicketTracker tracker, IHttpClientFactory httpClientFactory)
    {
        _tracker = tracker;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("onekhusa_webhook")]
    public async Task<IActionResult> HandleWebhook([FromBody] OneKhusaWebhook payload)
    {
        var myRef = payload.MetaData.ReferenceNumber;

        // S100 is the OneKhusa success code
        if (payload.ResponseCode == "S100")
        {
            // 1. Update the Internal RAM Tracker (for your React UI)
            _tracker.UpdateStatus(myRef, "Paid");
            Console.WriteLine($"[WEBHOOK] Status updated for {myRef}.");

            // 2. "CLOSE THE LOOP": Notify OneKhusa Hosted Page to redirect the user
            // This is what makes the Checkout Page turn green and send the user back to you.
            try
            {
                var notifyPayload = new
                {
                    paymentTransactionId = payload.PaymentTransactionId,
                    status = "SUCCESS",
                    responseCode = "S100"
                };

                var client = _httpClientFactory.CreateClient();
                // Send the confirmation to the OneKhusa checkout engine
                var response = await client.PostAsJsonAsync("https://checkout.onekhusa.com/requestToPay/webhook", notifyPayload);

                if (response.IsSuccessStatusCode)
                    Console.WriteLine($"[WEBHOOK] Hosted Page notified successfully for session {payload.PaymentTransactionId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WEBHOOK ERROR] Failed to notify Hosted Page: {ex.Message}");
            }
        }

        return Ok(); // Always return 200 OK to OneKhusa immediately
    }
}