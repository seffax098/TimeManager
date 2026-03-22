using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Front.Services;

namespace Front.Pages;

public partial class ProfilePage : Page
{
    private readonly DispatcherTimer _toastTimer = new() { Interval = TimeSpan.FromSeconds(1.6) };

    public ProfilePage()
    {
        InitializeComponent();
        DataContext = AppState.Current;
        _toastTimer.Tick += ToastTimer_Tick;
        ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
        Unloaded += ProfilePage_Unloaded;
        UpdateThemeButton();
    }

    private void ThemeManager_ThemeChanged(object? sender, AppTheme e) => UpdateThemeButton();

    private void UpdateThemeButton()
    {
        ThemeButton.Content = ThemeManager.CurrentTheme == AppTheme.Dark ? "☀" : "☾";
        ThemeButton.ToolTip = ThemeManager.CurrentTheme == AppTheme.Dark ? "Светлая тема" : "Тёмная тема";
    }

    private void ThemeButton_Click(object sender, RoutedEventArgs e) => ThemeManager.ToggleTheme();

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var value = StackTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            StackTextBox.Focus();
            return;
        }

        AppState.Current.WorkStackDisplay = value;
        ToastTextBlock.Text = "Сохранено.";
        ToastBorder.Visibility = Visibility.Visible;
        _toastTimer.Stop();
        _toastTimer.Start();
    }

    private void ToastTimer_Tick(object? sender, EventArgs e)
    {
        _toastTimer.Stop();
        ToastBorder.Visibility = Visibility.Collapsed;
    }

    private void ProfilePage_Unloaded(object sender, RoutedEventArgs e)
    {
        _toastTimer.Stop();
        ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
    }
}
