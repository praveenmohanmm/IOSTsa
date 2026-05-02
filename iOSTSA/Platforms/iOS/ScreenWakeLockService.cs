using iOSTSA.Services;
using UIKit;

namespace iOSTSA.Platforms.iOS;

public class ScreenWakeLockService : IScreenWakeLock
{
    public void Enable()  => UIApplication.SharedApplication.IdleTimerDisabled = true;
    public void Disable() => UIApplication.SharedApplication.IdleTimerDisabled = false;
}
