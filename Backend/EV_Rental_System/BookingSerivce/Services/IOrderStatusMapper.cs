namespace BookingSerivce.Services
{
    /// <summary>
    /// Maps internal order status to role-based display strings.
    /// Different roles see different user-friendly status messages.
    /// </summary>
    public interface IOrderStatusMapper
    {
        /// <summary>
        /// Gets customer-friendly status display text.
        /// </summary>
        string GetCustomerDisplayStatus(string internalStatus);

        /// <summary>
        /// Gets admin/staff-friendly status display text.
        /// </summary>
        string GetAdminDisplayStatus(string internalStatus);

        /// <summary>
        /// Checks if a status transition is valid.
        /// </summary>
        bool IsValidStatusTransition(string currentStatus, string newStatus);

        /// <summary>
        /// Gets available actions for admin/staff based on current status.
        /// </summary>
        IEnumerable<string> GetAvailableActions(string currentStatus);
    }
}
