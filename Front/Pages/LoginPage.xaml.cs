using Front.Services;
using System.Windows;
using System.Windows.Controls;

namespace Front.Pages
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            Unloaded += (s, e) => ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
            UpdateThemeButton();
        }

        private void ThemeManager_ThemeChanged(object? sender, AppTheme e) => UpdateThemeButton();

        private void UpdateThemeButton()
        {
            ThemeButton.Content = ThemeManager.CurrentTheme == AppTheme.Dark ? "☀" : "☾";
            ThemeButton.ToolTip = ThemeManager.CurrentTheme == AppTheme.Dark ? "Светлая тема" : "Тёмная тема";
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e) => ThemeManager.ToggleTheme();

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var login = LoginTextBox.Text.Trim();
            var password = PasswordBox.Password.Trim();

            if (login.Length < 3)
            {
                MessageBox.Show("Логин не менее 3 символов.");
                return;
            }
            if (password.Length < 6)
            {
                MessageBox.Show("Пароль не менее 6 символов.");
                return;
            }

            var success = await AppState.Current.LoginAsync(login, password);
            if (success)
            {
                if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
                    mainWindow.Navigate(new DashboardPage());
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}