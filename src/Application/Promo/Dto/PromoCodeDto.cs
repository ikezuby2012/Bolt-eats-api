using System.Text.Json.Serialization;
using Application.Restaurant.Dto;
using SharedKernel;

namespace Application.Promo.Dto;

public class PromoCodeDto : IAuditable<Guid>
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string? Description { get; set; }
    public string? PromoDiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscount { get; set; }
    public decimal? MinOrderValue { get; set; }
    public decimal? MaxDiscountCap { get; set; }
    public int? UsageLimit { get; set; }
    public int? UsageLimitPerUser { get; set; }
    public int UsageCount { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public Guid? RestaurantId { get; set; }
    public RestaurantDto? Restaurant { get; set; }
    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    [JsonIgnore]
    public bool IsSoftDeleted { get; set; }

    public static explicit operator PromoCodeDto(Domain.PromoCode.PromoCode code) => new PromoCodeDto
    {
        Id = code.Id,
        Code = code.Code,
        Description = code.Description,
        PromoDiscountType = code.DiscountType,
        DiscountValue = code.DiscountValue,
        MaxDiscount = code.MaxDiscount,
        MinOrderValue = code.MinOrderValue,
        MaxDiscountCap = code.MaxDiscountCap,
        UsageLimit = code.UsageLimit,
        UsageLimitPerUser = code.UsageLimitPerUser,
        UsageCount = code.UsageCount,
        StartsAt = code.StartsAt,
        ExpiresAt = code.ExpiresAt,
        RestaurantId = code.RestaurantId,
        Restaurant = code.Restaurant is not null ? (RestaurantDto)code.Restaurant : null,
        IsActive = code.IsActive,
        CreatedAt = code.CreatedAt,
        CreatedBy = code.CreatedBy,
        UpdatedAt = code.UpdatedAt,
        UpdatedBy = code.UpdatedBy
    };
}

public record PromoValidationResultDto(
    bool IsValid,
    string? Reason,
    string? Code,
    string? DiscountType,
    decimal? DiscountValue,
    decimal? ResolvedDiscount);
