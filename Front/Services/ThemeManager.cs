using System.Windows;

namespace Front.Services;

public enum AppTheme
{
    Light,
    Dark
}

public static class ThemeManager
{
    public static event EventHandler<AppTheme>? ThemeChanged;

    public static AppTheme CurrentTheme { get; private set; } = AppTheme.Light;

    public static void ToggleTheme() => ApplyTheme(CurrentTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light);

    public static void ApplyTheme(AppTheme theme)
    {
        var app = Application.Current;
        if (app is null)
        {
            return;
        }

        var dictionaries = app.Resources.MergedDictionaries;
        var existing = dictionaries.FirstOrDefault(d => d.Source is not null &&
                                                       (d.Source.OriginalString.Contains("LightTheme.xaml") ||
                                                        d.Source.OriginalString.Contains("DarkTheme.xaml")));
        if (existing is not null)
        {
            dictionaries.Remove(existing);
        }

        var path = theme == AppTheme.Light ? "Themes/LightTheme.xaml" : "Themes/DarkTheme.xaml";
        dictionaries.Add(new ResourceDictionary { Source = new Uri(path, UriKind.Relative) });

        CurrentTheme = theme;
        ThemeChanged?.Invoke(null, theme);
    }
}
