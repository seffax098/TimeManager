using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Front.Models;
using Front.Services;

namespace Front.Pages;

public partial class AdminPage : Page
{
    public AdminPage()
    {
        InitializeComponent();
        DataContext = AppState.Current;
        Loaded += AdminPage_Loaded;
        ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
        Unloaded += AdminPage_Unloaded;
        UpdateThemeButton();
    }
    private async void AdminPage_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadEmployeesAsync();
    }

    private async Task LoadEmployeesAsync()
    {
        try
        {
            var response = await AppState.Current.GetAdminEmployeesAsync();
            if (response != null)
            {
                var employees = AppState.Current.ConvertAdminEmployeesToEmployeeUsages(response);
                AppState.Current.Employees = employees;
            }
        }
        catch (System.Net.Http.HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            MessageBox.Show(
                "Доступ запрещён: требуется роль admin.",
                "Админ-панель",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // Optionally clear list
            AppState.Current.Employees.Clear();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось загрузить сотрудников: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void ThemeManager_ThemeChanged(object? sender, AppTheme e)
    {
        UpdateThemeButton();
    }

    private void UpdateThemeButton()
    {
        ThemeButton.Content = ThemeManager.CurrentTheme == AppTheme.Dark ? "☀" : "☾";
        ThemeButton.ToolTip = ThemeManager.CurrentTheme == AppTheme.Dark ? "Светлая тема" : "Тёмная тема";
    }

    private void ThemeButton_Click(object sender, RoutedEventArgs e)
    {
        ThemeManager.ToggleTheme();
    }

    private void ToggleEmployee_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not FrameworkElement fe)
                return;

            // Prefer DataContext (more reliable inside templates), fall back to Tag
            var employee = fe.DataContext as EmployeeUsage ?? (fe is Button b ? b.Tag as EmployeeUsage : null);
            if (employee == null)
                return;

            // Ensure toggle on UI thread
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => employee.IsExpanded = !employee.IsExpanded);
            }
            else
            {
                employee.IsExpanded = !employee.IsExpanded;
            }
        }
        catch (Exception ex)
        {
            // Show a user-friendly message instead of crashing the app
            MessageBox.Show($"Не удалось развернуть пользователя:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void OpenUrl_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Button button)
                return;

            var rawUrl = button.Tag?.ToString();
            if (string.IsNullOrWhiteSpace(rawUrl))
                return;

            var url = rawUrl.Trim();

            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось открыть ссылку:\n{ex.Message}",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void ScreenshotButton_Click(object sender, RoutedEventArgs e)
    {
        // Prevent click bubbling to parent Button/OpenUrl_Click
        if (e is RoutedEventArgs rea)
        {
            rea.Handled = true;
        }

        // Placeholder for future action (open screenshot link)
        // Currently do nothing
    }

    private void AdminPage_Unloaded(object sender, RoutedEventArgs e)
    {
        ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
    }
}