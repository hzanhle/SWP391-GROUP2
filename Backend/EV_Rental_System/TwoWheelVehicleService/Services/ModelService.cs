using TwoWheelVehicleService.DTOs;
using TwoWheelVehicleService.Models;
using TwoWheelVehicleService.Repositories;
using Microsoft.Extensions.Logging;

namespace TwoWheelVehicleService.Services
{
    public class ModelService : IModelService
    {
        private readonly IModelRepository _modelRepository;
        private readonly IVehicleService _vehicleService;
        private readonly IImageService _imageService;
        private readonly ILogger<ModelService> _logger;

        public ModelService(
            IModelRepository modelRepository,
            IImageService imageService,
            IVehicleService vehicleService,
            ILogger<ModelService> logger)
        {
            _modelRepository = modelRepository;
            _imageService = imageService;
            _vehicleService = vehicleService;
            _logger = logger;
        }

        public async Task AddModelAsync(ModelRequest request)
        {
            try
            {
                _logger.LogInformation("Adding new model: {ModelName}", request.ModelName);

                var model = new Model
                {
                    ModelName = request.ModelName,
                    Manufacturer = request.Manufacturer,
                    Year = request.Year,
                    MaxSpeed = request.MaxSpeed,
                    BatteryCapacity = request.BatteryCapacity,
                    ChargingTime = request.ChargingTime,
                    BatteryRange = request.BatteryRange,
                    VehicleCapacity = request.VehicleCapacity,
                    ModelCost = request.ModelCost,
                    RentFeeForHour = request.RentFeeForHour,
                    IsActive = true
                };

                await _modelRepository.AddModel(model);

                if (request.Files != null && request.Files.Count > 0)
                {
                    var uploadedImages = await _imageService.UploadImagesAsync(request.Files, model.ModelId);
                    foreach (var image in uploadedImages)
                        await _imageService.AddImage(image);

                    _logger.LogInformation("Uploaded {Count} images for model {ModelId}", request.Files.Count, model.ModelId);
                }

                _logger.LogInformation("Model created successfully: {ModelId}", model.ModelId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding model: {ModelName}", request.ModelName);
                throw;
            }
        }

        public async Task<ModelDTO?> GetModelByIdAsync(int modelId)
        {
            try
            {
                var model = await _modelRepository.GetModelById(modelId);
                if (model == null)
                {
                    _logger.LogWarning("Model not found with ID: {ModelId}", modelId);
                    return null;
                }

                var imageUrls = await _imageService.GetImagePathsAsync(modelId);

                return new ModelDTO
                {
                    ModelId = model.ModelId,
                    ModelName = model.ModelName,
                    Manufacturer = model.Manufacturer,
                    Year = model.Year,
                    MaxSpeed = model.MaxSpeed,
                    BatteryCapacity = model.BatteryCapacity,
                    ChargingTime = model.ChargingTime,
                    BatteryRange = model.BatteryRange,
                    VehicleCapacity = model.VehicleCapacity,
                    IsActive = model.IsActive,
                    ModelCost = model.ModelCost,
                    RentFeeForHour = model.RentFeeForHour,
                    ImageUrls = imageUrls
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching model with ID: {ModelId}", modelId);
                throw;
            }
        }

        public async Task<List<ModelDTO>> GetAllModelsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all models...");
                var models = await _modelRepository.GetAllModels();
                var result = new List<ModelDTO>();

                foreach (var model in models)
                {
                    var imageUrls = await _imageService.GetImagePathsAsync(model.ModelId);
                    result.Add(new ModelDTO
                    {
                        ModelId = model.ModelId,
                        ModelName = model.ModelName,
                        Manufacturer = model.Manufacturer,
                        Year = model.Year,
                        MaxSpeed = model.MaxSpeed,
                        BatteryCapacity = model.BatteryCapacity,
                        ChargingTime = model.ChargingTime,
                        BatteryRange = model.BatteryRange,
                        VehicleCapacity = model.VehicleCapacity,
                        IsActive = model.IsActive,
                        ModelCost = model.ModelCost,
                        RentFeeForHour = model.RentFeeForHour,
                        ImageUrls = imageUrls
                    });
                }

                _logger.LogInformation("Retrieved {Count} models", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all models");
                throw;
            }
        }

        public async Task<List<ModelDTO>> GetActiveModelsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching active models...");
                var models = await _modelRepository.GetActiveModels();
                var result = new List<ModelDTO>();

                foreach (var model in models)
                {
                    var imageUrls = await _imageService.GetImagePathsAsync(model.ModelId);
                    result.Add(new ModelDTO
                    {
                        ModelId = model.ModelId,
                        ModelName = model.ModelName,
                        Manufacturer = model.Manufacturer,
                        Year = model.Year,
                        MaxSpeed = model.MaxSpeed,
                        BatteryCapacity = model.BatteryCapacity,
                        ChargingTime = model.ChargingTime,
                        BatteryRange = model.BatteryRange,
                        VehicleCapacity = model.VehicleCapacity,
                        IsActive = model.IsActive,
                        ModelCost = model.ModelCost,
                        RentFeeForHour = model.RentFeeForHour,
                        ImageUrls = imageUrls
                    });
                }

                _logger.LogInformation("Retrieved {Count} active models", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching active models");
                throw;
            }
        }

