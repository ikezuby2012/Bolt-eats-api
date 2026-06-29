using System.Globalization;
using System.Text.RegularExpressions;
using Application.Abstractions.Services.UploadMedia;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.ImageUpload;

#pragma warning disable CA1308 // Normalize strings to uppercase
internal sealed class CloudinaryImageUploadService(IConfiguration config, ILogger<CloudinaryImageUploadService> logger) : IImageUploadService
{
    private const long ChunkThresholdBytes = 6 * 1024 * 1024;
    private readonly Cloudinary _cloudinary = BuildClient(config);
    private readonly string _baseFolder = config["Cloudinary:BaseFolder"] ?? "app";


    public async Task<bool> DeleteAsync(
         string publicId,
         CancellationToken cancellationToken = default)
    {
        try
        {
            DeletionResult result = await _cloudinary.DestroyAsync(
                new DeletionParams(publicId));

            return result.Result == "ok";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Cloudinary delete failed for {PublicId}", publicId);
            return false;
        }
    }

    public async Task<Application.Abstractions.Services.UploadResult.ImageUploadResult> ReplaceAsync(string existingPublicId, Stream stream, string fileName, string folder, ImageUploadOptions? options = null, CancellationToken cancellationToken = default)
    {
        Application.Abstractions.Services.UploadResult.ImageUploadResult uploadResult = await UploadAsync(
            stream, fileName, folder, options, cancellationToken);

        if (!uploadResult.IsSuccess)
        {
            return uploadResult;
        }


        // Best-effort delete of old image — log but don't fail the operation
        if (!string.IsNullOrEmpty(existingPublicId))
        {
            bool deleted = await DeleteAsync(existingPublicId, cancellationToken);
            if (!deleted)
            {
                logger.LogWarning(
                  "Could not delete old image {PublicId} after replace", existingPublicId);
            }

        }

        return uploadResult;
    }

