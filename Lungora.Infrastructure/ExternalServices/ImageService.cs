using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Lungora.Bl;
using Lungora.Bl.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Lungora.Services
{
    public class ImageService : IImageService
    {
        private readonly Cloudinary cloudinary;

        public ImageService( Cloudinary cloudinary)
        {
            this.cloudinary = cloudinary;
        }
        public async Task<string> UploadOneImageAsync(IFormFile file, string Folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Choose an image");

            //var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };

            //var fileExtension = Path.GetExtension(file.FileName).ToLower();
            //if (!allowedExtensions.Contains(fileExtension))
            //    throw new ArgumentException("Invalid file type. Only images are allowed!");

            //if (!file.ContentType.StartsWith("image/"))
            //    throw new ArgumentException("Invalid file format. Please upload an image!");

            string uniqueFileName = $"{Guid.NewGuid()}_{DateTime.Now:yyyy_MM_dd}_{file.FileName}";

            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(uniqueFileName, stream),
                Folder = Folder,
                UseFilename = true,
                UniqueFilename = true
            };

            var uploadResult = await cloudinary.UploadAsync(uploadParams);
            string imageUrl = uploadResult.SecureUrl.ToString();

            return imageUrl;
        }
    }
}
