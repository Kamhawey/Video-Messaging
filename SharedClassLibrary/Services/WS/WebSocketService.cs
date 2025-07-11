using Microsoft.Extensions.Options;
using SharedClassLibrary.Config;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Video.Messaging.App.Models;

namespace SharedClassLibrary.Services.WS;


public interface IWebSocketService
{
    Task ConnectAsync(string userId);
    Task DisconnectAsync();
    Task SendVideoMessageAsync(VideoMessage message);
    event Action<VideoMessage> OnVideoMessageReceived;
    event Action<WebSocketConnectionStatus> OnConnectionStatusChanged;
    WebSocketConnectionStatus ConnectionStatus { get; }
}

public enum WebSocketConnectionStatus
{
    Disconnected,
    Connecting,
    Connected,
    Reconnecting,
    Failed
}

public class WebSocketService : IWebSocketService
{
    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly string _baseUrl;
    private string? _userId;
    private Timer? _heartbeatTimer;

    public event Action<VideoMessage>? OnVideoMessageReceived;
    public event Action<WebSocketConnectionStatus>? OnConnectionStatusChanged;

    private WebSocketConnectionStatus _connectionStatus = WebSocketConnectionStatus.Disconnected;
    public WebSocketConnectionStatus ConnectionStatus
    {
        get => _connectionStatus;
        private set
        {
            if (_connectionStatus != value)
            {
                _connectionStatus = value;
                OnConnectionStatusChanged?.Invoke(value);
            }
        }
    }

    public WebSocketService(IOptions<WebSocketSettings> options)
    {
        _baseUrl = options.Value.BaseUrl;
    }
    public async Task ConnectAsync(string userId)
    {
        if (_webSocket?.State == WebSocketState.Open)
        {
            await DisconnectAsync();
        }

        _userId = userId;
        ConnectionStatus = WebSocketConnectionStatus.Connecting;

        try
        {
            _webSocket = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();

            _webSocket.Options.SetRequestHeader("ngrok-skip-browser-warning", "true");

            var uri = new Uri($"{_baseUrl}{userId}");
            await _webSocket.ConnectAsync(uri, _cancellationTokenSource.Token);

            ConnectionStatus = WebSocketConnectionStatus.Connected;

            _ = Task.Run(ListenForMessages);

            _heartbeatTimer = new Timer(SendHeartbeat, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }
        catch (Exception ex)
        {
            ConnectionStatus = WebSocketConnectionStatus.Failed;
            Console.WriteLine($"WebSocket connection failed: {ex.Message}");
        }
    }

    public async Task DisconnectAsync()
    {
        ConnectionStatus = WebSocketConnectionStatus.Disconnected;

        _heartbeatTimer?.Dispose();
        _cancellationTokenSource?.Cancel();

        if (_webSocket?.State == WebSocketState.Open)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", CancellationToken.None);
        }

        _webSocket?.Dispose();
        _cancellationTokenSource?.Dispose();
    }

    public async Task SendVideoMessageAsync(VideoMessage message)
    {
        if (_webSocket?.State != WebSocketState.Open)
        {
            throw new InvalidOperationException("WebSocket is not connected");
        }

        var json = JsonSerializer.Serialize(message);
        var bytes = Encoding.UTF8.GetBytes(json);
        var buffer = new ArraySegment<byte>(bytes);

        await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private async Task ListenForMessages()
    {
        var buffer = new byte[1024 * 4];

        try
        {
            while (_webSocket?.State == WebSocketState.Open && !_cancellationTokenSource!.Token.IsCancellationRequested)
            {
                var result = await _webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    _cancellationTokenSource.Token);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    try
                    {
                        var videoMessage = JsonSerializer.Deserialize<VideoMessage>(message);
                        if (videoMessage != null)
                        {
                            OnVideoMessageReceived?.Invoke(videoMessage);
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Failed to deserialize message: {ex.Message}");
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    ConnectionStatus = WebSocketConnectionStatus.Disconnected;
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (WebSocketException ex)
        {
            Console.WriteLine($"WebSocket error: {ex.Message}");
            ConnectionStatus = WebSocketConnectionStatus.Failed;

            if (!_cancellationTokenSource!.Token.IsCancellationRequested)
            {
                await AttemptReconnect();
            }
        }
    }

    private async Task AttemptReconnect()
    {
        if (string.IsNullOrEmpty(_userId))
            return;

        ConnectionStatus = WebSocketConnectionStatus.Reconnecting;

        // Wait before attempting to reconnect
        await Task.Delay(5000);

        try
        {
            await ConnectAsync(_userId);
        }
        catch
        {
            // Reconnection failed, will try again if another message comes in
            ConnectionStatus = WebSocketConnectionStatus.Failed;
        }
    }

    private async void SendHeartbeat(object? state)
    {
        if (_webSocket?.State == WebSocketState.Open)
        {
            try
            {
                var heartbeat = JsonSerializer.Serialize(new { type = "heartbeat", timestamp = DateTime.UtcNow });
                var bytes = Encoding.UTF8.GetBytes(heartbeat);
                var buffer = new ArraySegment<byte>(bytes);

                await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Heartbeat failed: {ex.Message}");
            }
        }
    }

    public void Dispose()
    {
        _heartbeatTimer?.Dispose();
        _cancellationTokenSource?.Cancel();
        _webSocket?.Dispose();
        _cancellationTokenSource?.Dispose();
    }
}