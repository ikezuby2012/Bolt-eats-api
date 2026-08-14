using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services.UploadMedia;
using Application.Tracking.UpdateRiderLocation;
using Application.Users.Dto;
using Application.Users.GetNearbyRiders;
using Application.Users.ReviewRiderProfileCommand;
using Application.Users.SubmitRiderProfile;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

public class Rider : IEndpoint
{
    public const string RiderDocuments = "rider_documents";

    public sealed record SubmitRiderProfileRequest(
    [FromForm] string NumberPlate,
    [FromForm] string VehicleType,
    [FromForm] string VehicleMake,
    [FromForm] string VehicleModel,
    [FromForm] string VehicleColor,
    [FromForm] string VehicleYear,
    IFormFile? DriverLicense,
    IFormFile? NationalId,
    IFormFile? VehiclePhoto,
    IFormFile? InsuranceCert);

    // Requests/ReviewRiderProfileRequest.cs
    public sealed record ReviewRiderProfileRequest(
        bool Approved,
        string? RejectionReason);

    public record UpdateRiderLocationRequest(double Latitude, double Longitude, double? Heading, double? Speed);


    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("rider/profile", async (
             [FromForm] SubmitRiderProfileRequest body,
             ICommandHandler<SubmitRiderProfileCommand, RiderProfileDto> handler,
             IImageUploadService imageService,
             IUserContext userContext,
             CancellationToken cancellationToken) =>
        {
            Guid userId = userContext.UserId;

            // ── Upload documents ──────────────────────────────────────────
            string? licenseUrl = null, licenseId = null;
            string? nationalIdUrl = null, nationalId = null;
            string? vehiclePhotoUrl = null, vehiclePhotoId = null;
            string? insuranceUrl = null, insuranceId = null;

            var docOptions = new ImageUploadOptions(
                MaxWidthPx: 1200,
                MaxHeightPx: 1200,
                Quality: 85,
                Format: "webp",
                GenerateThumbnail: false);

            if (body.DriverLicense is not null)
            {
                await using Stream stream = body.DriverLicense.OpenReadStream();
                Application.Abstractions.Services.UploadResult.ImageUploadResult result = await imageService.UploadAsync(
                    stream: stream,
                    fileName: body.DriverLicense.FileName,
                    folder: RiderDocuments,
                    options: docOptions,
                    cancellationToken: cancellationToken);

                if (!result.IsSuccess)
                {
                    return Results.BadRequest(
                       ApiResponse<string>.Error(
                           $"Driver license upload failed: {result.Error}", 404));
                }


                licenseUrl = result.Link;
                licenseId = result.PublicId;
            }

            if (body.NationalId is not null)
            {
                await using Stream stream = body.NationalId.OpenReadStream();
                Application.Abstractions.Services.UploadResult.ImageUploadResult result = await imageService.UploadAsync(
                    stream: stream,
                    fileName: body.NationalId.FileName,
                    folder: RiderDocuments,
                    options: docOptions,
                    cancellationToken: cancellationToken);

                if (!result.IsSuccess)
                {
                    if (licenseId is not null)
                    {
                        await imageService.DeleteAsync(licenseId, cancellationToken);
                    }


                    return Results.BadRequest(
                        ApiResponse<string>.Error(
                            $"National ID upload failed: {result.Error}"));
                }

                nationalIdUrl = result.Link;
                nationalId = result.PublicId;
            }

            if (body.VehiclePhoto is not null)
            {
                await using Stream stream = body.VehiclePhoto.OpenReadStream();
                Application.Abstractions.Services.UploadResult.ImageUploadResult result = await imageService.UploadAsync(
                    stream: stream,
                    fileName: body.VehiclePhoto.FileName,
                    folder: RiderDocuments,
                    options: docOptions,
                    cancellationToken: cancellationToken);

                if (!result.IsSuccess)
                {
                    if (licenseId is not null)
                    {
                        await imageService.DeleteAsync(licenseId, cancellationToken);
                    }

                    if (nationalId is not null)
                    {
                        await imageService.DeleteAsync(nationalId, cancellationToken);
                    }

                    return Results.BadRequest(
                        ApiResponse<string>.Error(
                            $"Vehicle photo upload failed: {result.Error}"));
                }

                vehiclePhotoUrl = result.Link;
                vehiclePhotoId = result.PublicId;
            }

            if (body.InsuranceCert is not null)
            {
                await using Stream stream = body.InsuranceCert.OpenReadStream();
                Application.Abstractions.Services.UploadResult.ImageUploadResult result = await imageService.UploadAsync(
                    stream: stream,
                    fileName: body.InsuranceCert.FileName,
                    folder: RiderDocuments,
                    options: docOptions,
                    cancellationToken: cancellationToken);

                if (!result.IsSuccess)
                {
                    if (licenseId is not null)
                    {
                        await imageService.DeleteAsync(licenseId, cancellationToken);
                    }
                    if (nationalId is not null)
                    {
                        await imageService.DeleteAsync(nationalId, cancellationToken);
                    }
                    if (vehiclePhotoId is not null)
                    { await imageService.DeleteAsync(vehiclePhotoId, cancellationToken); }


                    return Results.BadRequest(
                        ApiResponse<string>.Error(
                            $"Insurance certificate upload failed: {result.Error}"));
                }

                insuranceUrl = result.Link;
                insuranceId = result.PublicId;
            }

            // ── Dispatch command ──────────────────────────────────────────
            var command = new SubmitRiderProfileCommand(
                UserId: userId,
                NumberPlate: body.NumberPlate,
                VehicleType: body.VehicleType,
                VehicleMake: body.VehicleMake,
                VehicleModel: body.VehicleModel,
                VehicleColor: body.VehicleColor,
                VehicleYear: body.VehicleYear,
                DriverLicenseLink: licenseUrl,
                NationalIdLink: nationalIdUrl,
                VehiclePhotoLink: vehiclePhotoUrl,
                InsuranceCertLink: insuranceUrl);

            Result<RiderProfileDto> commandResult = await handler.Handle(command, cancellationToken);

            // ── Clean up uploads if command failed ────────────────────────
            if (commandResult.IsFailure)
            {
                if (licenseId is not null)
                {
                    await imageService.DeleteAsync(licenseId, cancellationToken);
                }
                if (nationalId is not null)
                { await imageService.DeleteAsync(nationalId, cancellationToken); }

                if (vehiclePhotoId is not null)
                { await imageService.DeleteAsync(vehiclePhotoId, cancellationToken); }

                if (insuranceId is not null)
                { await imageService.DeleteAsync(insuranceId, cancellationToken); }

            }

            return commandResult.Match(
                value => Results.Ok(ApiResponse<RiderProfileDto>.Success(
                    value, "Profile submitted successfully. Pending verification.")),
                error => CustomResults.Problem(error));
        })
         .WithTags(Tags.Users)
         .RequireAuthorization("Rider")
         .DisableAntiforgery()
         .Accepts<SubmitRiderProfileRequest>("multipart/form-data")
         .Produces<ApiResponse<RiderProfileDto>>()
         .Produces<ProblemDetails>(400);

