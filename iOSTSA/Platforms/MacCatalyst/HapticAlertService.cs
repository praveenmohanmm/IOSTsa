using iOSTSA.Services;

namespace iOSTSA.Platforms.iOS;

/// <summary>
/// Mac Catalyst stub — no haptic hardware on Mac.
/// </summary>
public class HapticAlertService : IHapticAlertService
{
    public void Alert() { /* no Taptic Engine on Mac */ }
}
