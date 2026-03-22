using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Front.Models;

public class EmployeeUsage : INotifyPropertyChanged
{
    private string _fullName = string.Empty;
    private bool _isExpanded;

    public EmployeeUsage()
    {
        Sites.CollectionChanged += Sites_CollectionChanged;
    }

    public string FullName
    {
        get => _fullName;
        set => SetField(ref _fullName, value);
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (SetField(ref _isExpanded, value))
            {
                OnPropertyChanged(nameof(ToggleText));
                OnPropertyChanged(nameof(ToggleGlyph));
            }
        }
    }

    public string ToggleText => IsExpanded ? "Свернуть" : "Развернуть";

    public string ToggleGlyph => IsExpanded ? "▲" : "▼";

    public ObservableCollection<SiteUsage> Sites { get; } = new();

    public int TotalMinutes => Sites.Sum(x => x.Minutes);

    public int WorkMinutes => Sites
        .Where(x => string.Equals(x.Verdict, "работа", StringComparison.OrdinalIgnoreCase))
        .Sum(x => x.Minutes);

    public int RestMinutes => Sites
        .Where(x => string.Equals(x.Verdict, "отдых", StringComparison.OrdinalIgnoreCase))
        .Sum(x => x.Minutes);

    public int WorkPercent => TotalMinutes == 0
        ? 0
        : (int)Math.Round(WorkMinutes * 100.0 / TotalMinutes);

    public int RestPercent => TotalMinutes == 0
        ? 0
        : (int)Math.Round(RestMinutes * 100.0 / TotalMinutes);

    public bool IsRestDanger => RestPercent > 15;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Sites_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (SiteUsage site in e.OldItems)
            {
                site.PropertyChanged -= Site_PropertyChanged;
            }
        }

        if (e.NewItems != null)
        {
            foreach (SiteUsage site in e.NewItems)
            {
                site.PropertyChanged += Site_PropertyChanged;
            }
        }

        RaiseComputed();
    }

    private void Site_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SiteUsage.Minutes) ||
            e.PropertyName == nameof(SiteUsage.Verdict))
        {
            RaiseComputed();
        }
    }

    private void RaiseComputed()
    {
        OnPropertyChanged(nameof(TotalMinutes));
        OnPropertyChanged(nameof(WorkMinutes));
        OnPropertyChanged(nameof(RestMinutes));
        OnPropertyChanged(nameof(WorkPercent));
        OnPropertyChanged(nameof(RestPercent));
        OnPropertyChanged(nameof(IsRestDanger));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}