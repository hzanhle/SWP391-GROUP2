namespace BookingService.Models.Enums
{
    /// <summary>
    /// Overall condition of the vehicle during inspection
    /// </summary>
    public enum VehicleCondition
    {
        Excellent,  // Like new, no issues
        Good,       // Minor wear and tear, acceptable
        Fair,       // Some issues but still usable
        Poor        // Significant issues, needs attention
    }
}
