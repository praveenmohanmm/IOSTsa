using Microsoft.Extensions.Logging;
using iOSTSA.Services;

#if IOS || MACCATALYST
using iOSTSA.Platforms.iOS;
using Microsoft.AspNetCore.Components.WebView.Maui;
using WebKit;
#endif

namespace iOSTSA;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // ── App services ─────────────────────────────────────────────────
        builder.Services.AddSingleton<TrafficSignalService>();
        builder.Services.AddSingleton<MauiAlertSettingsService>();
        builder.Services.AddSingleton<MauiGeolocationService>();

        // Platform-specific screen wake lock
#if IOS || MACCATALYST
        builder.Services.AddSingleton<IScreenWakeLock, ScreenWakeLockService>();
#else
        builder.Services.AddSingleton<IScreenWakeLock, NoOpScreenWakeLock>();
#endif

        // Haptic alert — vibrates iPhone Taptic Engine (Apple Watch mirrors it on the wrist)
#if IOS || MACCATALYST
        builder.Services.AddSingleton<IHapticAlertService, HapticAlertService>();
#else
        builder.Services.AddSingleton<IHapticAlertService, NoOpHapticAlertService>();
#endif

        // ── iOS / Mac Catalyst: allow audio autoplay in BlazorWebView ────
        // Removes the requirement for a user gesture before AudioContext
        // can play sound — this is the key difference vs a PWA.
#if IOS || MACCATALYST
        BlazorWebViewHandler.BlazorWebViewMapper.AppendToMapping(
            "AllowAutoplayAudio",
            (handler, _) =>
            {
                if (handler.PlatformView is WKWebView wkWebView)
                {
                    wkWebView.Configuration.MediaTypesRequiringUserActionForPlayback =
                        WKAudiovisualMediaTypes.None;
                    wkWebView.Configuration.AllowsInlineMediaPlayback = true;
                }
            });
#endif

        return builder.Build();
    }
}

/// <summary>No-op wake lock for platforms that don't need it (fallback).</summary>
file sealed class NoOpScreenWakeLock : IScreenWakeLock
{
    public void Enable()  { }
    public void Disable() { }
}

/// <summary>No-op haptic for platforms without a Taptic Engine (fallback).</summary>
file sealed class NoOpHapticAlertService : IHapticAlertService
{
    public void Alert() { }
}
