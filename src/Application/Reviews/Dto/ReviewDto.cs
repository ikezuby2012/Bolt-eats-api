namespace Application.Reviews.Dto;

public class ReviewDto
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public Guid RestaurantId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }

    public static explicit operator ReviewDto(Domain.Review.Review review) => new ReviewDto
    {
        OrderId = review.OrderId,
        UserId = review.UserId,
        RestaurantId = review.RestaurantId,
        Rating = review.Rating,
        Comment = review.Comment,
    };
}
