using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Repositories
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

        
        public async Task DeleteImage(int imageId)
        {
            var image = await _context.Images.FindAsync(imageId);
            if (image != null)
            {
                _context.Images.Remove(image);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Image>> GetImagesByTypeId(string type, int typeId)
        {
            return await _context.Images
                .Where(img => img.Type == type && img.TypeId == typeId)
                .ToListAsync();
        }
    }
}