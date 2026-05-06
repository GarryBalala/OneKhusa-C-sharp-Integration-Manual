using Microsoft.AspNetCore.Mvc;
using OneKhusa.SDK;
using OneKhusa.SDK.Models.Transactions.SingleDisbursements;
using OneKhusa.SDK.Models.Transactions.BatchDisbursements;
using OneTicket.API.DTOs;

namespace OneTicket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DisbursementsController : ControllerBase
{
    private readonly IOneKhusaClient _oneKhusa;
    private readonly IConfiguration _config;

    public DisbursementsController(IOneKhusaClient oneKhusa, IConfiguration config)
    {
        _oneKhusa = oneKhusa;
        _config = config;
    }

    [HttpPost("single")]
    public async Task<IActionResult> CreateSinglePayout([FromBody] SinglePayoutRequest request)
    {
        int.TryParse(_config["OneKhusa:MerchantAccountNumber"], out int merchantAcc);
        var portalEmail = _config["OneKhusa:PortalEmail"] ?? "balalagarry@gmail.com";

        var sdkRequest = new CreatePayoutRequest
        {
            MerchantAccountNumber = merchantAcc,
            TransactionAmount = request.Amount,
            BeneficiaryName = "Garry Demo Payout",
            BeneficiaryAccountNumber = request.AccountNumber,
            SourceReferenceNumber = "WDL" + DateTime.Now.Ticks.ToString().Substring(10),
            ConnectorId = 221300,
            TransactionDescription = "OneTicket System Withdrawal",
            CapturedBy = portalEmail
        };

        try
        {
            string idempotencyKey = Guid.NewGuid().ToString();
            var response = await _oneKhusa.Transactions.SingleDisbursements.CreatePayoutAsync(sdkRequest, idempotencyKey);

            if (response is { IsSuccess: true, Data: not null })
            {
                return Ok(new
                {
                    message = "Payout Successful",
                    refNum = response.Data.TransactionReferenceNumber
                });
            }

            return BadRequest(new
            {
                title = response.Error?.Title ?? "Payout Declined",
                details = response.Error?.Detail ?? "Insufficient Funds in Sandbox Merchant Wallet."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { details = "SDK Connection Error: " + ex.Message });
        }
    }

    /// <summary>Processes batch disbursements from CSV/XLSX file with multiple payout records.</summary>
    [HttpPost("upload-file")]
    public async Task<IActionResult> UploadBatchFile([FromForm] IFormFile file, [FromQuery] string? email)
    {
        // 1. Validation
        if (file == null || file.Length == 0)
            return BadRequest(new { details = "No file selected." });

        // 2. Setup IDs
        int.TryParse(_config["OneKhusa:MerchantAccountNumber"], out int merchantAcc);
        var portalEmail = string.IsNullOrEmpty(email) ? "balalagarry@gmail.com" : email;
        string extension = Path.GetExtension(file.FileName).ToLower();

        try
        {
            // 3. READ RAW BYTES (This handles Excel and CSV correctly)
            // We do NOT loop through lines. We do NOT parse the data.
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            // 4. PREPARE SDK REQUEST
            var batchRequest = new UploadBatchFileRequest
            {
                MerchantAccountNumber = merchantAcc,
                CapturedBy = portalEmail,
                IsBatchScheduled = false,
                // The SDK tells OneKhusa how to parse the file based on this string
                ContentType = extension == ".csv" ? "CSV" : "XLSX",
                FileContent = fileBytes
            };

            // 5. SEND TO ONEKHUSA (Unique key must be 15-80 chars)
            string idempotencyKey = "UPLOAD-BATCH-FILE-" + Guid.NewGuid().ToString();

            var response = await _oneKhusa.Transactions.BatchDisbursements.UploadFileAsync(batchRequest, idempotencyKey);

            if (response is { IsSuccess: true, Data: not null })
            {
                return Ok(new
                {
                    batchNumber = response.Data.BatchNumber,
                    status = response.Data.BatchStatusCode,
                    message = "File received and queued for processing by OneKhusa."
                });
            }

            // Handle errors returned by the SDK
            return BadRequest(new
            {
                title = response.Error?.Title,
                details = response.Error?.Detail
            });
        }
        catch (Exception ex)
        {
            // If you see an error here, it's a connection issue
            return StatusCode(500, new { details = "Internal Server Error: " + ex.Message });
        }
    }
}