using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Rider;

public record RiderLocationUpdatedPayload(
    Guid RiderId,
    Guid OrderId,
    double Latitude,
    double Longitude,
    double? Bearing,
    double? Speed,
    DateTime RecordedAt);

public record OrderStatusChanged(
    Guid OrderId,
    string OldStatus,
    string NewStatus,
    DateTime ChangedAt);
