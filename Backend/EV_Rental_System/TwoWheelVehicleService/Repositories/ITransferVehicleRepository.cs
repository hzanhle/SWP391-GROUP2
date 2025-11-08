using TwoWheelVehicleService.Models;

namespace TwoWheelVehicleService.Repositories
{
    public interface ITransferVehicleRepository
    {
        public Task<IEnumerable<TransferVehicle>> GetTransferVehicles();

        public Task<IEnumerable<TransferVehicle>> GetTransferVehiclesByModelId(int modelId);

        public Task<TransferVehicle?> GetTransferVehicleByVehicleId(int vehicleId);

        public Task AddTransferVehicle(TransferVehicle transferVehicle);

        public Task UpdateTransferVehicle(TransferVehicle transferVehicle);

        public Task DeleteTransferVehicle(TransferVehicle transferVehicle);
        public Task<IEnumerable<TransferVehicle>> GetTransferVehiclesByStatus(string status);

    }
}
