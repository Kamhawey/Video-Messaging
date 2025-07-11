namespace SharedClassLibrary.Models;

public class VideoFileData
{
    public int[] Data { get; set; } = Array.Empty<int>();
    public string MimeType { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public long Size { get; set; }
}