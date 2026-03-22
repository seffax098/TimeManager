using System.Windows;
using System.Windows.Controls;
using Front.Services;

namespace Front.Pages;

public partial class RegisterPage : Page
{
    public RegisterPage()
    {
        InitializeComponent();
        ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
        Unloaded += RegisterPage_Unloaded;
        UpdateThemeButton();
    }

    private void ThemeManager_ThemeChanged(object? sender, AppTheme e) => UpdateThemeButton();

    private void UpdateThemeButton()
    {
        ThemeButton.Content = ThemeManager.CurrentTheme == AppTheme.Dark ? "☀" : "☾";
        ThemeButton.ToolTip = ThemeManager.CurrentTheme == AppTheme.Dark ? "Светлая тема" : "Тёмная тема";
    }

    private void ThemeButton_Click(object sender, RoutedEventArgs e) => ThemeManager.ToggleTheme();

    private void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        var login = LoginTextBox.Text.Trim();
        var password = PasswordTextBox.Password.Trim();

        if (login.Length < 3)
        {
            MessageBox.Show("Логин должен содержать минимум 3 символа.", "Регистрация", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (password.Length < 6)
        {
            MessageBox.Show("Пароль должен содержать минимум 6 символов.", "Регистрация", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        MessageBox.Show($"Пользователь {login} зарегистрирован (демо).", "Регистрация", MessageBoxButton.OK, MessageBoxImage.Information);
        LoginTextBox.Clear();
        PasswordTextBox.Clear();
    }

    private void RegisterPage_Unloaded(object sender, RoutedEventArgs e)
    {
        ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
    }
}
