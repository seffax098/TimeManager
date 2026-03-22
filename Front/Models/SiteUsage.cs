using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Front.Models;

public class SiteUsage : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private int _minutes;
    private string _verdict = "работа";
    private double _chartHeight;
    private int _workPercent;
    private int _restPercent;

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public int Minutes
    {
        get => _minutes;
        set
        {
            if (SetField(ref _minutes, value))
            {
                OnPropertyChanged(nameof(TimeDisplay));
            }
        }
    }

    public string Verdict
    {
        get => _verdict;
        set
        {
            if (SetField(ref _verdict, value))
            {
                UpdatePercents();
                OnPropertyChanged(nameof(Verdict));
            }
        }
    }

    public double ChartHeight
    {
        get => _chartHeight;
        set => SetField(ref _chartHeight, value);
    }

    public ObservableCollection<LinkVisit> Links { get; } = new();

    public string TimeDisplay => TimeSpan.FromMinutes(Minutes).ToString(@"hh\:mm\:ss");

    public int WorkPercent
    {
        get => _workPercent;
        set => SetField(ref _workPercent, value);
    }

    public int RestPercent
    {
        get => _restPercent;
        set => SetField(ref _restPercent, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public SiteUsage()
    {
        UpdatePercents();
    }

    private void UpdatePercents()
    {
        var wp = _verdict == "работа" ? 100 : 0;
        var rp = 100 - wp;
        // Use SetField to raise change notifications
        SetField(ref _workPercent, wp, nameof(WorkPercent));
        SetField(ref _restPercent, rp, nameof(RestPercent));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
