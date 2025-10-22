using BookingService.Models;

namespace BookingService.Repositories
{
    public interface IOnlineContractRepository
    {
        /// <summary>
        /// Tạo bản ghi hợp đồng mới (sau khi thanh toán).
        /// </summary>
        Task<OnlineContract> CreateAsync(OnlineContract contract);

        /// <summary>
        /// Lấy hợp đồng bằng ID (hiếm dùng).
        /// </summary>
        Task<OnlineContract?> GetByIdAsync(int contractId);

        /// <summary>
        /// Lấy hợp đồng bằng OrderId (phổ biến nhất).
        /// </summary>
        Task<OnlineContract?> GetByOrderIdAsync(int orderId);

        /// <summary>
        /// Lấy hợp đồng bằng mã hợp đồng.
        /// </summary>
        Task<OnlineContract?> GetByContractNumberAsync(string contractNumber);

        /// <summary>
        /// Kiểm tra xem hợp đồng đã tồn tại cho Order này chưa.
        /// </summary>
        Task<bool> ExistsByOrderIdAsync(int orderId);
    }
}