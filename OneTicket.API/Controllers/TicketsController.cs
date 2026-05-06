using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using OneKhusa.SDK;
using OneTicket.API.Services;
using OneTicket.API.DTOs;

namespace OneTicket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly IOneKhusaClient _oneKhusa;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;

    public TicketsController(IOneKhusaClient oneKhusa, IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _oneKhusa = oneKhusa;
        _config = config;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("buy/{eventId}")]
    public async Task<IActionResult> BuyTicket(string eventId, [FromQuery] string email)
    {
        var apiKey = _config["OneKhusa:ApiKey"];
        var apiSecret = _config["OneKhusa:ApiSecret"];
        var orgId = _config["OneKhusa:OrganizationId"];
        int.TryParse(_config["OneKhusa:MerchantAccountNumber"], out int merchantAcc);

        string sourceRef = "OT" + DateTime.Now.Ticks.ToString().Substring(10);

        // 1. Prepare Payload
        var payload = new
        {
            authentication = new { apiKey, apiSecret },
            merchant = new { organisationId = orgId, merchantAccountNumber = merchantAcc },
            payment = new
            {
                sourceReferenceNumber = sourceRef,
                description = "Ticket Purchase",
                amount = 2500.00,
                capturedBy = email
            },
            route = new
            {
                successRedirectionUrl = "http://localhost:5173/success",
                failureRedirectionUrl = "http://localhost:5173/failed",
                callbackApiUrl = "https://zena-unjudgeable-renita.ngrok-free.dev/wc-api/onekhusa_webhook"
            }
        };

        try
        {
            var client = _httpClientFactory.CreateClient();

            // 2. Create the Request Message manually to add headers
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.onekhusa.com/sandbox/v1/checkout/rtp/initiate");

            // ADD THE MISSING HEADER (Must be 15-80 characters)
            string idempotencyKey = "INIT-HOSTED-CHECKOUT-" + Guid.NewGuid().ToString();
            request.Headers.Add("X-Idempotency-Key", idempotencyKey);

            // Add Content
            request.Content = JsonContent.Create(payload);

            // 3. Send Request
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                string ptid = result.GetProperty("paymentTransactionId").GetString();

                return Ok(new
                {
                    checkoutUrl = $"https://checkout.onekhusa.com/requestToPay/initiate?ptid={ptid}&rn={sourceRef}",
                    reference = sourceRef
                });
            }

            // Diagnostic logging
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[OneKhusa Error] Body: {errorContent}");

            return BadRequest(new { details = "OneKhusa rejected request", error = errorContent });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { details = ex.Message });
        }
    }

    [HttpGet("status/{reference}")]
    public IActionResult GetStatus(string reference, [FromServices] TicketTracker tracker)
    {
        return Ok(new { status = tracker.GetStatus(reference) });
    }
}