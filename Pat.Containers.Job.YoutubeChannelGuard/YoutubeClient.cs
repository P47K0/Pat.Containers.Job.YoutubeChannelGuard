using System.Text.Json;
using Pat.Containers.Jobs.YoutubeChannelGuard.Models;

namespace Pat.Containers.Jobs.YoutubeChannelGuard;

public static class YoutubeClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static async Task<ChannelInfo?> GetChannelAsync(HttpClient http, Settings settings)
    {
        var identifierQuery = !string.IsNullOrWhiteSpace(settings.YoutubeChannelId)
            ? $"id={Uri.EscapeDataString(settings.YoutubeChannelId!)}"
            : $"forHandle={Uri.EscapeDataString(settings.YoutubeChannelHandle!)}";

        var url = $"https://www.googleapis.com/youtube/v3/channels?part=snippet,statistics,contentDetails&{identifierQuery}&key={Uri.EscapeDataString(settings.YoutubeApiKey)}";
        using var resp = await http.GetAsync(url);
        var json = await resp.Content.ReadAsStringAsync();
        resp.EnsureSuccessStatusCode();

        var data = JsonSerializer.Deserialize<ChannelsResponse>(json, JsonOptions);
        var item = data?.Items?.FirstOrDefault();
        if (item is null) return null;

        return new ChannelInfo(
            item.Id ?? string.Empty,
            item.Snippet?.Title ?? item.Id ?? "Unknown channel",
            int.TryParse(item.Statistics?.VideoCount, out var count) ? count : 0,
            item.ContentDetails?.RelatedPlaylists?.Uploads ?? string.Empty);
    }

    public static async Task<VideoInfo?> GetLatestVideoAsync(HttpClient http, Settings settings, string uploadsPlaylistId)
    {
        if (string.IsNullOrWhiteSpace(uploadsPlaylistId)) return null;

        var url = $"https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&playlistId={Uri.EscapeDataString(uploadsPlaylistId)}&maxResults=1&key={Uri.EscapeDataString(settings.YoutubeApiKey)}";
        using var resp = await http.GetAsync(url);
        var json = await resp.Content.ReadAsStringAsync();
        resp.EnsureSuccessStatusCode();

        var data = JsonSerializer.Deserialize<PlaylistItemsResponse>(json, JsonOptions);
        var item = data?.Items?.FirstOrDefault();
        var snippet = item?.Snippet;
        var videoId = snippet?.ResourceId?.VideoId;
        if (string.IsNullOrWhiteSpace(videoId) || string.IsNullOrWhiteSpace(snippet?.Title)) return null;

        return new VideoInfo(videoId, snippet.Title, snippet.PublishedAt ?? DateTime.MinValue);
    }

    private sealed class ChannelsResponse { public List<ChannelItem>? Items { get; set; } }
    private sealed class ChannelItem
    {
        public string? Id { get; set; }
        public Snippet? Snippet { get; set; }
        public Statistics? Statistics { get; set; }
        public ContentDetails? ContentDetails { get; set; }
    }
    private sealed class Snippet { public string? Title { get; set; } }
    private sealed class Statistics { public string? VideoCount { get; set; } }
    private sealed class ContentDetails { public RelatedPlaylists? RelatedPlaylists { get; set; } }
    private sealed class RelatedPlaylists { public string? Uploads { get; set; } }

    private sealed class PlaylistItemsResponse { public List<PlaylistItem>? Items { get; set; } }
    private sealed class PlaylistItem { public PlaylistSnippet? Snippet { get; set; } }
    private sealed class PlaylistSnippet
    {
        public string? Title { get; set; }
        public DateTime? PublishedAt { get; set; }
        public ResourceId? ResourceId { get; set; }
    }
    private sealed class ResourceId { public string? VideoId { get; set; } }
}

