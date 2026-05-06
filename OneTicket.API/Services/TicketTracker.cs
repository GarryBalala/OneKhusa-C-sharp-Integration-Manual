using System.Collections.Concurrent;

namespace OneTicket.API.Services;

/// <summary>
/// Thread-safe in-memory service for tracking real-time payment status.
/// Maintains a concurrent dictionary of transaction references to payment states.
/// Enables polling from UI clients without external database dependencies.
/// </summary>
public class TicketTracker
{
    /// <summary>
    /// Thread-safe dictionary mapping transaction reference numbers to their current payment status
    /// </summary>
    private readonly ConcurrentDictionary<string, string> _statusMap = new();

    /// <summary>
    /// Updates or creates a status entry for a given transaction reference
    /// </summary>
    /// <param name="reference">Unique transaction reference identifier</param>
    /// <param name="status">Current payment status (e.g., "Reserved", "Paid", "Pending")</param>
    public void UpdateStatus(string reference, string status)
    {
        _statusMap[reference] = status;
    }

    /// <summary>
    /// Retrieves current payment status for a transaction reference
    /// </summary>
    /// <param name="reference">Unique transaction reference identifier</param>
    /// <returns>Current status or "Reserved" if reference not found</returns>
    public string GetStatus(string reference)
    {
        return _statusMap.TryGetValue(reference, out var status) ? status : "Reserved";
    }
}