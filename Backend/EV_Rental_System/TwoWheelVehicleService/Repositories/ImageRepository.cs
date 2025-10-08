using Microsoft.EntityFrameworkCore;
using TwoWheelVehicleService.Models;

namespace TwoWheelVehicleService.Repositories
{
    public class ImageRepository : IImageRepository
    {
        private readonly MyDbContext _context;

        public ImageRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task AddImage(Image image)
        {
            await _context.Images.AddAsync(image);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Image>> GetImagesByModelId(int modelId)
        {
            return await _context.Images
                .Where(img => img.ModelId == modelId)
                .ToListAsync();
        }

        public async Task DeleteImages(List<int> imageIds)
        {
            foreach (var item in imageIds)
            {
                var image = await _context.Images.FindAsync(item);
                if (image != null)
                {
                    _context.Images.Remove(image);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task DeleteImage(int imageId)
        {
            var image = await _context.Images.FindAsync(imageId);
            if (image != null)
            {
                _context.Images.Remove(image);
                await _context.SaveChangesAsync();
            }
        }
    }
}