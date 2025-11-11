using TwoWheelVehicleService.Models;

namespace TwoWheelVehicleService.Services
{
    public interface ITransferVehicleService
    {
        public Task<IEnumerable<TransferVehicle>> GetAllTransferVehicle();

        public Task<IEnumerable<TransferVehicle>> GetTransferVehiclesByModelId(int modelId);

        public Task<TransferVehicle?> GetTransferVehicleByVehicleId(int vehicleId);

        public Task<IEnumerable<TransferVehicle>> GetTransferVehiclesByStatus(string status);

        public Task AddTransferVehicle(int vehicleId, int modelId, int currentStationId, int targetStationId);

        public Task UpdateTransferVehicle(int vehicleId, string status);

        public Task DeleteTransferVehicle(int vehicleId);
    }
}
