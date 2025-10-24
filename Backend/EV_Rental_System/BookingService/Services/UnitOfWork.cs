using BookingService.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace BookingService.Services
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MyDbContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed = false;

        // Lazy initialization cho repositories
        private IOrderRepository? _orderRepository;
        private IPaymentRepository? _paymentRepository;
        private IOnlineContractRepository? _contractRepository;

        public UnitOfWork(MyDbContext context)
        {
            _context = context;
        }

        // ===== REPOSITORIES =====

        public IOrderRepository Orders
        {
            get
            {
                _orderRepository ??= new OrderRepository(_context);
                return _orderRepository;
            }
        }

        public IPaymentRepository Payments
        {
            get
            {
                _paymentRepository ??= new PaymentRepository(_context);
                return _paymentRepository;
            }
        }

        public IOnlineContractRepository Contracts
        {
            get
            {
                _contractRepository ??= new OnlineContractRepository(_context);
                return _contractRepository;
            }
        }

        // ===== TRANSACTIONS =====

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Transaction đã được bắt đầu rồi!");
            }

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("Chưa có transaction để commit!");
            }

            try
            {
                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("Chưa có transaction để rollback!");
            }

            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // ===== DISPOSE =====

        public void Dispose()
        {
            if (!_disposed)
            {
                _transaction?.Dispose();
                _context?.Dispose();
                _disposed = true;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                }

                if (_context != null)
                {
                    await _context.DisposeAsync();
                }

                _disposed = true;
            }
        }
    }
}