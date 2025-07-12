using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SharedClassLibrary.Config;

namespace SharedClassLibrary.Services;

public interface IVideoApiService
{
    Task<string> UploadVideoAsync(IFormFile videoFile, string clientId);
}

public class VideoApiService : IVideoApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public VideoApiService(HttpClient httpClient, IOptions<VideoApiSettings> options)
    {
        _httpClient = httpClient;
        _baseUrl = options.Value.BaseUrl;

    }

    public async Task<string> UploadVideoAsync(IFormFile videoFile, string clientId)
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(videoFile.OpenReadStream());

        var mimeType = videoFile.ContentType;
        if (mimeType.Contains(';'))
        {
            mimeType = mimeType.Split(';')[0].Trim();
        }

        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);
        content.Add(fileContent, "file", videoFile.FileName);
        content.Add(new StringContent(clientId), "client_id");

        var response = await _httpClient.PostAsync($"{_baseUrl}/upload-device/", content);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }

        throw new Exception($"Upload failed: {response.StatusCode}");
    }
}