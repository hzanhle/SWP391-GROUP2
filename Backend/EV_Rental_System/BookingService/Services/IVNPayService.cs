namespace BookingService.Services
{
    public interface IVNPayService
    {
        string CreatePaymentUrl(int orderId, decimal amount, string description);
        bool ValidateCallback(IQueryCollection query);
    }
}