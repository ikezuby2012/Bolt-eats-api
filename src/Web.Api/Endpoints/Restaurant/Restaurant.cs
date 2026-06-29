using Application.Abstractions.Messaging;
using Application.Abstractions.Services.UploadMedia;
using Application.Abstractions.Services.UploadResult;
using Application.Restaurant.CreateRestaurant;
using Application.Restaurant.DeleteRestaurant;
using Application.Restaurant.Dto;
using Application.Restaurant.GetAllRestaurant;
using Application.Restaurant.GetFeaturedRestaurant;
using Application.Restaurant.GetNearbyRestaurant;
using Application.Restaurant.GetPopularRestaurants;
using Application.Restaurant.GetRestaurantById;
using Application.Restaurant.ToggleStatusRestaurant;
using Application.Restaurant.UpdateRestaurantInfo;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Restaurant;

public class Restaurant : IEndpoint
{
    internal sealed record ToggleStatusReq(bool IsOpen);

    internal sealed record CreateRestaurantRequest(
        string Name,
        string Description,
        string PhoneNumber,
        string? Email,
        string? LogoUrl,
        string? BannerUrl,
        decimal? DeliveryFeeMin,
        decimal? DeliveryFeeMax,
        decimal? MinOrderAmount,
        int? EstDeliveryMin,
        int? EstDeliveryMax,
        bool CompanyPartner,

        // Address
        string AddressStreet,
        string AddressCity,
        string AddressState,
        string AddressCountry,
        decimal AddressLat,
        decimal AddressLng,
        string? AddressPostalCode,

        double? Rating = 0
    );
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("restaurant", async ([AsParameters] GetAllRestaurantQuery param, IQueryHandler<GetAllRestaurantQuery, PaginatedResult<RestaurantDto>> handler, CancellationToken cancellationToken) =>
        {
            var query = new GetAllRestaurantQuery(param.PageSize, param.pageNumber, param.DateFrom, param.DateTo, param.IsActive);

            Result<PaginatedResult<RestaurantDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<PaginatedResult<RestaurantDto>>.Success(value, "All Restaurants")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Restaurant).RequireAuthorization();

        app.MapGet("restaurant/{Id:Guid}", async (Guid Id, IQueryHandler<GetRestaurantByIdQuery, RestaurantDto> handler, CancellationToken cancellationToken) =>
        {
            var query = new GetRestaurantByIdQuery(Id);

            Result<RestaurantDto> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<RestaurantDto>.Success(value, "Restaurant fetched successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Restaurant).RequireAuthorization();

        app.MapPost("restaurant", async (
            [FromForm] string name,
            [FromForm] string description,
            [FromForm] string phoneNumber,
            [FromForm] string? email,
            [FromForm] decimal? deliveryFeeMin,
            [FromForm] decimal? deliveryFeeMax,
            [FromForm] decimal? minOrderAmount,
            [FromForm] int? estDeliveryMin,
            [FromForm] int? estDeliveryMax,
            [FromForm] bool companyPartner,
            [FromForm] string addressStreet,
            [FromForm] string addressCity,
            [FromForm] string addressState,
            [FromForm] string addressCountry,
            [FromForm] decimal addressLat,
            [FromForm] decimal addressLng,
            [FromForm] string? addressPostalCode,
            IFormFile? logo,
            IFormFile? banner,
            IImageUploadService imageService,
            ICommandHandler<CreateRestaurantCommand, RestaurantDto> handler,
            CancellationToken cancellationToken) =>
        {
            string? logoUrl = null, logoPublicId = null;
            string? bannerUrl = null;
            //string? bannerPublicId = null;

            if (logo is not null)
            {
                await using Stream logoStream = logo.OpenReadStream();

                ImageUploadResult logoResult = await imageService.UploadAsync(
                                        stream: logoStream,
                                        fileName: logo.FileName,
                                        folder: UploadFolders.Restaurants,
                                        options: new ImageUploadOptions(
                                            MaxWidthPx: 400,
                                            MaxHeightPx: 400,
                                            Quality: 85,
                                            Format: "webp",
                                            GenerateThumbnail: true,
                                            ThumbnailSizePx: 100),
                                        cancellationToken: cancellationToken);

                if (!logoResult.IsSuccess)
                {
                    return Results.BadRequest(ApiResponse<string>.Error($"Logo upload failed: {logoResult.Error}"));
                }

                logoUrl = logoResult.Link;
                logoPublicId = logoResult.PublicId;
            }

            if (banner is not null)
            {
                await using Stream bannerStream = banner.OpenReadStream();
                ImageUploadResult bannerResult = await imageService.UploadAsync(
                    stream: bannerStream,
                    fileName: banner.FileName,
                    folder: UploadFolders.Restaurants,
                    options: new ImageUploadOptions(
                        MaxWidthPx: 1200,
                        MaxHeightPx: 400,
                        Quality: 80,
                        Format: "webp",
                        GenerateThumbnail: false),
                    cancellationToken: cancellationToken);

                if (!bannerResult.IsSuccess)
                {
                    if (logoPublicId is not null)
                    {
                        await imageService.DeleteAsync(logoPublicId, cancellationToken);
                    }

                    return Results.BadRequest(ApiResponse<string>.Error($"Banner upload failed: {bannerResult.Error}"));
                }

                bannerUrl = bannerResult.Link;
                //bannerPublicId = bannerResult.PublicId;
            }
            var command = new CreateRestaurantCommand(
                   Name: name,
                   Description: description,
                   PhoneNumber: phoneNumber,
                   Email: email,
                   LogoLink: logoUrl,
                   BannerLink: bannerUrl,
                   DeliveryFeeMin: deliveryFeeMin,
                   DeliveryFeeMax: deliveryFeeMax,
                   MinOrderAmount: minOrderAmount,
                   EstDeliveryMin: estDeliveryMin,
                   EstDeliveryMax: estDeliveryMax,
                   CompanyPartner: companyPartner,
                   Address: new CreateAddressRequest(
                       addressStreet,
                       addressCity,
                       addressState,
                       addressCountry,
                       addressLat,
                       addressLng,
                       addressPostalCode));

            Result<RestaurantDto> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Created($"restaurant/{value.Id}", ApiResponse<RestaurantDto>.Success(value, "Restaurant Created Successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Restaurant).RequireAuthorization().DisableAntiforgery();

        app.MapPost("restaurant-dump", async ([FromBody] CreateRestaurantRequest body, ICommandHandler<CreateRestaurantCommand, RestaurantDto> handler, CancellationToken cancellationToken) =>
        {
            var command = new CreateRestaurantCommand(
                body.Name,
                body.Description,
                body.PhoneNumber,
                body.Email,
                body.LogoUrl,
                body.BannerUrl,
                body.DeliveryFeeMin,
                body.DeliveryFeeMax,
                body.MinOrderAmount,
                body.EstDeliveryMin,
                body.EstDeliveryMax,
                body.CompanyPartner,
                new CreateAddressRequest(
                    body.AddressStreet,
                    body.AddressCity,
                    body.AddressState,
                    body.AddressCountry,
                    body.AddressLat,
                    body.AddressLng,
                    body.AddressPostalCode),
                body.Rating);

            Result<RestaurantDto> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<RestaurantDto>.Success(value, "Restaurant Created Successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Restaurant).RequireAuthorization().ExcludeFromDescription();

        app.MapPatch("restaurant/{Id:guid}", async (Guid Id, [FromBody] UpdateRestaurantCommand body, ICommandHandler<UpdateRestaurantCommand, RestaurantDto> handler, CancellationToken cancellationToken) =>
        {
            var command = new UpdateRestaurantCommand(Id, body.Name, body.Description, body.PhoneNumber, body.Email, body.LogoLink, body.BannerLink, body.DeliveryFeeMin, body.DeliveryFeeMax, body.MinOrderAmount, body.EstDeliveryMin, body.EstDeliveryMax, body.CompanyPartner, body.Address);

            Result<RestaurantDto> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<RestaurantDto>.Success(value, "Restaurant Updated Successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Restaurant).RequireAuthorization();

        app.MapDelete("restaurant/{Id:guid}", async (Guid Id, ICommandHandler<DeleteRestaurantCommand> handler, CancellationToken cancellationToken) =>
        {
            var command = new DeleteRestaurantCommand(Id);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(
                 () => Results.NoContent(),
                 error => CustomResults.Problem(error));
        }).WithTags(Tags.Restaurant).RequireAuthorization();

        app.MapGet("restaurants/nearby", async ([AsParameters] GetNearbyRestaurantQuery param, IQueryHandler<GetNearbyRestaurantQuery, PaginatedResult<RestaurantDto>> handler, CancellationToken cancellationToken) =>
        {
            var query = new GetNearbyRestaurantQuery(param.lat, param.lng, param.RadiusKm, param.PageSize, param.pageNumber);

            Result<PaginatedResult<RestaurantDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<PaginatedResult<RestaurantDto>>.Success(value, "All Nearby Restaurants")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Restaurant).RequireAuthorization().WithName("GetNearbyRestaurants");

        app.MapGet("restaurants/featured", async ([AsParameters] GetFeaturedRestaurantQuery param, IQueryHandler<GetFeaturedRestaurantQuery, PaginatedResult<RestaurantDto>> handler, CancellationToken cancellationToken) =>
        {
            var query = new GetFeaturedRestaurantQuery();

            Result<PaginatedResult<RestaurantDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<PaginatedResult<RestaurantDto>>.Success(value, "All Featured Restaurants")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Restaurant).RequireAuthorization().WithName("GetFeaturedRestaurants");

        app.MapPatch("restaurant/{Id:guid}/toggle-status", async (Guid Id, [FromBody] ToggleStatusReq body, ICommandHandler<ToggleStatusRestaurantCommand, RestaurantDto> handler, CancellationToken cancellationToken) =>
        {
            var command = new ToggleStatusRestaurantCommand(Id, body.IsOpen);

            Result<RestaurantDto> result = await handler.Handle(command, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<RestaurantDto>.Success(value, "Restaurant Toggle Status Updated Successfully")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Restaurant).RequireAuthorization();

        app.MapGet("restaurant/popular", async (IQueryHandler<GetPopularRestaurantsQuery, IReadOnlyList<RestaurantDto>> handler, CancellationToken cancellationToken, [FromQuery] double rating = 4, [FromQuery] int limit = 10) =>
        {
            var query = new GetPopularRestaurantsQuery(rating, limit);

            Result<IReadOnlyList<RestaurantDto>> result = await handler.Handle(query, cancellationToken);

            return result.Match(value => Results.Ok(ApiResponse<IReadOnlyList<RestaurantDto>>.Success(value, "All Popular Restaurants")), error => CustomResults.Problem(error));
        }).WithTags(Tags.Restaurant).WithName("GetPopularRestaurants");
    }
}
