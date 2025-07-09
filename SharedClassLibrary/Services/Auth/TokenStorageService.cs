using SharedClassLibrary.Models.Auth;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SharedClassLibrary.Services.Auth;

public interface ITokenStorageService
{
    Task SaveTokenAsync(string token);
    Task SaveRefreshTokenAsync(string refreshToken);
    Task<string?> GetTokenAsync();
    Task<string?> GetRefreshTokenAsync();
    Task SaveUserDataAsync(LoginResponseDto userData);
    Task<LoginResponseDto?> GetUserDataAsync();
    Task ClearAllAsync();
}


public class TokenStorageService : ITokenStorageService
{
    private readonly string _dataPath;
    private readonly string _tokenFile;
    private readonly string _refreshTokenFile;
    private readonly string _userDataFile;

    public TokenStorageService()
    {
        _dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HandTalk-Video-Messaging");
        _tokenFile = Path.Combine(_dataPath, "token.dat");
        _refreshTokenFile = Path.Combine(_dataPath, "refresh_token.dat");
        _userDataFile = Path.Combine(_dataPath, "user_data.dat");

        Directory.CreateDirectory(_dataPath);
    }

    public async Task SaveTokenAsync(string token)
    {
        await SaveEncryptedDataAsync(_tokenFile, token);
    }

    public async Task SaveRefreshTokenAsync(string refreshToken)
    {
        await SaveEncryptedDataAsync(_refreshTokenFile, refreshToken);
    }

    public async Task<string?> GetTokenAsync()
    {
        return await GetEncryptedDataAsync(_tokenFile);
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        return await GetEncryptedDataAsync(_refreshTokenFile);
    }

    public async Task SaveUserDataAsync(LoginResponseDto userData)
    {
        var json = JsonSerializer.Serialize(userData);
        await SaveEncryptedDataAsync(_userDataFile, json);
    }

    public async Task<LoginResponseDto?> GetUserDataAsync()
    {
        var json = await GetEncryptedDataAsync(_userDataFile);
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<LoginResponseDto>(json);
        }
        catch
        {
            return null;
        }
    }

    public async Task ClearAllAsync()
    {
        var files = new[] { _tokenFile, _refreshTokenFile, _userDataFile };
        foreach (var file in files)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }

    private async Task SaveEncryptedDataAsync(string filePath, string data)
    {
        try
        {
            var encrypted = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(data),
                null,
                DataProtectionScope.CurrentUser);

            await File.WriteAllBytesAsync(filePath, encrypted);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save encrypted data: {ex.Message}");
        }
    }

    private async Task<string?> GetEncryptedDataAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return null;

            var encrypted = await File.ReadAllBytesAsync(filePath);
            var decrypted = ProtectedData.Unprotect(
                encrypted,
                null,
                DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(decrypted);
        }
        catch
        {
            return null;
        }
    }
}