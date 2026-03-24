using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Front.Contracts;
using Front.Models;

namespace Front.Services
{
    public sealed class AppState : INotifyPropertyChanged
    {
        private static AppState? _current;
        private ApiService _api;
        private string _accessToken;
        private UserDto _currentUser;
        private TimeSpan _workTime;
        private ObservableCollection<string> _workStack = new();
        private ObservableCollection<SiteUsage> _dashboardSites = new();
        private ObservableCollection<EmployeeUsage> _employees = new();

        private static readonly string TokenFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SmartTimeManager",
            "access-token.txt");

        public static AppState Current => _current ??= new AppState();

        private AppState() { }

        public void Initialize(string baseUrl)
        {
            _api = new ApiService(baseUrl);

            // Try restore token from disk
            try
            {
                if (File.Exists(TokenFilePath))
                {
                    var token = File.ReadAllText(TokenFilePath).Trim();
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        AccessToken = token;
                        _api.SetAuthToken(token);
                    }
                }
            }
            catch
            {
                // ignore
            }
        }

        public string AccessToken
        {
            get => _accessToken;
            private set
            {
                _accessToken = value;
                OnPropertyChanged();

                // Persist token
                try
                {
                    var folder = Path.GetDirectoryName(TokenFilePath);
                    if (!string.IsNullOrWhiteSpace(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    if (string.IsNullOrWhiteSpace(value))
                    {
                        if (File.Exists(TokenFilePath))
                        {
                            File.Delete(TokenFilePath);
                        }
                    }
                    else
                    {
                        File.WriteAllText(TokenFilePath, value);
                    }
                }
                catch
                {
                    // ignore
                }
            }
        }

        public UserDto CurrentUser
        {
            get => _currentUser;
            private set
            {
                _currentUser = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UserFullName));
            }
        }

        public string UserFullName => CurrentUser?.FullName ?? "";

        public TimeSpan WorkTime
        {
            get => _workTime;
            set
            {
                _workTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WorkTimeDisplay));
            }
        }

        public string WorkTimeDisplay => WorkTime.ToString(@"hh\:mm\:ss");

        public ObservableCollection<string> WorkStack
        {
            get => _workStack;
            set { _workStack = value; OnPropertyChanged(); }
        }

        public ObservableCollection<SiteUsage> DashboardSites
        {
            get => _dashboardSites;
            set { _dashboardSites = value; OnPropertyChanged(); }
        }

        public ObservableCollection<EmployeeUsage> Employees
        {
            get => _employees;
            set { _employees = value; OnPropertyChanged(); }
        }

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
                foreach (var item in items) WorkStack.Add(item);
                OnPropertyChanged();
            }
        }

        // --- API вызовы ---
        public async Task<bool> LoginAsync(string login, string password)
        {
            try
            {
                var request = new LoginRequest { Login = login, Password = password };
                var response = await _api.PostAsync<LoginResponse>("api/auth/login", request);
                if (response != null)
                {
                    AccessToken = response.AccessToken;
                    CurrentUser = response.User;
                    _api.SetAuthToken(AccessToken);
                    await LoadProfileAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Login error: {ex.Message}");
            }

            return false;
        }

        public async Task<bool> RegisterAsync(string login, string password, string fullName)
        {
            try
            {
                var request = new RegisterRequest { Login = login, Password = password, FullName = fullName };
                var response = await _api.PostAsync<RegisterResponse>("api/auth/register", request);
                return response != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> TryLoadProfileAfterStartupAsync()
        {
            if (string.IsNullOrWhiteSpace(AccessToken))
            {
                return false;
            }

            try
            {
                await LoadProfileAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task LoadProfileAsync()
        {
            try
            {
                var profile = await _api.GetAsync<ProfileResponse>("api/profile");
                if (profile != null)
                {
                    CurrentUser = new UserDto(profile.UserId, profile.Login, profile.FullName, profile.Role, profile.CreatedAt);
                    WorkStack.Clear();
                    foreach (var item in profile.TechStack)
                        WorkStack.Add(item.Name);
                    WorkTime = TimeSpan.Parse(profile.Settings.WorkTime);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"LoadProfile error: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateProfileAsync(UpdateProfileRequest request)
        {
            try
            {
                await _api.PutAsync<UpdateProfileResponse>("api/profile", request);
                await LoadProfileAsync();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"UpdateProfile error: {ex.Message}");
                throw;
            }
        }

        public async Task<StartTimerResponse> StartTimerAsync(DateOnly? date = null)
        {
            return await _api.PostAsync<StartTimerResponse>("api/timer/start", new StartTimerRequest { Date = date });
        }

        public async Task<StopTimerResponse> StopTimerAsync(Guid sessionId)
        {
            return await _api.PostAsync<StopTimerResponse>("api/timer/stop", new StopTimerRequest { SessionId = sessionId });
        }

        public async Task<ActivityResponse> SendActivityAsync(ActivityRequest activity)
        {
            return await _api.PostAsync<ActivityResponse>("api/activity", activity);
        }

        public async Task<DayReportResponse> GetDayReportAsync(DateOnly? date = null)
        {
            var query = date.HasValue ? $"?date={date.Value:yyyy-MM-dd}" : "";
            return await _api.GetAsync<DayReportResponse>($"api/reports/day{query}");
        }

        public async Task<AdminEmployeesResponse> GetAdminEmployeesAsync(DateOnly? date = null, string sortBy = null, string order = null)
        {
            var query = $"date={(date?.ToString("yyyy-MM-dd") ?? DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"))}";
            if (!string.IsNullOrEmpty(sortBy)) query += $"&sort_by={sortBy}";
            if (!string.IsNullOrEmpty(order)) query += $"&order={order}";
            return await _api.GetAsync<AdminEmployeesResponse>($"api/admin/employees?{query}");
        }

        public async Task<AdminEmployeeDetailsResponse> GetAdminEmployeeDetailsAsync(Guid userId, DateOnly? date = null)
        {
            var query = date.HasValue ? $"?date={date.Value:yyyy-MM-dd}" : "";
            return await _api.GetAsync<AdminEmployeeDetailsResponse>($"api/admin/employees/{userId}{query}");
        }

        public async Task<ActiveTimerSessionResponse?> GetActiveTimerSessionAsync()
        {
            return await _api.GetAsync<ActiveTimerSessionResponse?>("api/timer/active");
        }

        // --- Вспомогательные методы для преобразования ---
        public ObservableCollection<SiteUsage> ConvertDayReportToSiteUsages(DayReportResponse report)
        {
            var sites = new ObservableCollection<SiteUsage>();
            foreach (var site in report.Sites)
            {
                var usage = new SiteUsage
                {
                    Name = site.Domain,
                    Minutes = site.DurationSec / 60,
                    Verdict = site.Verdict.ToString()
                };
                // Заполняем существующую коллекцию Links
                usage.Links.Clear();
                foreach (var u in site.Urls)
                {
                    usage.Links.Add(new LinkVisit
                    {
                        Url = u.Url,
                        Minutes = u.DurationSec / 60,
                        Verdict = u.Verdict.ToString()
                    });
                }
                sites.Add(usage);
            }

            var maxMinutes = sites.Any() ? sites.Max(x => x.Minutes) : 1;
            foreach (var site in sites)
                site.ChartHeight = Math.Max(10, site.Minutes * 170.0 / maxMinutes);

            return sites;
        }

        public ObservableCollection<EmployeeUsage> ConvertAdminEmployeesToEmployeeUsages(AdminEmployeesResponse response)
        {
            var employees = new ObservableCollection<EmployeeUsage>();
            foreach (var emp in response.Employees)
            {
                var employee = new EmployeeUsage { FullName = emp.FullName };
                foreach (var site in emp.Sites)
                {
                    var siteUsage = new SiteUsage
                    {
                        Name = site.Domain,
                        Minutes = site.DurationSec / 60,
                        Verdict = site.Verdict.ToString()
                    };
                    siteUsage.Links.Clear();
                    foreach (var u in site.Urls)
                    {
                        siteUsage.Links.Add(new LinkVisit
                        {
                            Url = u.Url,
                            Minutes = u.DurationSec / 60,
                            Verdict = u.Verdict.ToString()
                        });
                    }
                    employee.Sites.Add(siteUsage);
                }
                employees.Add(employee);
            }
            return employees;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}