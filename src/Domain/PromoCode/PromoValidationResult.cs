namespace Domain.PromoCode;

public record PromoValidationResult(bool IsValid, string? Reason, decimal? DiscountValue, string? DiscountType);
