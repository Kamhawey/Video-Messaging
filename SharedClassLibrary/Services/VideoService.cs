using Video.Messaging.App.Models;

namespace Video.Messaging.App.Services;

public interface IVideoService
{
    Task<bool> StartRecordingAsync();
    Task<bool> StopRecordingAsync();
    Task<bool> PauseRecordingAsync();
    Task<bool> ResumeRecordingAsync();
    Task<VideoMessage> SaveRecordingAsync(string senderName);
    Task<bool> DiscardRecordingAsync();
    Task<VideoMessage> RetakeRecordingAsync();
    Task<string> GetPreviewUrlAsync();
    Task<List<VideoMessage>> GetVideoMessagesAsync();
    Task<bool> DeleteVideoMessageAsync(Guid messageId);
}

public class VideoService : IVideoService
{
    private readonly IJSVideoService _jsVideoService;
    private static readonly List<VideoMessage> _videoMessages = new(); // In-memory storage

    public VideoService(IJSVideoService jsVideoService)
    {
        _jsVideoService = jsVideoService;
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

    public async Task<VideoMessage> SaveRecordingAsync(string senderName)
    {
        var videoUrl = await _jsVideoService.GetRecordedVideoUrlAsync();
        if (!string.IsNullOrEmpty(videoUrl))
        {
            var videoMessage = new VideoMessage
            {
                Id = Guid.NewGuid(),
                SenderName = senderName,
                VideoUrl = videoUrl,
                IsIncoming = true, // Mark as incoming for employees
                CreatedAt = DateTime.UtcNow
            };
            _videoMessages.Add(videoMessage);
            return videoMessage;
        }
        throw new InvalidOperationException("No video recorded to save.");
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
            SenderName = "Retake",
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<string> GetPreviewUrlAsync()
    {
        return await _jsVideoService.GetRecordedVideoUrlAsync();
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
            // Revoke the object URL to free memory
            if (!string.IsNullOrEmpty(message.VideoUrl))
            {
                await _jsVideoService.CleanupAsync();
            }
            return true;
        }
        return false;
    }
}