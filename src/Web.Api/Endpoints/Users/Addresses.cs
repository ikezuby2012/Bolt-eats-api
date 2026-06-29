using Application.Abstractions.Messaging;
using Application.Users.CreateNewAddress;
using Application.Users.DeleteMyAddress;
using Application.Users.Dto;
using Application.Users.GetMyAddresses;
using Application.Users.SetAddressAsDefault;
using Application.Users.UpdateMyAddress;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

public class Addresses : IEndpoint
{
    internal sealed record CreateNewAddressRequest(
        string Label,
        string Street,
        string City,
        string State,
        string Country,
        string PostalCode,
        string LatitudeRaw,
        string LongitudeRaw,
        string? DeliveryInstructions,
        string? BuildingType,
        string? AddressLabel,
        Dictionary<string, string>? BuildingDetails,
        bool IsDefault
    ) : ICommand<AddressDto>;

    internal sealed record UpdateMyAddressRequest(
            string? Label,
            string? Street,
            string? City,
            string? State,
            string? Country,
            string? PostalCode,
            string? LatitudeRaw,
            string? LongitudeRaw,
            string? DeliveryInstructions,
            string? BuildingType,
            string? AddressLabel,
            Dictionary<string, string>? BuildingDetails,
            bool IsDefault
        );


    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/me/addresses", async (IQueryHandler<GetMyAddressesQuery, IEnumerable<AddressDto>> handler, CancellationToken cancellationToken) =>
        {
            var query = new GetMyAddressesQuery();

            Result<IEnumerable<AddressDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<IEnumerable<AddressDto>>.Success(value, "My Addresses")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Users).RequireAuthorization();

        app.MapPost("users/me/addresses", async ([FromBody] CreateNewAddressRequest body, ICommandHandler<CreateNewAddressCommand, AddressDto> handler, CancellationToken cancellationToken) =>
        {
            var command = new CreateNewAddressCommand(body.Label, body.Street, body.City, body.State, body.Country, body.PostalCode, body.LatitudeRaw, body.LongitudeRaw, body.DeliveryInstructions, body.BuildingType, body.AddressLabel, body.BuildingDetails, body.IsDefault);

            Result<AddressDto> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Created($"/user-addresses/{result.Value.Id}", ApiResponse<AddressDto>.Success(value, "Created new Address")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Users).RequireAuthorization();


        app.MapPut("users/me/addresses/{Id:Guid}", async (Guid Id, [FromBody] UpdateMyAddressRequest body, ICommandHandler<UpdateMyAddressCommand, AddressDto> handler, CancellationToken cancellationToken) =>
        {
            var command = new UpdateMyAddressCommand(Id, body.Label, body.Street, body.City, body.State, body.Country, body.PostalCode, body.LatitudeRaw, body.LongitudeRaw, body.DeliveryInstructions, body.BuildingType, body.AddressLabel, body.BuildingDetails, body.IsDefault);

            Result<AddressDto> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<AddressDto>.Success(value, "Address updated Successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Users).RequireAuthorization();

        app.MapDelete("users/me/addresses/{Id:Guid}", async (Guid Id, ICommandHandler<DeleteMyAddressCommand> handler, CancellationToken cancellationToken) =>
        {
            var command = new DeleteMyAddressCommand(Id);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(
                 () => Results.NoContent(),
                 error => CustomResults.Problem(error));
        }).WithTags(Tags.Users).RequireAuthorization();

        app.MapPut("users/me/addresses/{Id:Guid}/default", async (Guid Id, ICommandHandler<SetAddressAsDefaultCommand, AddressDto> handler, CancellationToken cancellationToken) =>
        {
            var command = new SetAddressAsDefaultCommand(Id);

            Result<AddressDto> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<AddressDto>.Success(value, "Address updated to default Successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Users).RequireAuthorization();

    }
}
