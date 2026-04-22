using SharedKernel;

namespace Domain.PromoCode;

public sealed class PromoCode : Auditable<Guid>
{
    public string Code { get; set; }
    public string Description { get; set; }
    public string DiscountType { get; set; } // fixed or percentage
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
    public Restaurant.Restaurant? Restaurant { get; set; }
    public bool IsActive { get; set; }

    public ICollection<PromoCodeUsage> Usages { get; set; } = [];
}
