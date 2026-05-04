using System.ComponentModel.DataAnnotations;
using Application.Abstractions.Messaging;
using Application.Reviews.Dto;
using Application.Reviews.EditReview;
using Application.Reviews.GetReviewById;
using Application.Reviews.RemoveReview;
using Application.Reviews.SubmitReview;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Review;

public class Review : IEndpoint
{
    internal sealed record SubmitReviewRequest([Required] Guid OrderId, int Rating, string Comment);
    internal sealed record EditReviewRequest(int Rating, string Comment);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("review", async ([FromBody] SubmitReviewRequest body, ICommandHandler<SubmitReviewCommand, ReviewDto> handler, CancellationToken cancellationToken) =>
        {
            var command = new SubmitReviewCommand(body.OrderId, body.Rating, body.Comment);

            Result<ReviewDto> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<ReviewDto>.Success(value, "Review Submitted Successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Review).RequireAuthorization();

        app.MapGet("review/{Id:Guid}", async (Guid Id, IQueryHandler<GetReviewByIdQuery, ReviewDto> handler, CancellationToken cancellationToken) =>
        {
            var query = new GetReviewByIdQuery(Id);

            Result<ReviewDto> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<ReviewDto>.Success(value, "Review fetched successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Review).RequireAuthorization();

        app.MapPatch("review/{Id:Guid}", async (Guid Id, [FromBody] EditReviewRequest body, ICommandHandler<EditReviewCommand, ReviewDto> handler, CancellationToken cancellationToken) =>
        {
            var query = new EditReviewCommand(Id, body.Rating, body.Comment);

            Result<ReviewDto> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<ReviewDto>.Success(value, "Review Updated successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Review).RequireAuthorization();

        app.MapDelete("review/{Id:Guid}", async (Guid Id, ICommandHandler<RemoveReviewCommand> handler, CancellationToken cancellationToken) =>
        {
            var query = new RemoveReviewCommand(Id);

            Result result = await handler.Handle(query, cancellationToken);

            return result.Match(
                 () => Results.NoContent(),
                 error => CustomResults.Problem(error));
        }).WithTags(Tags.Review).RequireAuthorization();
    }
}
