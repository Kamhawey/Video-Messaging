using KristofferStrube.Blazor.MediaCaptureStreams;
using Microsoft.Extensions.DependencyInjection;
using Video.Messaging.App.Services;

namespace Video.Messaging.App.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVideoMessagingServices(this IServiceCollection services)
    {
        services.AddScoped<IVideoService, VideoService>();
        services.AddScoped<IVideoPlayerService, VideoPlayerService>();
        services.AddScoped<IJSVideoService, JSVideoService>();

        services.AddMediaDevicesService();


        return services;
    }
}