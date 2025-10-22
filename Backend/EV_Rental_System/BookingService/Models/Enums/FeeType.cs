namespace BookingService.Models.Enums
{
    /// <summary>
    /// Type of additional fee charged to the customer
    /// </summary>
    public enum FeeType
    {
        LateReturn,      // Fee for returning vehicle late
        Damage,          // Fee for vehicle damage
        ExcessMileage,   // Fee for exceeding included mileage
        Cleaning,        // Fee for extra cleaning required
        Other            // Other miscellaneous fees
    }
}
