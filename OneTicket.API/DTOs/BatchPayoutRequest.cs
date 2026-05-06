namespace OneTicket.API.DTOs;

/// <summary>
/// Request model for batch disbursement operations.
/// Contains admin metadata and audit information for bulk payout processing.
/// </summary>
public class BatchPayoutRequest
{
    /// <summary>
    /// Email address of the administrator or portal user executing the batch operation
    /// Used for audit trail and transaction capture identification
    /// </summary>
    public string AdminEmail { get; set; } = "balalagarry@gmail.com";

    /// <summary>
    /// Descriptive label for the batch operation (e.g., "Weekly Event Payouts", "Emergency Refunds")
    /// Assists with transaction categorization and reporting
    /// </summary>
    public string BatchDescription { get; set; } = "Weekly Event Payouts";
}