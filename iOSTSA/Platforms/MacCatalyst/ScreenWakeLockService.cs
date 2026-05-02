using iOSTSA.Services;
using UIKit;

// Same namespace as the iOS implementation — both are compiled under #if IOS || MACCATALYST
// and only one is ever included per build, so the duplicate class name is fine.
namespace iOSTSA.Platforms.iOS;

public class ScreenWakeLockService : IScreenWakeLock
{
    public void Enable()  => UIApplication.SharedApplication.IdleTimerDisabled = true;
    public void Disable() => UIApplication.SharedApplication.IdleTimerDisabled = false;
}
