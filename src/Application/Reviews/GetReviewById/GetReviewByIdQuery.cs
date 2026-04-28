using Application.Abstractions.Messaging;
using Application.Reviews.Dto;

namespace Application.Reviews.GetReviewById;
public sealed record GetReviewByIdQuery(Guid Id) : IQuery<ReviewDto>;
