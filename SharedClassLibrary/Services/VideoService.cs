using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using SharedClassLibrary.Services;
using Video.Messaging.App.Models;

namespace Video.Messaging.App.Services;

public interface IVideoService
{
    Task<bool> StartRecordingAsync();
    Task<bool> StopRecordingAsync();
    Task<bool> PauseRecordingAsync();
    Task<bool> ResumeRecordingAsync();
    Task<string> SaveRecordingAsync(string senderName, string clientId, IProgress<int> progress = null);


    Task<bool> DiscardRecordingAsync();
    Task<VideoMessage> RetakeRecordingAsync();
    Task<string> GetPreviewUrlAsync();
    Task<List<VideoMessage>> GetVideoMessagesAsync();
    Task<bool> DeleteVideoMessageAsync(Guid messageId);
    Task<IFormFile?> GetVideoAsFormFileAsync();
    Task<bool> DownloadVideoForTestingAsync();
}

public class VideoService : IVideoService
{
    private readonly IVideoApiService _videoApiService;
    private readonly IJSVideoService _jsVideoService;
    private static readonly List<VideoMessage> _videoMessages = new(); // In-memory storage
    public async Task<IFormFile?> GetVideoAsFormFileAsync()
    {
        var videoData = await _jsVideoService.GetVideoBlobAsync();
        if (videoData == null || videoData.Data.Length == 0)
            return null;

        // Convert int array back to byte array
        var byteArray = videoData.Data.Select(x => (byte)x).ToArray();
        var stream = new MemoryStream(byteArray);

        return new FormFile(stream, 0, stream.Length, "video", videoData.Filename)
        {
            Headers = new HeaderDictionary(),
            ContentType = videoData.MimeType
        };
    }

    public async Task<bool> DownloadVideoForTestingAsync()
    {
        return await _jsVideoService.DownloadVideoFileAsync();
    }

    public VideoService(IJSVideoService jsVideoService, IVideoApiService videoApiService)
    {
        _jsVideoService = jsVideoService;
        _videoApiService = videoApiService;
    }

    public async Task<bool> StartRecordingAsync()
    {
        return await _jsVideoService.StartMediaRecordingAsync();
    }

    public async Task<bool> StopRecordingAsync()
    {
        return await _jsVideoService.StopMediaRecordingAsync();
    }

    public async Task<bool> PauseRecordingAsync()
    {
        return await _jsVideoService.PauseMediaRecordingAsync();
    }

    public async Task<bool> ResumeRecordingAsync()
    {
        return await _jsVideoService.ResumeMediaRecordingAsync();
    }

    public async Task<string> SaveRecordingAsync(string senderName, string clientId, IProgress<int> progress = null)
    {
        var videoFile = await GetVideoAsFormFileAsync();
        if (videoFile == null)
            throw new Exception("No video file available");

        try
        {
            var uploadResult = await _videoApiService.UploadVideoAsync(videoFile, clientId, progress);
            return uploadResult;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to upload video: {ex.Message}");
        }
    }
    public async Task<bool> DiscardRecordingAsync()
    {
        var success = await _jsVideoService.StopMediaRecordingAsync();
        if (success)
        {
            await _jsVideoService.CleanupAsync();
        }
        return success;
    }

    public async Task<VideoMessage> RetakeRecordingAsync()
    {
        await _jsVideoService.CleanupAsync();
        await _jsVideoService.StartMediaRecordingAsync();
        return new VideoMessage
        {
            Id = Guid.NewGuid(),
        };
    }

    public async Task<string> GetPreviewUrlAsync()
    {
        return await _jsVideoService.GetVideoAsDataUrlAsync();
    }

    public async Task<List<VideoMessage>> GetVideoMessagesAsync()
    {
        return await Task.FromResult(_videoMessages);
    }

    public async Task<bool> DeleteVideoMessageAsync(Guid messageId)
    {
        var message = _videoMessages.FirstOrDefault(m => m.Id == messageId);
        if (message != null)
        {
            _videoMessages.Remove(message);

            await _jsVideoService.CleanupAsync();

            return true;
        }
        return false;
    }
}