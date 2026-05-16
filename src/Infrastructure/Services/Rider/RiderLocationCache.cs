using System.Text.Json;
using Application.Abstractions.Services.Rider;
using Application.Tracking.Dto;
using StackExchange.Redis;

namespace Infrastructure.Services.Rider;

internal sealed class RiderLocationCache(IConnectionMultiplexer redis) : IRiderLocationCache
{
    private const string GeoKey = "riders:locations";
    private const string StatusPrefix = "riders:status:";
    private const string MetaPrefix = "riders:meta:";
    private const string LoadPrefix = "riders:load:";

    private static readonly TimeSpan AvailableTtl = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan BusyTtl = TimeSpan.FromHours(8);

    private IDatabase Db => redis.GetDatabase();

    public Task DecrementLoadAsync(Guid riderId, CancellationToken cancellationToken = default) => Db.StringDecrementAsync($"{LoadPrefix}{riderId}");

    public async Task<RiderMeta?> GetMetaAsync(Guid riderId, CancellationToken cancellationToken = default)
    {
        RedisValue val = await Db.StringGetAsync($"{MetaPrefix}{riderId}");
        return val.HasValue
            ? JsonSerializer.Deserialize<RiderMeta>(val!.ToString())
            : null;
    }

    public async Task<IReadOnlyList<NearbyRider>> GetNearbyRidersAsync(double latitude, double longitude, double radiusKm, int maxResults = 10, CancellationToken cancellationToken = default)
    {
        GeoRadiusResult[]? results = await Db.GeoRadiusAsync(
           GeoKey,
           longitude,          // Redis GEO: longitude first
           latitude,
           radiusKm,
           GeoUnit.Kilometers,
           maxResults,
           Order.Ascending,    // nearest first
           GeoRadiusOptions.WithCoordinates | GeoRadiusOptions.WithDistance);

        if (results is null)
        {
            return [];
        }


        return results
            .Where(r => r.Member.HasValue)
            .Select(r => new NearbyRider(
                RiderId: Guid.Parse(r.Member!.ToString()),
                Latitude: r.Position?.Latitude ?? 0,
                Longitude: r.Position?.Longitude ?? 0,
                StraightLineDistanceKm: r.Distance ?? 0))
            .ToList();
    }

    public async Task<RiderAvailability?> GetStatusAsync(Guid riderId, CancellationToken cancellationToken = default)
    {
        RedisValue val = await Db.StringGetAsync($"{StatusPrefix}{riderId}");
        if (!val.HasValue)
        {
            return null;   // TTL expired = offline
        }

        return val.ToString() switch
        {
            "available" => RiderAvailability.Available,
            "busy" => RiderAvailability.Busy,
            _ => RiderAvailability.Offline
        };
    }

    public Task IncrementLoadAsync(Guid riderId, CancellationToken cancellationToken = default) => Db.StringIncrementAsync($"{LoadPrefix}{riderId}");

    public async Task SetStatusAsync(Guid riderId, RiderAvailability status, CancellationToken cancellationToken = default)
    {
        await Db.StringSetAsync(
             $"{StatusPrefix}{riderId}",
             status.ToString().ToLower(System.Globalization.CultureInfo.CurrentCulture),
             status == RiderAvailability.Available ? AvailableTtl : BusyTtl);
    }

    public async Task UpdateLocationAsync(Guid riderId, double latitude, double longitude, double? heading, double? speed, CancellationToken cancellationToken = default)
    {
        string key = riderId.ToString();

        // GEOADD takes (longitude, latitude) — note the order
        await Db.GeoAddAsync(GeoKey, new GeoEntry(longitude, latitude, key));

        int load = await GetLoadInternalAsync(riderId);
        var meta = new RiderMeta(heading, speed, null, DateTime.UtcNow, load);

        await Db.StringSetAsync(
            $"{MetaPrefix}{key}",
            JsonSerializer.Serialize(meta),
            AvailableTtl);
    }

    private async Task<int> GetLoadInternalAsync(Guid riderId)
    {
        RedisValue val = await Db.StringGetAsync($"{LoadPrefix}{riderId}");
        return val.HasValue ? (int)(long)val : 0;
    }
}
