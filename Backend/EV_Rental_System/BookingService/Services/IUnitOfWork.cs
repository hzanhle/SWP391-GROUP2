using BookingService.Repositories;

namespace BookingService.Services
{
    public interface IUnitOfWork : IDisposable
    {
        // Transaction Management
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

        // Repository Access (nếu cần)
        IOrderRepository Orders { get; }
        // Thêm các repository khác nếu cần
        IPaymentRepository Payments { get; }
        IOnlineContractRepository Contracts { get; }
    }
}
