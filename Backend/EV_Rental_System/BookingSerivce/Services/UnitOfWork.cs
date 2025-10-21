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

        public UnitOfWork(MyDbContext context)
        {
            _context = context;
        }

        // ===== REPOSITORY PROPERTIES =====

        public IOrderRepository Orders
        {
            get
            {
                _orderRepository ??= new OrderRepository(_context);
                return _orderRepository;
            }
        }

        public IPaymentRepository Payments => throw new NotImplementedException();
        public IOnlineContractRepository Contracts => throw new NotImplementedException();

        // ===== TRANSACTION MANAGEMENT =====

        /// <summary>
        /// Bắt đầu transaction mới
        /// </summary>
        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Transaction đã được bắt đầu rồi!");
            }

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// Commit transaction - Tự động SaveChanges và Commit
        /// </summary>
        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("Không có transaction nào để commit!");
            }

            try
            {
                // Lưu tất cả changes vào database
                await _context.SaveChangesAsync();

                // Commit transaction
                await _transaction.CommitAsync();
            }
            catch
            {
                // Nếu có lỗi, tự động rollback
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                // Dispose transaction sau khi commit
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <summary>
        /// Rollback transaction
        /// </summary>
        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("Không có transaction nào để rollback!");
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

        // ===== ⚠️ QUAN TRỌNG: XÓA METHOD SaveChangesAsync() =====
        // Method này gây nhầm lẫn và duplicate SaveChanges
        // Services CHỈ nên dùng:
        // - BeginTransactionAsync()
        // - CommitTransactionAsync() (đã có SaveChanges bên trong)
        // - RollbackTransactionAsync()

        // ❌ ĐÃ XÓA:
        // public async Task<int> SaveChangesAsync()
        // {
        //     return await _context.SaveChangesAsync();
        // }

        // ===== DISPOSE PATTERN =====

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose transaction nếu còn
                    _transaction?.Dispose();

                    // Dispose context
                    _context?.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}