namespace Lungora.Bl.Interfaces
{
    public interface IImageService
    {
        Task<string> UploadOneImageAsync(IFormFile files, string Folder);
    }
}
