using Domain.Users;
using SharedKernel;

namespace Domain.Rider;

public class RiderProfile : Auditable<Guid>
{
    public Guid UserId { get; set; }
    public User User { get; set; }
    public string NumberPlate { get; set; }
    public string VehicleType { get; set; }
    public string VehicleMake { get; set; }
    public string VehicleModel { get; set; }
    public string VehicleColor { get; set; }
    public string VehicleYear { get; set; }
    public string? DriverLicenseUrl { get; set; }   // Cloudinary public URL
    public string? DriverLicenseId { get; set; }   // Cloudinary public_id
    public string? NationalIdUrl { get; set; }
    public string? NationalIdId { get; set; }
    public string? VehiclePhotoUrl { get; set; }
    public string? VehiclePhotoId { get; set; }
    public string? InsuranceCertUrl { get; set; }
    public string? InsuranceCertId { get; set; }
    public int StatusId { get; set; }
    public RiderVerificationStatus Status { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public Guid? VerifiedBy { get; set; }
}
