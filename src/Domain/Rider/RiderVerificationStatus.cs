using System;
using System.Collections.Generic;
using System.Text;
using Domain.Payment;
using SharedKernel;

namespace Domain.Rider;

public sealed class RiderVerificationStatus : Enumeration<RiderVerificationStatus>
{
    public static readonly RiderVerificationStatus Pending = new(1, "Pending");
    public static readonly RiderVerificationStatus UnderReview = new(2, "Under Review");
    public static readonly RiderVerificationStatus Verified = new(3, "Succeeded");
    public static readonly RiderVerificationStatus Rejected = new(4, "Rejected");

    private RiderVerificationStatus(int Id, string name) : base(Id, name) { }
}
