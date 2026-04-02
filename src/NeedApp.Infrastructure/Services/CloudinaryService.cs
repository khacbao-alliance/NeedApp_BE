using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using NeedApp.Application.Interfaces;
using NeedApp.Infrastructure.Settings;

namespace NeedApp.Infrastructure.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IOptions<CloudinarySettings> options)
    {
        var settings = options.Value;
        var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<CloudinaryUploadResult> UploadFileAsync(Stream fileStream, string fileName, string folder = "files")
    {
        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = $"needapp/{folder}",
            UseFilename = true,
            UniqueFilename = true
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
            throw new Exception($"Cloudinary upload failed: {result.Error.Message}");

        return new CloudinaryUploadResult(result.PublicId, result.SecureUrl.ToString(), result.Bytes, result.Format);
    }

    public async Task<CloudinaryUploadResult> UploadImageAsync(Stream imageStream, string fileName, string folder = "avatars")
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, imageStream),
            Folder = $"needapp/{folder}",
            Transformation = new Transformation().Width(400).Height(400).Crop("fill").Gravity("face")
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
            throw new Exception($"Cloudinary upload failed: {result.Error.Message}");

        return new CloudinaryUploadResult(result.PublicId, result.SecureUrl.ToString(), result.Bytes, result.Format);
    }

    public async Task DeleteFileAsync(string publicId)
    {
        await _cloudinary.DestroyAsync(new DeletionParams(publicId));
    }
}
