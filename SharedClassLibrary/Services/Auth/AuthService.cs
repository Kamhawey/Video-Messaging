using Blazored.Toast.Services;
using Microsoft.Extensions.Configuration;
using SharedClassLibrary.Models.Auth;
using SharedClassLibrary.Models.Common;
using System.Text;
using System.Text.Json;

namespace SharedClassLibrary.Services.Auth;

public interface IAuthService
{
    Task<Result<LoginResponseDto>?> LoginAsync(string email, string password);
    Task<LoginResponseDto?> GetCurrentUserAsync();
    Task<bool> IsAuthenticatedAsync();
    Task LogoutAsync();
    Task<string?> GetAuthTokenAsync();
}

public class AuthService(HttpClient httpClient, ITokenStorageService tokenStorage, IConfiguration configuration, IToastService toast) : IAuthService
{
    private readonly string _apiUrl = configuration["ApiUrl"] ?? throw new ArgumentNullException("ApiUrl not configured");

    public async Task<Result<LoginResponseDto>?> LoginAsync(string email, string password)
    {
        try
        {
            var loginRequest = new LoginRequestDto
            {
                Email = email,
                Password = password
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"{_apiUrl}/api/Identity/Login", content);

            if (!response.IsSuccessStatusCode)
            {
                toast.ShowError("Login failed: Server returned an error.");
                return null;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Result<LoginResponseDto>>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
            {
                toast.ShowError("Login failed: Unable to process server response.");
                return null;
            }

            if (result.IsSuccess && result.Data != null)
            {
                await tokenStorage.SaveTokenAsync(result.Data.Token);
                await tokenStorage.SaveRefreshTokenAsync(result.Data.RefreshToken);
                await tokenStorage.SaveUserDataAsync(result.Data);

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Data.Token);

                toast.ShowSuccess("Login successful!");
                return result;
            }
            else
            {
                toast.ShowError($"Login failed: {result.Error.Message} (Code: {result.Error.Code})");
                return result;
            }
        }
        catch (Exception ex)
        {
            toast.ShowError($"Login failed: {ex.Message}");
            return null;
        }
    }

    public async Task<LoginResponseDto?> GetCurrentUserAsync()
    {
        return await tokenStorage.GetUserDataAsync();
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await tokenStorage.GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return false;

        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        return true;
    }

    public async Task LogoutAsync()
    {
        await tokenStorage.ClearAllAsync();
        httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<string?> GetAuthTokenAsync()
    {
        return await tokenStorage.GetTokenAsync();
    }
}