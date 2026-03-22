using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Front.Models;
using Front.Services;
using NavigationArgs = System.Windows.Navigation.RequestNavigateEventArgs;

namespace Front.Pages;

public partial class DashboardPage : Page, INotifyPropertyChanged
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private readonly AppState _state = AppState.Current;
    private int _elapsedSeconds;
    private bool _isRunning;
    private ObservableCollection<LinkVisit> _selectedSiteLinks = new();

    public DashboardPage()
    {
        InitializeComponent();
        DataContext = this;
        _timer.Tick += Timer_Tick;
        ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
        Unloaded += DashboardPage_Unloaded;
        UpdateThemeButton();

        if (_state.DashboardSites.Count > 0)
        {
            SitesListBox.SelectedIndex = 0;
            UpdateSelectedSite(_state.DashboardSites[0]);
        }

        RenderTimer();
    }

    public string UserFullName => _state.UserFullName;
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

    private void ToggleTimerButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isRunning)
        {
            _isRunning = false;
            _timer.Stop();
            ToggleTimerButton.Content = "Старт";
            TimerStatusTextBlock.Text = "Таймер остановлен";
        }
        else
        {
            _isRunning = true;
            _timer.Start();
            ToggleTimerButton.Content = "Стоп";
            TimerStatusTextBlock.Text = "Таймер запущен";
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
    }

    private void LinksDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }
}
