using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Front.Models;
using Front.Services;
using Front.Contracts;
using NavigationArgs = System.Windows.Navigation.RequestNavigateEventArgs;

namespace Front.Pages;

public partial class DashboardPage : Page, INotifyPropertyChanged
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private readonly AppState _state = AppState.Current;
    private int _elapsedSeconds;
    private bool _isRunning;
    private ObservableCollection<LinkVisit> _selectedSiteLinks = new();
    private Guid? _currentSessionId;

    public DashboardPage()
    {
        InitializeComponent();
        DataContext = this;
        Loaded += DashboardPage_Loaded;
        // Подписка на изменения в AppState
        _state.PropertyChanged += AppState_PropertyChanged;

        // Загружаем данные после создания
        Loaded += async (s, e) => await LoadDashboardAsync();
    }

    private void AppState_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // При изменении UserFullName или DashboardSites обновить UI
        if (e.PropertyName == nameof(AppState.DashboardSites))
        {
            // Обновить список сайтов (уже привязан)
            OnPropertyChanged(nameof(DashboardSites));
            // Обновить выбранный сайт
            if (_state.DashboardSites.Count > 0)
            {
                SitesListBox.SelectedIndex = 0;
                UpdateSelectedSite(_state.DashboardSites[0]);
            }
        }
        // При изменении UserFullName перерисовка произойдёт автоматически через Binding
    }

    private async void DashboardPage_Loaded(object sender, RoutedEventArgs e)
    {
        await SyncActiveSessionAsync();
        await LoadDashboardAsync();
    }

    private async Task SyncActiveSessionAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(AppState.Current.AccessToken))
            {
                return;
            }

            var active = await AppState.Current.GetActiveTimerSessionAsync();
            if (active is null)
            {
                _currentSessionId = null;
                _isRunning = false;
                _timer.Stop();
                ToggleTimerButton.Content = "Старт";
                TimerStatusTextBlock.Text = "Таймер остановлен";
                return;
            }

            _currentSessionId = active.SessionId;

            // Approximate elapsed time from server startedAt
            _elapsedSeconds = (int)Math.Max(0, (DateTimeOffset.UtcNow - active.StartedAt).TotalSeconds);
            RenderTimer();

            _isRunning = true;
            _timer.Start();
            ToggleTimerButton.Content = "Стоп";
            TimerStatusTextBlock.Text = "Таймер запущен";
        }
        catch (Exception ex)
        {
            // Don't crash dashboard if sync fails
            Debug.WriteLine($"SyncActiveSession error: {ex}");
        }
    }

    private async Task LoadDashboardAsync()
    {
        var report = await AppState.Current.GetDayReportAsync();
        if (report != null)
        {
            var sites = AppState.Current.ConvertDayReportToSiteUsages(report);
            AppState.Current.DashboardSites = sites;
            OnPropertyChanged(nameof(DashboardSites));
            // Если нужно выбрать первый сайт
            if (sites.Count > 0)
                UpdateSelectedSite(sites[0]);
        }
    }

    public ObservableCollection<SiteUsage> DashboardSites => _state.DashboardSites;
    public ObservableCollection<string> Verdicts { get; } = new(new[] { "отдых", "работа" });

    public ObservableCollection<LinkVisit> SelectedSiteLinks
    {
        get => _selectedSiteLinks;
        set
        {
            _selectedSiteLinks = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void ThemeManager_ThemeChanged(object? sender, AppTheme e) => UpdateThemeButton();

    private void UpdateThemeButton()
    {
        ThemeButton.Content = ThemeManager.CurrentTheme == AppTheme.Dark ? "☀" : "☾";
        ThemeButton.ToolTip = ThemeManager.CurrentTheme == AppTheme.Dark ? "Светлая тема" : "Тёмная тема";
    }

    private void ThemeButton_Click(object sender, RoutedEventArgs e) => ThemeManager.ToggleTheme();

    private void Timer_Tick(object? sender, EventArgs e)
    {
        _elapsedSeconds++;
        RenderTimer();
    }

    private void RenderTimer() => TimerTextBlock.Text = TimeSpan.FromSeconds(_elapsedSeconds).ToString(@"hh\:mm\:ss");

    private async void ToggleTimerButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isRunning)
        {
            // stop
            try
            {
                if (_currentSessionId is null)
                {
                    _isRunning = false;
                    _timer.Stop();
                    ToggleTimerButton.Content = "Старт";
                    TimerStatusTextBlock.Text = "Таймер остановлен";
                    return;
                }

                await AppState.Current.StopTimerAsync(_currentSessionId.Value);

                _isRunning = false;
                _timer.Stop();
                ToggleTimerButton.Content = "Старт";
                TimerStatusTextBlock.Text = "Таймер остановлен";
                _currentSessionId = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось остановить таймер: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        else
        {
            // start
            try
            {
                if (string.IsNullOrWhiteSpace(AppState.Current.AccessToken))
                {
                    MessageBox.Show("Сначала выполните вход (нет access token).", "Авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var response = await AppState.Current.StartTimerAsync();
                _currentSessionId = response.SessionId;

                _isRunning = true;
                _timer.Start();
                ToggleTimerButton.Content = "Стоп";
                TimerStatusTextBlock.Text = "Таймер запущен";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось запустить таймер: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    private void SitesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SitesListBox.SelectedItem is SiteUsage site)
        {
            UpdateSelectedSite(site);
        }
    }

    private void UpdateSelectedSite(SiteUsage site)
    {
        DetailsTitleTextBlock.Text = site.Name;
        DetailsTotalTextBlock.Text = $"Итого: {site.TimeDisplay}";
        SelectedSiteLinks = site.Links;
        LinksDataGrid.ItemsSource = SelectedSiteLinks;
    }

    private void Hyperlink_RequestNavigate(object sender, NavigationArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void DashboardPage_Unloaded(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
        ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
        _state.PropertyChanged -= AppState_PropertyChanged;
    }

    private void LinksDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }
}
