namespace OneTicket.API.DTOs;

/// <summary>
/// Webhook payload model for receiving payment confirmation events from OneKhusa API.
/// Contains transaction status, metadata, and event classification for event-driven updates.
/// </summary>
public class OneKhusaWebhook
{
    public string? ResponseCode { get; set; }
    public string? PaymentTransactionId { get; set; }

    public WebhookMetaData MetaData { get; set; } = new();
}
/// <summary>
/// Metadata container for webhook events containing transaction identifiers and audit data.
/// </summary>
public class WebhookMetaData
{
   
    /// Unique reference number assigned by the system for this transaction
    
    public string ReferenceNumber { get; set; } = string.Empty;
}