        app.MapPut("rider/profile/{id:guid}/review", async (
          Guid id,
          [FromBody] ReviewRiderProfileRequest body,
          ICommandHandler<ReviewRiderProfileCommand, RiderProfileDto> handler,
          IUserContext userContext,
          CancellationToken cancellationToken) =>
        {
            var command = new ReviewRiderProfileCommand(
                RiderProfileId: id,
                Approved: body.Approved,
                RejectionReason: body.RejectionReason,
                ReviewedBy: userContext.UserId);

            Result<RiderProfileDto> result = await handler.Handle(command, cancellationToken);

            return result.Match(
                value => Results.Ok(ApiResponse<RiderProfileDto>.Success(
                    value, body.Approved
                        ? "Rider profile approved."
                        : "Rider profile rejected.")),
                error => CustomResults.Problem(error));
        })
      .WithTags(Tags.Users)
      .RequireAuthorization("Admin")
      .Produces<ApiResponse<RiderProfileDto>>()
      .Produces<ProblemDetails>(400)
      .Produces<ProblemDetails>(404);


        app.MapPut("rider/location", async (UpdateRiderLocationRequest body, IUserContext ctx, ICommandHandler<UpdateRiderLocationCommand> handler, CancellationToken ct) =>
        {
            Result result = await handler.Handle(
                new UpdateRiderLocationCommand(
                    ctx.UserId,
                    body.Latitude,
                    body.Longitude,
                    body.Heading,
                    body.Speed), ct);

            return result.Match(
                () => Results.NoContent(),
                error => CustomResults.Problem(error));
        })
            .WithName("UpdateRiderLocation")
            .WithTags(Tags.Users)
            .RequireAuthorization("Rider")
            .Produces(204);

        // GET /rider/nearby?lat=4.8156&lng=7.0498&radiusKm=5&limit=20
        app.MapGet("rider/nearby", async (
            IQueryHandler<GetNearbyRidersQuery, IReadOnlyList<NearbyRiderDto>> handler,
            CancellationToken ct,
            [FromQuery] double lat = 0,
            [FromQuery] double lng = 0,
            [FromQuery] double radiusKm = 5,
            [FromQuery] int limit = 20) =>
        {
            if (lat < -90 || lat > 90 || lng < -180 || lng > 180)
            {
                return Results.BadRequest(
                    ApiResponse<string>.Error("Invalid latitude or longitude."));
            }


            if (radiusKm is <= 0 or > 50)
            {
                return Results.BadRequest(
                    ApiResponse<string>.Error("radiusKm must be between 1 and 50."));
            }

            Result<IReadOnlyList<NearbyRiderDto>> result = await handler.Handle(
                new GetNearbyRidersQuery(lat, lng, radiusKm, limit), ct);

            return result.Match(
                value => Results.Ok(
                    ApiResponse<IReadOnlyList<NearbyRiderDto>>.Success(value)),
                error => CustomResults.Problem(error));
        })
        .WithName("GetNearbyRiders")
        .WithTags(Tags.Users)
        .RequireAuthorization()   // Admin only — exposes rider positions
        .Produces<ApiResponse<IReadOnlyList<NearbyRiderDto>>>();
    }
}
