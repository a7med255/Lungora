using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Lungora.Bl;
using Lungora.Bl.Interfaces;

namespace Lungora.Services
{
    public class ImageService : IImageService
    {
        private readonly Cloudinary cloudinary;
        private readonly LungoraContext context;

        public ImageService(LungoraContext context, Cloudinary cloudinary)
        {
            this.context = context;
            this.cloudinary = cloudinary;
        }
        public async Task<string> UploadOneImageAsync(IFormFile file, string Folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Choose an image");

            string uniqueFileName = $"{Guid.NewGuid()}_{DateTime.Now:yyyy_MM_dd}_{file.FileName}";

            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(uniqueFileName, stream),
                Folder = Folder
            };

            var uploadResult = await cloudinary.UploadAsync(uploadParams);
            string imageUrl = uploadResult.SecureUrl.ToString();

            return imageUrl;
        }
    }
}
