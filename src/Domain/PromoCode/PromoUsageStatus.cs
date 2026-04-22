using Domain.Users;
using SharedKernel;

namespace Domain.PromoCode;

public sealed class PromoUsageStatus : Enumeration<PromoUsageStatus>
{
    public static readonly PromoUsageStatus Pending = new(1, "Pending");
    public static readonly PromoUsageStatus Redeemed = new(2, "Redeemed");
    public static readonly PromoUsageStatus Cancelled = new(3, "Cancelled");

    private PromoUsageStatus(int Id, string name) : base(Id, name) { }
}
