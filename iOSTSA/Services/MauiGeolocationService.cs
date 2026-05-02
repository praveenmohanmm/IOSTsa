namespace iOSTSA.Services;

public record LocationResult(double Latitude, double Longitude, double Accuracy, double Speed);

/// <summary>
/// Native GPS service using Microsoft.Maui.Devices.Sensors.Geolocation.
/// Polls at 2 Hz (500 ms) and falls back to Haversine speed when the device
/// doesn't report speed directly — mirrors the web app's geolocation.js logic.
/// </summary>
public class MauiGeolocationService : IAsyncDisposable
{
    public event Action<LocationResult>? LocationChanged;
    public event Action<string>?         ErrorOccurred;

    public bool IsTracking { get; private set; }

    // Speed computation fallback
    private double? _prevLat, _prevLon;
    private DateTime? _prevTime;

    private CancellationTokenSource? _cts;

    // ── Public API ────────────────────────────────────────────────────────

    public async Task StartAsync()
    {
        // Request permission first
        var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            ErrorOccurred?.Invoke(
                "Location permission was denied. Please allow access in Settings → Privacy → Location.");
            return;
        }

        _prevLat = _prevLon = null;
        _prevTime = null;
        _cts = new CancellationTokenSource();
        IsTracking = true;

        // Fire-and-forget poll loop — errors surface via ErrorOccurred event
        _ = PollLoopAsync(_cts.Token);
    }

    public Task StopAsync()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        IsTracking = false;
        return Task.CompletedTask;
    }

    // ── Poll loop ─────────────────────────────────────────────────────────

    private async Task PollLoopAsync(CancellationToken ct)
    {
        var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(5));

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var location = await Geolocation.Default.GetLocationAsync(request, ct);

                if (location is not null)
                {
                    var speed = ResolveSpeed(location);
                    LocationChanged?.Invoke(new LocationResult(
                        location.Latitude,
                        location.Longitude,
                        location.Accuracy ?? 0,
                        speed
                    ));
                }
            }
            catch (FeatureNotSupportedException)
            {
                ErrorOccurred?.Invoke("GPS is not supported on this device.");
                break;
            }
            catch (PermissionException)
            {
                ErrorOccurred?.Invoke("Location permission was denied.");
                break;
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                // Transient error — log and keep trying
                System.Diagnostics.Debug.WriteLine($"[TSA Geo] error: {ex.Message}");
            }

            try { await Task.Delay(500, ct); }
            catch (OperationCanceledException) { break; }
        }

        IsTracking = false;
    }

    // ── Speed resolution (mirrors resolveSpeed in geolocation.js) ─────────

    private double ResolveSpeed(Location location)
    {
        double speed = location.Speed ?? -1;

        if (speed < 0)
        {
            var now = DateTime.UtcNow;
            if (_prevLat.HasValue && _prevTime.HasValue)
            {
                var dt = (now - _prevTime.Value).TotalSeconds;
                speed = dt > 0
                    ? TrafficSignalService.HaversineMetres(
                        _prevLat.Value, _prevLon!.Value,
                        location.Latitude, location.Longitude) / dt
                    : 0;
            }
            else
            {
                speed = 0;
            }
        }

        _prevLat  = location.Latitude;
        _prevLon  = location.Longitude;
        _prevTime = DateTime.UtcNow;
        return speed;
    }

    // ── Cleanup ───────────────────────────────────────────────────────────

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}
