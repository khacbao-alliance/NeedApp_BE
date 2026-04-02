namespace NeedApp.Application.Interfaces;

public interface ICloudinaryService
{
    Task<CloudinaryUploadResult> UploadFileAsync(Stream fileStream, string fileName, string folder = "files");
    Task<CloudinaryUploadResult> UploadImageAsync(Stream imageStream, string fileName, string folder = "avatars");
    Task DeleteFileAsync(string publicId);
}

public record CloudinaryUploadResult(string PublicId, string Url, long Bytes, string Format);
