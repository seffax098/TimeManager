using System;
using System.Windows;
using Front.Services;

namespace Front;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Apply default theme
        ThemeManager.ApplyTheme(AppTheme.Light);

        // Global exception handlers to avoid process exit on UI errors
        this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    private void App_DispatcherUnhandledException(object? sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        try
        {
            MessageBox.Show($"Произошла ошибка приложения:\n{e.Exception.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
        catch
        {
            // ignore
        }
    }

    private void CurrentDomain_UnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show($"Необработанное исключение домена:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch
        {
            // ignore
        }
    }
}
