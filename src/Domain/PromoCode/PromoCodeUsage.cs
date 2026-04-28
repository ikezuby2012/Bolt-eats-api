using Domain.Users;
using SharedKernel;

namespace Domain.PromoCode;

public class PromoCodeUsage : Auditable<Guid>
{
    public Guid PromoCodeId { get; set; }
    public PromoCode PromoCode { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; }

    public int StatusId { get; set; }
    public PromoUsageStatus Status { get; set; }
    public decimal DiscountApplied { get; set; }
    public DateTime? RedeemedAt { get; set; }
    public int TimesUsed { get; set; }
}
