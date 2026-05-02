namespace iOSTSA.Services;

/// <summary>
/// Keeps the screen on while tracking so the OS doesn't suspend GPS.
/// Implemented per-platform; no-op on unsupported platforms.
/// </summary>
public interface IScreenWakeLock
{
    void Enable();
    void Disable();
}
