using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Front.Models;

public class LinkVisit : INotifyPropertyChanged
{
    private string _url = string.Empty;
    private int _minutes;
    private string _verdict = "отдых";

    public string Url
    {
        get => _url;
        set => SetField(ref _url, value);
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
        set => SetField(ref _verdict, value);
    }

    public string TimeDisplay => TimeSpan.FromMinutes(Minutes).ToString(@"hh\:mm\:ss");

    public event PropertyChangedEventHandler? PropertyChanged;

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
