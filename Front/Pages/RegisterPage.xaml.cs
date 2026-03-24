using System.Windows;
using System.Windows.Controls;
using Front.Services;
using Front.Contracts;

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

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        var login = LoginTextBox.Text.Trim();
        var password = PasswordTextBox.Password.Trim();
        var fullName = FullNameTextBox.Text.Trim();

        if (login.Length < 3) { MessageBox.Show("Логин не менее 3 символов."); return; }
        if (password.Length < 6) { MessageBox.Show("Пароль не менее 6 символов."); return; }
        if (string.IsNullOrWhiteSpace(fullName)) { MessageBox.Show("Введите ФИО."); return; }

        var success = await AppState.Current.RegisterAsync(login, password, fullName);
        if (success)
        {
            MessageBox.Show("Регистрация успешна. Теперь войдите.");
            LoginTextBox.Clear();
            PasswordTextBox.Clear();
            FullNameTextBox.Clear();
            // Можно автоматически перейти на страницу логина (если есть)
        }
        else
        {
            MessageBox.Show("Ошибка регистрации. Возможно, логин занят.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RegisterPage_Unloaded(object sender, RoutedEventArgs e)
    {
        ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
    }
}
