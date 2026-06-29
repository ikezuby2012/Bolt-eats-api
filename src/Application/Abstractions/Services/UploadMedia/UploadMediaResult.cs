namespace Application.Abstractions.Services.UploadResult;

public record ImageUploadResult(
    bool IsSuccess,
    string? PublicId,
    string? Link,
    string? ThumbnailLink,
    string? Error = null);

public static class UploadFolders
{
    public const string Restaurants = "restaurants";
    public const string MenuItems = "menu_items";
    public const string Users = "users";
    public const string Reviews = "reviews";
    public const string Riders = "riders";
}
