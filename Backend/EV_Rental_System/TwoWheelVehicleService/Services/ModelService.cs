using TwoWheelVehicleService.DTOs;
using TwoWheelVehicleService.Models;
using TwoWheelVehicleService.Repositories;

namespace TwoWheelVehicleService.Services
{
    public class ModelService : IModelService
    {
        private readonly IModelRepository _modelRepository;
        private readonly IImageService _imageService;

        public ModelService(IModelRepository modelRepository, IImageService imageService)
        {
            _modelRepository = modelRepository;
            _imageService = imageService;
        }

        public async Task AddModelAsync(ModelRequest request)
        {
            // 1. Tạo Model entity từ request
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
                Price = request.Price,
                IsActive = true // Default value
            };

            // 2. Lưu model để có ModelId
            await _modelRepository.AddModel(model);

            // 3. Xử lý hình ảnh nếu có
            if (request.Files != null && request.Files.Count > 0)
            {
                var uploadedImages = await _imageService.UploadImagesAsync(request.Files, model.ModelId);
                foreach (var image in uploadedImages)
                {
                    await _imageService.AddImage(image);
                }
            }
        }

        public async Task<ModelDTO> GetModelByIdAsync(int modelId)
        {
            var model = await _modelRepository.GetModelById(modelId);
            if (model == null)
                return null;

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
                Price = model.Price,
                ImageUrls = imageUrls
            };
        }

        public async Task<List<ModelDTO>> GetAllModelsAsync()
        {
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
                    Price = model.Price,
                    ImageUrls = imageUrls
                });
            }

            return result;
        }

        public async Task<List<ModelDTO>> GetActiveModelsAsync()
        {
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
                    Price = model.Price,
                    ImageUrls = imageUrls
                });
            }

            return result;
        }

        public async Task UpdateModelAsync(int modelId, ModelRequest request)
        {
            var existingModel = await _modelRepository.GetModelById(modelId);
            if (existingModel == null)
                throw new ArgumentException("Model not found");

            // 1. Cập nhật thông tin model
            existingModel.ModelName = request.ModelName;
            existingModel.Manufacturer = request.Manufacturer;
            existingModel.Year = request.Year;
            existingModel.MaxSpeed = request.MaxSpeed;
            existingModel.BatteryCapacity = request.BatteryCapacity;
            existingModel.ChargingTime = request.ChargingTime;
            existingModel.BatteryRange = request.BatteryRange;
            existingModel.VehicleCapacity = request.VehicleCapacity;
            existingModel.Price = request.Price;

            await _modelRepository.UpdateModel(existingModel);

            // 2. Xử lý hình ảnh nếu có hình mới
            if (request.Files != null && request.Files.Count > 0)
            {
                // Xóa hình cũ
                await _imageService.DeleteImagesAsync(modelId);

                // Upload hình mới
                var uploadedImages = await _imageService.UploadImagesAsync(request.Files, modelId);
                foreach (var image in uploadedImages)
                {
                    await _imageService.AddImage(image);
                }
            }
        }

        public async Task DeleteModelAsync(int modelId)
        {
            // 1. Xóa tất cả hình ảnh liên quan
            //await _imageService.DeleteImagesAsync(modelId);

            // 2. Soft delete model
            await _modelRepository.DeleteModel(modelId);
        }

        
    }
}