        public async Task UpdateModelAsync(int modelId, ModelRequest request)
        {
            try
            {
                _logger.LogInformation("Updating model ID {ModelId}", modelId);

                var existingModel = await _modelRepository.GetModelById(modelId);
                if (existingModel == null)
                {
                    _logger.LogWarning("Model not found for update: ID {ModelId}", modelId);
                    throw new ArgumentException("Model not found");
                }

                existingModel.ModelName = request.ModelName;
                existingModel.Manufacturer = request.Manufacturer;
                existingModel.Year = request.Year;
                existingModel.MaxSpeed = request.MaxSpeed;
                existingModel.BatteryCapacity = request.BatteryCapacity;
                existingModel.ChargingTime = request.ChargingTime;
                existingModel.BatteryRange = request.BatteryRange;
                existingModel.VehicleCapacity = request.VehicleCapacity;
                existingModel.ModelCost = request.ModelCost;
                existingModel.RentFeeForHour = request.RentFeeForHour;

                await _modelRepository.UpdateModel(existingModel);
                _logger.LogInformation("Updated model successfully: {ModelId}", modelId);

                if (request.Files != null && request.Files.Count > 0)
                {
                    await _imageService.DeleteImagesAsync(modelId);
                    var uploadedImages = await _imageService.UploadImagesAsync(request.Files, modelId);
                    foreach (var image in uploadedImages)
                        await _imageService.AddImage(image);

                    _logger.LogInformation("Replaced {Count} images for model {ModelId}", request.Files.Count, modelId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating model ID {ModelId}", modelId);
                throw;
            }
        }

        public async Task DeleteModelAsync(int modelId)
        {
            try
            {
                _logger.LogInformation("Deleting model ID {ModelId}", modelId);

                await _imageService.DeleteImagesAsync(modelId);
                await _modelRepository.DeleteModel(modelId);

                _logger.LogInformation("Deleted model successfully: {ModelId}", modelId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting model ID {ModelId}", modelId);
                throw;
            }
        }

        public async Task ToggleStatusAsync(int modelId)
        {
            try
            {
                _logger.LogInformation("Toggling status for model ID {ModelId}", modelId);

                var model = await _modelRepository.GetModelById(modelId);
                if (model == null)
                {
                    _logger.LogWarning("Model not found while toggling status: {ModelId}", modelId);
                    return;
                }

                model.IsActive = !model.IsActive;
                await _modelRepository.UpdateModel(model);

                var vehicles = await _vehicleService.GetAllVehiclesByModelId(modelId);
                foreach (var vehicle in vehicles)
                {
                    vehicle.IsActive = model.IsActive;
                    await _vehicleService.UpdateVehicleAsync(vehicle);
                }

                _logger.LogInformation("Model {ModelId} and {Count} vehicles toggled to IsActive={Status}",
                    modelId, vehicles.Count, model.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling model status for ID {ModelId}", modelId);
                throw;
            }
        }

        public async Task<List<ModelDTO>> SearchModelsAsync(string searchValue)
        {
            try
            {
                var models = await _modelRepository.SearchModelsAsync(searchValue);
                if (models == null)
                {
                    _logger.LogWarning("No models found for search value: {SearchValue}", searchValue);
                    return new List<ModelDTO>();
                }

                var list = new List<ModelDTO>();
                foreach (var item in models)
                {
                    var model = new ModelDTO
                    {
                        ModelId = item.ModelId,
                        ModelName = item.ModelName,
                        Manufacturer = item.Manufacturer,
                        Year = item.Year,
                        MaxSpeed = item.MaxSpeed,
                        BatteryCapacity = item.BatteryCapacity,
                        ChargingTime = item.ChargingTime,
                        BatteryRange = item.BatteryRange,
                        VehicleCapacity = item.VehicleCapacity,
                        IsActive = item.IsActive,
                        ModelCost = item.ModelCost,
                        RentFeeForHour = item.RentFeeForHour
                    };
                    list.Add(model);
                }

                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching models with value: {SearchValue}", searchValue);
                throw;
            }
        }
    }
}
