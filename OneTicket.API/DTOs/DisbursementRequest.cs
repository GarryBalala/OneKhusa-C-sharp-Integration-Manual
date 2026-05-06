namespace OneTicket.API.DTOs;

/// <summary>
/// Request model for processing a single payout disbursement to a beneficiary account.
/// Used to collect all required details for immediate fund transfer via OneKhusa.
/// </summary>
public class SinglePayoutRequest
{
    /// <summary>
    /// System identifier for the organizer or user initiating the withdrawal
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Withdrawal amount in currency units (e.g., MWK, ZWL)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Full name of the beneficiary receiving funds
    /// </summary>
    public string BeneficiaryName { get; set; } = string.Empty;

    /// <summary>
    /// Bank account number or mobile money number for fund transfer
    /// </summary>
    public string AccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// OneKhusa bank connector ID (e.g., 221300 for National Bank, 112400 for Airtel Money)
    /// </summary>
    public int ConnectorId { get; set; }

    /// <summary>
    /// Human-readable description of the transaction for audit trail
    /// </summary>
    public string Description { get; set; } = "Organizer Withdrawal";

    /// <summary>
    /// Portal user email for transaction logging and audit
    /// </summary>
    public string Email { get; set; } = string.Empty;
}