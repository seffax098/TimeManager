using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Front.Models;
using Front.Services;

namespace Front.Pages;

public partial class SettingsPage : Page
{
    private readonly AppState _state = AppState.Current;

    public SettingsPage()
    {
        InitializeComponent();
        DataContext = this;
        ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
        Unloaded += SettingsPage_Unloaded;
        UpdateThemeButton();
        LoadFromState();
    }

    public ObservableCollection<EditableTechItem> EditableStack { get; } = new();

    private void ThemeManager_ThemeChanged(object? sender, AppTheme e) => UpdateThemeButton();

    private void UpdateThemeButton()
    {
        ThemeButton.Content = ThemeManager.CurrentTheme == AppTheme.Dark ? "☀" : "☾";
        ThemeButton.ToolTip = ThemeManager.CurrentTheme == AppTheme.Dark ? "Светлая тема" : "Тёмная тема";
    }

    private void LoadFromState()
    {
        EditableStack.Clear();
        foreach (var item in _state.WorkStack)
        {
            EditableStack.Add(new EditableTechItem { Name = item });
        }

        HoursTextBox.Text = _state.WorkTime.Hours.ToString("00");
        MinutesTextBox.Text = _state.WorkTime.Minutes.ToString("00");
        SecondsTextBox.Text = _state.WorkTime.Seconds.ToString("00");
    }

    private void ThemeButton_Click(object sender, RoutedEventArgs e) => ThemeManager.ToggleTheme();

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var value = NewTechTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        EditableStack.Add(new EditableTechItem { Name = value });
        NewTechTextBox.Clear();
        NewTechTextBox.Focus();
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: EditableTechItem item })
        {
            EditableStack.Remove(item);
        }
    }

    private void MoveUpButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: EditableTechItem item })
        {
            return;
        }

        var index = EditableStack.IndexOf(item);
        if (index <= 0)
        {
            return;
        }

        EditableStack.Move(index, index - 1);
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var hours = Clamp(HoursTextBox.Text, 0, 23);
        var minutes = Clamp(MinutesTextBox.Text, 0, 59);
        var seconds = Clamp(SecondsTextBox.Text, 0, 59);

        HoursTextBox.Text = hours.ToString("00");
        MinutesTextBox.Text = minutes.ToString("00");
        SecondsTextBox.Text = seconds.ToString("00");

        _state.WorkStack.Clear();
        foreach (var item in EditableStack.Where(x => !string.IsNullOrWhiteSpace(x.Name)))
        {
            _state.WorkStack.Add(item.Name.Trim());
        }

        _state.WorkTime = new TimeSpan(hours, minutes, seconds);

        MessageBox.Show(
            $"Сохранено.\nСтек: {_state.WorkStackDisplay}\nРабочее время: {_state.WorkTimeDisplay}",
            "Настройки",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        _state.ResetDefaults();
        LoadFromState();
    }

    private static int Clamp(string value, int min, int max)
    {
        var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
        if (!int.TryParse(digits, out var parsed))
        {
            parsed = 0;
        }

        if (parsed < min)
        {
            return min;
        }

        if (parsed > max)
        {
            return max;
        }

        return parsed;
    }

    private void SettingsPage_Unloaded(object sender, RoutedEventArgs e)
    {
        ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
    }
}
