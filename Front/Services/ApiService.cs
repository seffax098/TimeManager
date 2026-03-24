using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace Front.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApiService(string baseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _httpClient = new HttpClient();
        }

        public void SetAuthToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        private static JsonSerializerOptions JsonOptions { get; } = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private static Exception CreateHttpException(HttpResponseMessage response, string responseBody)
        {
            var message = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}. Body: {responseBody}";
            return new HttpRequestException(message, null, response.StatusCode);
        }

        public async Task<T?> GetAsync<T>(string endpoint, CancellationToken ct = default)
        {
            var url = $"{_baseUrl}/{endpoint.TrimStart('/')}";
            Debug.WriteLine($"GET {url}");

            var response = await _httpClient.GetAsync(url, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);
            Debug.WriteLine($"Response status: {(int)response.StatusCode} {response.StatusCode}");
            Debug.WriteLine($"Response body: {responseBody}");

            if (!response.IsSuccessStatusCode)
            {
                throw CreateHttpException(response, responseBody);
            }

            return JsonSerializer.Deserialize<T>(responseBody, JsonOptions);
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data, CancellationToken ct = default)
        {
            var url = $"{_baseUrl}/{endpoint.TrimStart('/')}";
            var json = JsonSerializer.Serialize(data);

            Debug.WriteLine($"POST {url}");
            Debug.WriteLine($"Request body: {json}");

            var response = await _httpClient.PostAsJsonAsync(url, data, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);
            Debug.WriteLine($"Response status: {(int)response.StatusCode} {response.StatusCode}");
            Debug.WriteLine($"Response body: {responseBody}");

            if (!response.IsSuccessStatusCode)
            {
                throw CreateHttpException(response, responseBody);
            }

            return JsonSerializer.Deserialize<T>(responseBody, JsonOptions);
        }

        public async Task<T?> PutAsync<T>(string endpoint, object data, CancellationToken ct = default)
        {
            var url = $"{_baseUrl}/{endpoint.TrimStart('/')}";
            var json = JsonSerializer.Serialize(data);

            Debug.WriteLine($"PUT {url}");
            Debug.WriteLine($"Request body: {json}");

            var response = await _httpClient.PutAsJsonAsync(url, data, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);
            Debug.WriteLine($"Response status: {(int)response.StatusCode} {response.StatusCode}");
            Debug.WriteLine($"Response body: {responseBody}");

            if (!response.IsSuccessStatusCode)
            {
                throw CreateHttpException(response, responseBody);
            }

            return JsonSerializer.Deserialize<T>(responseBody, JsonOptions);
        }

        public async Task<T?> PostMultipartAsync<T>(string endpoint, MultipartFormDataContent content, CancellationToken ct = default)
        {
            var url = $"{_baseUrl}/{endpoint.TrimStart('/')}";
            Debug.WriteLine($"POST(Multipart) {url}");

            var response = await _httpClient.PostAsync(url, content, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);
            Debug.WriteLine($"Response status: {(int)response.StatusCode} {response.StatusCode}");
            Debug.WriteLine($"Response body: {responseBody}");

            if (!response.IsSuccessStatusCode)
            {
                throw CreateHttpException(response, responseBody);
            }

            return JsonSerializer.Deserialize<T>(responseBody, JsonOptions);
        }
    }
}