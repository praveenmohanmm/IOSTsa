namespace iOSTSA.Services;

/// <summary>
/// Triggers haptic feedback on the iPhone when a signal alert fires.
/// On iPhone, this uses UINotificationFeedbackGenerator (warning pattern).
/// Apple Watch mirrors notification haptics from the paired iPhone —
/// when the iPhone fires a warning haptic, the Watch vibrates on the wrist.
/// </summary>
public interface IHapticAlertService
{
    /// <summary>
    /// Play the alert haptic. Safe to call from any thread.
    /// </summary>
    void Alert();
}
