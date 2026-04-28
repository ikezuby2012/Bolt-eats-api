using System.Text.Json.Serialization;
using SharedKernel;

namespace Application.Reviews.Dto;

public class ReviewDto : IAuditable<Guid>
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public Guid RestaurantId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    [JsonIgnore]
    public bool IsSoftDeleted { get; set; }


    public static explicit operator ReviewDto(Domain.Review.Review review) => new ReviewDto
    {
        Id = review.Id,
        OrderId = review.OrderId,
        UserId = review.UserId,
        RestaurantId = review.RestaurantId,
        Rating = review.Rating,
        Comment = review.Comment,
        CreatedAt = review.CreatedAt,
        CreatedBy = review.CreatedBy,
        UpdatedAt = review.UpdatedAt,
        UpdatedBy = review.UpdatedBy,
    };
}

