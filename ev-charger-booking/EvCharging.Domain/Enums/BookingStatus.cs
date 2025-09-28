namespace EvCharging.Domain.Enums;


public static class BookingStatus
{
    public const string Pending = "Pending"; // created by owner, awaiting approval
    public const string Approved = "Approved"; // approved, QR generated
    public const string Completed = "Completed"; // session finalized by operator
    public const string Cancelled = "Cancelled"; // cancelled by owner/backoffice (>= 12h rule)
}