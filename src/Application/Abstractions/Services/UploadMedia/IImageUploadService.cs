using Application.Abstractions.Services.UploadResult;

namespace Application.Abstractions.Services.UploadMedia;

public interface IImageUploadService
{
    Task<ImageUploadResult> UploadAsync(
       Stream stream,
       string fileName,
       string folder,
       ImageUploadOptions? options = null,
       CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
       string publicId,
       CancellationToken cancellationToken = default);

    Task<ImageUploadResult> ReplaceAsync(
        string existingPublicId,
        Stream stream,
        string fileName,
        string folder,
        ImageUploadOptions? options = null,
        CancellationToken cancellationToken = default);
}

public record ImageUploadOptions(
    int? MaxWidthPx = null,
    int? MaxHeightPx = null,
    int Quality = 80,
    string Format = "webp",
    bool GenerateThumbnail = true,
    int ThumbnailSizePx = 200);
