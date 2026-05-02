using iOSTSA.Services;
using UIKit;

namespace iOSTSA.Platforms.iOS;

/// <summary>
/// iOS implementation of IHapticAlertService.
///
/// Uses UINotificationFeedbackGenerator with .Warning feedback type.
/// This is the same haptic pattern iOS uses for notifications and alerts,
/// which Apple Watch automatically mirrors via the "Haptic Alerts" feature:
///   Settings → Sounds & Haptics → Haptic Alerts → (on Watch) Always On
///
/// No Watch app or WatchConnectivity framework is required — the OS
/// delivers the notification-pattern haptic to the wrist automatically.
/// </summary>
public class HapticAlertService : IHapticAlertService
{
    // Pre-create the generator so it is ready when needed.
    // Generator must be created and used on the main thread.
    private UINotificationFeedbackGenerator? _generator;

    public void Alert()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Lazily create on first call (must be on main thread)
            _generator ??= new UINotificationFeedbackGenerator();

            // Prepare tells the Taptic Engine to warm up so the haptic
            // fires with minimal latency when NotificationOccurred is called.
            _generator.Prepare();

            // Warning = double-tap pattern (same as receiving a message notification).
            // This is the haptic iOS/watchOS uses to signal "something needs attention."
            _generator.NotificationOccurred(UINotificationFeedbackType.Warning);
        });
    }
}
