using BookingService.Models;
using BookingService.DTOs; // Cần ContractDetailsDto
using System.Threading.Tasks;

namespace BookingService.Services
{
    public interface IOnlineContractService
    {
        /// <summary>
        /// (Giữ nguyên) Tạo hợp đồng PDF từ snapshot FE và data BE.
        /// Được gọi bởi OrderService sau khi thanh toán thành công.
        /// </summary>
        /// <param name="completedPayment">Tham chiếu 3: Dữ liệu thanh toán (từ BE)</param>
        /// <param name="contractDataJson">Chuỗi JSON (snapshot) chứa ContractBindingData (từ FE)</param>
        /// <returns>Đối tượng OnlineContract đã được lưu.</returns>
        Task<OnlineContract> GenerateAndSendContractAsync( // <<-- CORRECT NAME & PARAMETERS
            int orderId,
            int userId,
            int vehicleId,
            Payment completedPayment // Or Payment DTO if PaymentService returns DTO
        );

        /// <summary>
        /// (Giữ nguyên) Lấy chi tiết hợp đồng (đã ký) để xem lại.
        /// </summary>
        /// <param name="orderId">ID của Order.</param>
        /// <returns>DTO chứa thông tin chi tiết hoặc null nếu không tìm thấy.</returns>
        Task<ContractDetailsDto?> GetContractDetailsByOrderIdAsync(int orderId);

        /// <summary>
        /// (Giữ nguyên) Lấy URL (đường dẫn tương đối) để tải file hợp đồng ĐÃ KÝ.
        /// </summary>
        /// <param name="orderId">ID của Order.</param>
        /// <param name="userId">ID của người dùng yêu cầu (để kiểm tra quyền).</param>
        /// <returns>Đường dẫn tương đối tới file PDF hoặc null nếu không có quyền/không tìm thấy.</returns>
        Task<string?> GetContractDownloadUrlAsync(int orderId, int userId);
    }
}