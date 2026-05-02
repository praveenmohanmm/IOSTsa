using System.Text.Json;
using iOSTSA.Models;

namespace iOSTSA.Services;

/// <summary>
/// Persists alert settings using MAUI Preferences (maps to NSUserDefaults on iOS).
/// Drop-in replacement for the web app's localStorage-based service.
/// </summary>
public class MauiAlertSettingsService
{
    private const string StorageKey = "tsa-settings";

    public AlertSettings Settings { get; private set; } = new();

    public event Action? SettingsChanged;

    public Task LoadAsync()
    {
        try
        {
            var json = Preferences.Default.Get<string?>(StorageKey, null);
            if (!string.IsNullOrWhiteSpace(json))
                Settings = JsonSerializer.Deserialize<AlertSettings>(json) ?? new();
        }
        catch { /* ignore – first run */ }
        return Task.CompletedTask;
    }

    public Task SaveAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(Settings);
            Preferences.Default.Set(StorageKey, json);
        }
        catch { }
        SettingsChanged?.Invoke();
        return Task.CompletedTask;
    }

    public Task ResetAsync()
    {
        Settings = new();
        return SaveAsync();
    }
}