    public async Task<Application.Abstractions.Services.UploadResult.ImageUploadResult> UploadAsync(Stream stream, string fileName, string folder, ImageUploadOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new ImageUploadOptions();

        try
        {
            // Validate before touching Cloudinary
            string? validation = ValidateStream(stream, fileName);
            if (validation is not null)
            {
                return new Application.Abstractions.Services.UploadResult.ImageUploadResult(false, null, null, null, validation);
            }


            string publicId = BuildPublicId(folder, fileName);
            string fullFolder = $"{_baseFolder}/{folder}";

            // Branch: direct upload vs chunked (resumable)
            return stream.Length > ChunkThresholdBytes
                ? await UploadChunkedAsync(stream, publicId, fullFolder, options, cancellationToken)
                : await UploadDirectAsync(stream, publicId, fullFolder, options, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Cloudinary upload failed for {FileName}", fileName);
            return new Application.Abstractions.Services.UploadResult.ImageUploadResult(false, null, null, null, "Upload failed. Please try again.");
        }
    }

    private async Task<Application.Abstractions.Services.UploadResult.ImageUploadResult> UploadDirectAsync(
        Stream stream,
        string publicId,
        string folder,
        ImageUploadOptions options,
        CancellationToken cancellationToken)
    {
        ImageUploadParams uploadParams = BuildUploadParams(stream, publicId, folder, options);

        CloudinaryDotNet.Actions.ImageUploadResult result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

        if (result.Error is not null)
        {
            logger.LogError("Cloudinary direct upload error: {Error}", result.Error.Message);
            return new Application.Abstractions.Services.UploadResult.ImageUploadResult(false, null, null, null, result.Error.Message);
        }

        return BuildResult(result.PublicId, result.SecureUrl.ToString(), options);
    }

    private static ImageUploadParams BuildUploadParams(
       Stream stream,
       string publicId,
       string folder,
       ImageUploadOptions options)
    {
        var transformations = new Transformation();

        // Resize on upload — saves storage + bandwidth
        if (options.MaxWidthPx.HasValue || options.MaxHeightPx.HasValue)
        {
            transformations = transformations
                .Width(options.MaxWidthPx)
                .Height(options.MaxHeightPx)
                .Crop("limit");   // never upscale
        }

        transformations = transformations
            .Quality(options.Quality.ToString(CultureInfo.InvariantCulture))
            .FetchFormat(options.Format == "auto" ? "auto" : options.Format);

        return new ImageUploadParams
        {
            File = new FileDescription("upload", stream),
            PublicId = publicId,
            Folder = folder,
            Overwrite = true,
            UniqueFilename = false,
            Transformation = transformations,
            // Keep original for reference if needed
            UseFilename = false,
            // Automatically remove background on profile photos (optional, uses AI credit)
            // BackgroundRemoval = "cloudinary_ai",
        };
    }

    private async Task<Application.Abstractions.Services.UploadResult.ImageUploadResult> UploadChunkedAsync(
       Stream stream,
       string publicId,
       string folder,
       ImageUploadOptions options,
       CancellationToken cancellationToken)
    {
        // TUS-style resumable: Cloudinary uses X-Unique-Upload-Id header
        // The .NET SDK handles chunking internally via UploadLargeAsync
        ImageUploadParams uploadParams = BuildUploadParams(stream, publicId, folder, options);
        uploadParams.UniqueFilename = false;

        // Chunk size: 6 MB per chunk
        CloudinaryDotNet.Actions.ImageUploadResult result = await _cloudinary.UploadLargeAsync(
            uploadParams,
            bufferSize: (int)ChunkThresholdBytes, cancellationToken);

        if (result.Error is not null)
        {
            logger.LogError("Cloudinary chunked upload error: {Error}", result.Error.Message);
            return new Application.Abstractions.Services.UploadResult.ImageUploadResult(false, null, null, null, result.Error.Message);
        }

        return BuildResult(result.PublicId, result.SecureUrl.ToString(), options);
    }

    private static Application.Abstractions.Services.UploadResult.ImageUploadResult BuildResult(
        string publicId,
        string url,
        ImageUploadOptions options)
    {
        // Build thumbnail URL using Cloudinary URL transforms (no extra API call)
        string? thumbnailUrl = null;
        if (options.GenerateThumbnail)
        {
            // Cloudinary URL transform: inject /c_fill,w_200,h_200/ into the URL
            thumbnailUrl = InjectTransform(
                url,
                $"c_fill,w_{options.ThumbnailSizePx},h_{options.ThumbnailSizePx},g_face");
        }

        return new Application.Abstractions.Services.UploadResult.ImageUploadResult(
            IsSuccess: true,
            PublicId: publicId,
            Link: url,
            ThumbnailLink: thumbnailUrl);
    }

    private static string InjectTransform(string url, string transform)
    {
        // Insert transform segment before /upload/ in the Cloudinary URL
        // e.g. https://res.cloudinary.com/demo/image/upload/v123/folder/image.webp
        //  →   https://res.cloudinary.com/demo/image/upload/c_fill,w_200,h_200/v123/folder/image.webp
        const string uploadSegment = "/upload/";
        int idx = url.IndexOf(uploadSegment, StringComparison.Ordinal);
        return idx < 0
            ? url
            : url.Insert(idx + uploadSegment.Length, $"{transform}/");
    }

    private static string BuildPublicId(string folder, string fileName)
    {
        // Strip extension — Cloudinary manages format via transformation
        string name = Path.GetFileNameWithoutExtension(fileName);
        string cleaned = Regex.Replace(name, @"[^a-zA-Z0-9_\-]", "_").ToLowerInvariant();
        return $"{folder}/{cleaned}_{Guid.NewGuid():N}";
    }

    private static string? ValidateStream(Stream stream, string fileName)
    {
        if (stream.Length == 0)
        {

            return "File is empty.";
        }
        // 50 MB hard ceiling
        const long maxBytes = 50L * 1024 * 1024;
        if (stream.Length > maxBytes)
        {
            return "File exceeds the maximum allowed size of 50 MB.";
        }



        string ext = Path.GetExtension(fileName).ToLowerInvariant();

        string[] allowed = [".jpg", ".jpeg", ".png", ".webp", ".gif", ".heic", ".jfif"];
        if (!allowed.Contains(ext))
        {
            return $"File type '{ext}' is not allowed. Accepted: jpg, jpeg, png, webp, gif, heic, jfif.";
        }


        return null;
    }

    private static Cloudinary BuildClient(IConfiguration config) =>
       new(new Account(
           config["Cloudinary:CloudName"]!,
           config["Cloudinary:ApiKey"]!,
           config["Cloudinary:ApiSecret"]!));
}
#pragma warning restore CA1308 // Normalize strings to uppercase
