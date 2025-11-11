using Microsoft.EntityFrameworkCore;
using TwoWheelVehicleService.Models;
using TwoWheelVehicleService.Repositories;

namespace TwoWheelVehicleService.Services
{
    public class TransferVehicleService : ITransferVehicleService
    {
        private readonly ITransferVehicleRepository _transferVehicleRepository;
        private readonly IVehicleService _vehicleService;
        private readonly ILogger<TransferVehicleService> _logger;

        public TransferVehicleService(ITransferVehicleRepository transferVehicleRepository, ILogger<TransferVehicleService> logger, IVehicleService vehicleService)
        {
            _transferVehicleRepository = transferVehicleRepository;
            _logger = logger;
            _vehicleService = vehicleService;
        }

        public async Task<IEnumerable<TransferVehicle>> GetAllTransferVehicle()
        {
            return await _transferVehicleRepository.GetTransferVehicles();
        }

        public async Task AddTransferVehicle(int vehicleId, int modelId, int currentStationId, int targetStationId)
        {
            var existingTransfer = await _transferVehicleRepository.GetTransferVehicleByVehicleId(vehicleId);

            if (existingTransfer != null)
            {
                throw new InvalidOperationException($"Vehicle {vehicleId} already has a transfer request");
            }

            var transferVehicle = new TransferVehicle
            {
                VehicleId = vehicleId,
                ModelId = modelId,
                CurrentStationId = currentStationId,
                TargetStationId = targetStationId,
                TransferStatus = "Đang chuyển", // Trạng thái mặc định
                CreateAt = DateTime.UtcNow
            };
            _transferVehicleRepository.AddTransferVehicle(transferVehicle);
        }

        public async Task<TransferVehicle?> GetTransferVehicleByVehicleId(int vehicleId)
        {
            return await _transferVehicleRepository.GetTransferVehicleByVehicleId(vehicleId);
        }

        public async Task<IEnumerable<TransferVehicle>> GetTransferVehiclesByModelId(int modelId)
        {
            return await _transferVehicleRepository.GetTransferVehiclesByModelId(modelId);
        }

        public async Task UpdateTransferVehicle(int vehicleId, string status)
        {
            var transferVehicle = await _transferVehicleRepository.GetTransferVehicleByVehicleId(vehicleId);
            if (transferVehicle == null)
            {
                _logger.LogWarning("Transfer vehicle with VehicleId {VehicleId} not found.", vehicleId);
                return;
            }
            if (status.Equals("Hoàn thành"))
            {
                // Cập nhật lại StationId của Vehicle khi hoàn thành chuyển xe
                await _vehicleService.UpdateVehicleStationId(vehicleId, transferVehicle.TargetStationId);
            }
            transferVehicle.TransferStatus = status;
            transferVehicle.UpdateAt = DateTime.Now;
            await _transferVehicleRepository.UpdateTransferVehicle(transferVehicle);
        }

        public async Task DeleteTransferVehicle(int vehicleId)
        {
            var transferVehicle = await _transferVehicleRepository.GetTransferVehicleByVehicleId(vehicleId);
            if (transferVehicle == null)
            {
                _logger.LogWarning("Transfer vehicle with VehicleId {VehicleId} not found.", vehicleId);
                return;
            }
            await _transferVehicleRepository.DeleteTransferVehicle(transferVehicle);
        }

        public Task<IEnumerable<TransferVehicle>> GetTransferVehiclesByStatus(string status)
        {
            return _transferVehicleRepository.GetTransferVehiclesByStatus(status);
        }

        
    }
}
