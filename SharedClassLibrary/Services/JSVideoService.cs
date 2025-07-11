using Microsoft.JSInterop;
using SharedClassLibrary.Models;
using Video.Messaging.App.Models;

namespace Video.Messaging.App.Services;

public interface IJSVideoService
{
    Task<bool> InitializeVideoElementAsync(string elementId);
    Task<bool> StartMediaRecordingAsync();
    Task<bool> ResumeMediaRecordingAsync();
    Task<bool> ResumeVideoAsync(string elementId);
    Task<BoundingRect> GetElementBoundingRectAsync(string elementId);
    Task<bool> StopMediaRecordingAsync();
    Task<string> GetRecordedVideoUrlAsync();
    Task<bool> PlayVideoAsync(string elementId, string videoUrl);
    Task<bool> PauseVideoAsync(string elementId);
    Task<bool> PauseMediaRecordingAsync();
    Task<bool> CleanupAsync();
    Task<double> GetVideoCurrentTimeAsync(string elementId);
    Task<double> GetVideoDurationAsync(string elementId);
    Task<bool> SetVideoCurrentTimeAsync(string elementId, double currentTime);
    Task<bool> SetVideoPlaybackRateAsync(string elementId, double playbackRate);
    Task<bool> CheckMediaRecordingSupport();
    Task<string> CaptureThumbnailAsync(string elementId);

    Task<VideoFileData?> GetVideoBlobAsync();
    Task<bool> DownloadVideoFileAsync();
}

public class JSVideoService : IJSVideoService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _jsModule;

    public JSVideoService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    public async Task<VideoFileData?> GetVideoBlobAsync()
    {
        try
        {
            var module = await GetJSModule();
            return await module.InvokeAsync<VideoFileData?>("getVideoBlob");
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DownloadVideoFileAsync()
    {
        try
        {
            var module = await GetJSModule();
            return await module.InvokeAsync<bool>("downloadVideoFile");
        }
        catch
        {
            return false;
        }
    }

    private async Task<IJSObjectReference> GetJSModule()
    {
        _jsModule ??= await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/videoService.js");
        return _jsModule;
    }
    public async Task<BoundingRect> GetElementBoundingRectAsync(string elementId)
    {
        try
        {
            var module = await GetJSModule();
            var result = await module.InvokeAsync<BoundingRect>("getElementBoundingRect", elementId);
            return result ?? new BoundingRect();
        }
        catch (Exception ex)
        {
            return new BoundingRect();
        }
    }

    public async Task<bool> ResumeVideoAsync(string elementId)
    {
        try
        {
            var module = await GetJSModule();
            return await module.InvokeAsync<bool>("resumeVideo", elementId);
        }
        catch
        {
            return false;
        }
    }
    public async Task<bool> CleanupAsync()
    {
        try
        {
            var module = await GetJSModule();
            return await module.InvokeAsync<bool>("cleanup");

        }
        catch
        {
            return false;
        }
    }
    public async Task<bool> InitializeVideoElementAsync(string elementId)
    {
        try
        {
            var module = await GetJSModule();
            return await module.InvokeAsync<bool>("initializeVideoElement", elementId);
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> StartMediaRecordingAsync()
    {
        try
        {
            var module = await GetJSModule();
            return await module.InvokeAsync<bool>("startMediaRecording");
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> PauseMediaRecordingAsync()
    {
        try
        {
            var module = await GetJSModule();
            return await module.InvokeAsync<bool>("pauseMediaRecording");
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ResumeMediaRecordingAsync()
    {
        try
        {
            var module = await GetJSModule();
            return await module.InvokeAsync<bool>("resumeMediaRecording");
        }
        catch
        {
            return false;
        }
    }
    public async Task<bool> StopMediaRecordingAsync()
    {
        try
        {
            var module = await GetJSModule();
            return await module.InvokeAsync<bool>("stopMediaRecording");
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetRecordedVideoUrlAsync()
    {
        try
        {
            var module = await GetJSModule();
            return await module.InvokeAsync<string>("getRecordedVideoUrl") ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public async Task<string> GetRecordingStateAsync()
    {
        try
        {
            var module = await GetJSModule();
            return await module.InvokeAsync<string>("getRecordingState") ?? "inactive";
        }
        catch
        {
            return "inactive";
        }
    }

    public async Task<bool> IsRecordingPausedAsync()
    {
        try
        {
            var module = await GetJSModule();
            return await module.InvokeAsync<bool>("isRecordingPaused");
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> PlayVideoAsync(string elementId, string videoUrl)
    {
        try
        {
            var module = await GetJSModule();
            return await module.InvokeAsync<bool>("playVideo", elementId, videoUrl);
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> PauseVideoAsync(string elementId)
    {
        try
        {
            var module = await GetJSModule();
            return await module.InvokeAsync<bool>("pauseVideo", elementId);
        }
        catch
        {
            return false;
        }
    }

    public async Task<double> GetVideoCurrentTimeAsync(string elementId)
    {
        try
        {
            var module = await GetJSModule();
            return await module.InvokeAsync<double>("getVideoCurrentTime", elementId);
        }
        catch
        {
            return 0;
        }
    }

    public async Task<double> GetVideoDurationAsync(string elementId)
    {
        try
        {
            var module = await GetJSModule();
            return await module.InvokeAsync<double>("getVideoDuration", elementId);
        }
        catch
        {
            return 0;
        }
    }

    public async Task<bool> SetVideoCurrentTimeAsync(string elementId, double currentTime)
    {
        try
        {
            var module = await GetJSModule();
            return await module.InvokeAsync<bool>("setVideoCurrentTime", elementId, currentTime);
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SetVideoPlaybackRateAsync(string elementId, double playbackRate)
    {
        try
        {
            var module = await GetJSModule();
            return await module.InvokeAsync<bool>("setVideoPlaybackRate", elementId, playbackRate);
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CheckMediaRecordingSupport()
    {
        try
        {
            var module = await GetJSModule();
            return await module.InvokeAsync<bool>("checkMediaRecordingSupport");
        }
        catch
        {
            return false;
        }
    }
    public async Task<string> CaptureThumbnailAsync(string elementId)
    {
        try
        {
            var module = await GetJSModule();
            return await module.InvokeAsync<string>("captureThumbnail", elementId);
        }
        catch
        {
            return string.Empty;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsModule != null)
        {
            await _jsModule.DisposeAsync();
        }
    }

}