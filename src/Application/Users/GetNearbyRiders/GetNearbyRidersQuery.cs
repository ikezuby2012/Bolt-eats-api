using Application.Abstractions.Messaging;
using Application.Users.Dto;

namespace Application.Users.GetNearbyRiders;

public sealed record GetNearbyRidersQuery(
    double Lat,
    double Lng,
    double RadiusKm = 5,
    int Limit = 20)
    : IQuery<IReadOnlyList<NearbyRiderDto>>;
