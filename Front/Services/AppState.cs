using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Front.Models;

namespace Front.Services;

public sealed class AppState : INotifyPropertyChanged
{
    private string _userFullName = "Иванов Иван Иванович";
    private TimeSpan _workTime = new(8, 30, 0);

    public static AppState Current { get; } = new();

    private AppState()
    {
        WorkStack.CollectionChanged += WorkStack_CollectionChanged;
        ResetDefaults();
    }

    public string UserFullName
    {
        get => _userFullName;
        set => SetField(ref _userFullName, value);
    }

    public ObservableCollection<string> WorkStack { get; } = new();

    public TimeSpan WorkTime
    {
        get => _workTime;
        set
        {
            if (SetField(ref _workTime, value))
            {
                OnPropertyChanged(nameof(WorkTimeDisplay));
            }
        }
    }

    public string WorkTimeDisplay => WorkTime.ToString(@"hh\:mm\:ss");

    public ObservableCollection<SiteUsage> DashboardSites { get; } = new();

    public ObservableCollection<EmployeeUsage> Employees { get; } = new();

    public string WorkStackDisplay
    {
        get => string.Join(", ", WorkStack.Where(x => !string.IsNullOrWhiteSpace(x)));
        set
        {
            var items = (value ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            WorkStack.Clear();
            foreach (var item in items)
            {
                WorkStack.Add(item);
            }

            OnPropertyChanged();
        }
    }

    public void ResetDefaults()
    {
        UserFullName = "Иванов Иван Иванович";
        WorkTime = new TimeSpan(8, 30, 0);

        WorkStack.Clear();
        foreach (var item in new[] { "Java", "Spring", "PostgreSQL" })
        {
            WorkStack.Add(item);
        }

        DashboardSites.Clear();
        foreach (var site in DemoDataService.CreateDashboardSites())
        {
            DashboardSites.Add(site);
        }

        Employees.Clear();
        foreach (var employee in DemoDataService.CreateEmployees())
        {
            Employees.Add(employee);
        }

        OnPropertyChanged(nameof(WorkStackDisplay));
        OnPropertyChanged(nameof(WorkTimeDisplay));
    }

    private void WorkStack_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        OnPropertyChanged(nameof(WorkStackDisplay));

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
