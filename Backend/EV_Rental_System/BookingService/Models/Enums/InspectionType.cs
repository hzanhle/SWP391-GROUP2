namespace BookingService.Models.Enums
{
    /// <summary>
    /// Type of vehicle inspection
    /// </summary>
    public enum InspectionType
    {
        Pickup,  // Inspection when customer picks up vehicle (start rental)
        Return   // Inspection when customer returns vehicle (end rental)
    }
}
