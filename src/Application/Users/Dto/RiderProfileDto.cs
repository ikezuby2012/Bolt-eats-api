using System.Text.Json.Serialization;
using SharedKernel;

namespace Application.Users.Dto;

public class RiderProfileDto : IAuditable<Guid>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string NumberPlate { get; set; }
    public string VehicleType { get; set; }
    public string VehicleMake { get; set; }
    public string VehicleModel { get; set; }
    public string VehicleColor { get; set; }
    public string VehicleYear { get; set; }
    public string? DriverLicenseUrl { get; set; }
    public string? DriverLicenseId { get; set; }
    public string? NationalIdUrl { get; set; }
    public string? NationalIdId { get; set; }
    public string? VehiclePhotoUrl { get; set; }
    public string? VehiclePhotoId { get; set; }
    public string? InsuranceCertUrl { get; set; }
    public string? InsuranceCertId { get; set; }
    public int StatusId { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public Guid? VerifiedBy { get; set; }

    // IAuditable<Guid>
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    [JsonIgnore]
    public bool IsSoftDeleted { get; set; }

    public static explicit operator RiderProfileDto(Domain.Rider.RiderProfile profile) => new RiderProfileDto
    {
        Id = profile.Id,
        UserId = profile.UserId,
        NumberPlate = profile.NumberPlate,
        VehicleType = profile.VehicleType,
        VehicleMake = profile.VehicleMake,
        VehicleModel = profile.VehicleModel,
        VehicleColor = profile.VehicleColor,
        VehicleYear = profile.VehicleYear,
        DriverLicenseUrl = profile.DriverLicenseUrl,
        DriverLicenseId = profile.DriverLicenseId,
        NationalIdUrl = profile.NationalIdUrl,
        NationalIdId = profile.NationalIdId,
        VehiclePhotoUrl = profile.VehiclePhotoUrl,
        VehiclePhotoId = profile.VehiclePhotoId,
        InsuranceCertUrl = profile.InsuranceCertUrl,
        InsuranceCertId = profile.InsuranceCertId,
        StatusId = profile.StatusId,
        RejectionReason = profile.RejectionReason,
        VerifiedAt = profile.VerifiedAt,
        VerifiedBy = profile.VerifiedBy,
        CreatedAt = profile.CreatedAt,
        CreatedBy = profile.CreatedBy,
        UpdatedAt = profile.UpdatedAt,
        UpdatedBy = profile.UpdatedBy,
        IsSoftDeleted = profile.IsSoftDeleted
    };
}

public sealed record NearbyRiderDto(
    Guid RiderId,
    string FullName,
    string? ProfileImgLink,
    double Latitude,
    double Longitude,
    double DistanceKm,
    double? Heading,
    double? Speed,
    int ActiveOrders,
    double Rating,
    string VehicleType,
    string NumberPlate,
    string VehicleColor,
    bool IsOnline);
