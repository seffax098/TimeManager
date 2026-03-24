using System;
using System.Windows;
using Front.Services;

namespace Front;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        // Global exception handlers (helps diagnose crashes on navigation / API calls)
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        base.OnStartup(e);

        var baseUrl = "http://localhost:5000"; // можно вынести в конфигурацию
        AppState.Current.Initialize(baseUrl);

        var mainWindow = new MainWindow();
        mainWindow.Show();

        // If token exists from previous run - pre-load profile in background.
        try
        {
            await AppState.Current.TryLoadProfileAfterStartupAsync();
        }
        catch
        {
            // ignore (message is shown in AppState)
        }
    }

    private void App_DispatcherUnhandledException(object? sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        try
        {
            MessageBox.Show($"Произошла ошибка приложения:\n{e.Exception}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show($"Необработанное исключение домена:\n{ex}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch
        {
            // ignore
        }
    }
}
