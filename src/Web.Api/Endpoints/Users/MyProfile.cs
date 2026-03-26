using Application.Abstractions.Messaging;
using Application.Users.DeactivateMyProfile;
using Application.Users.Dto;
using Application.Users.GetMyProfile;
using Application.Users.UpdateMyProfile;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class MyProfile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/me", async (IQueryHandler<GetMyProfileQuery, UserDto> handler, CancellationToken cancellationToken) =>
        {
            var query = new GetMyProfileQuery();

            Result<UserDto> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<UserDto>.Success(value, "My Profile")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Users).RequireAuthorization();


        app.MapPatch("users/me", async ([AsParameters] UpdateMyProfileCommand _params, ICommandHandler<UpdateMyProfileCommand, UserDto> handler, CancellationToken cancellationToken) =>
        {
            var command = new UpdateMyProfileCommand(_params.firstName, _params.lastName, _params.phoneNumber, _params.dateOfBirth);

            Result<UserDto> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<UserDto>.Success(value, "My Profile")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Users).RequireAuthorization().WithName("UpdateMyProfile");


        app.MapDelete("users/me", async (ICommandHandler<DeactivateMyProfileCommand> handler, CancellationToken cancellationToken) =>
        {
            var command = new DeactivateMyProfileCommand();

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(
                () => Results.NoContent(),
                error => CustomResults.Problem(error));
        }).WithTags(Tags.Users).RequireAuthorization().WithName("DeactivateMyProfile");
    }
}
