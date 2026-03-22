using System.Windows;
using System.Windows.Controls;
using Front.Pages;
using Front.Services;

namespace Front;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
        Navigate(new RegisterPage());
    }

    private void ThemeManager_ThemeChanged(object? sender, AppTheme e) => UpdateThemeButton();

    private void UpdateThemeButton()
    {
    }

    private void Navigate(Page page) => MainFrame.Navigate(page);

    private void RegisterNavButton_Click(object sender, RoutedEventArgs e) => Navigate(new RegisterPage());
    private void DashboardNavButton_Click(object sender, RoutedEventArgs e) => Navigate(new DashboardPage());
    private void AdminNavButton_Click(object sender, RoutedEventArgs e) => Navigate(new AdminPage());
    private void ProfileNavButton_Click(object sender, RoutedEventArgs e) => Navigate(new ProfilePage());
    private void SettingsNavButton_Click(object sender, RoutedEventArgs e) => Navigate(new SettingsPage());

    protected override void OnClosed(System.EventArgs e)
    {
        ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
        base.OnClosed(e);
    }
}
