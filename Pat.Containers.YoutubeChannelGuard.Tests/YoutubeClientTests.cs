using System.Net;
using System.Text;
using Pat.Containers.Jobs.YoutubeChannelGuard;
using Pat.Containers.Jobs.YoutubeChannelGuard.Models;
using Xunit;

namespace Pat.Containers.Jobs.YoutubeChannelGuard.Tests;

public class YoutubeClientTests
{
    [Fact]
    public async Task GetChannelAsync_Returns_ChannelInfo_From_ChannelId_Response()
    {
        var json = """
    {
        "items": [
        {
            "id": "UC123",
            "snippet": { "title": "Test Channel" },
            "statistics": { "videoCount": "2" },
            "contentDetails": { "relatedPlaylists": { "uploads": "UU123" } }
        }
        ]
    }
    """;

        using var handler = new FakeHttpMessageHandler(json);
        using var http = new HttpClient(handler);
        var settings = TestSettings(channelId: "UC123");

        var result = await YoutubeClient.GetChannelAsync(http, settings);

        Assert.NotNull(result);
        Assert.Equal("UC123", result!.ChannelId);
        Assert.Equal("Test Channel", result.Title);
        Assert.Equal(2, result.PublicVideoCount);
        Assert.Equal("UU123", result.UploadsPlaylistId);
        Assert.Contains("id=UC123", handler.LastRequestUri);
        Assert.DoesNotContain("forHandle=", handler.LastRequestUri);
    }

    [Fact]
    public async Task GetChannelAsync_Uses_Handle_When_ChannelId_Is_Not_Set()
    {
        var json = """
    {
        "items": [
        {
            "id": "UC999",
            "snippet": { "title": "Handle Channel" },
            "statistics": { "videoCount": "7" },
            "contentDetails": { "relatedPlaylists": { "uploads": "UU999" } }
        }
        ]
    }
    """;

        using var handler = new FakeHttpMessageHandler(json);
        using var http = new HttpClient(handler);
        var settings = TestSettings(handle: "myhandle");

        var result = await YoutubeClient.GetChannelAsync(http, settings);

        Assert.NotNull(result);
        Assert.Equal("UC999", result!.ChannelId);
        Assert.Contains("forHandle=myhandle", handler.LastRequestUri);
        Assert.DoesNotContain("id=", handler.LastRequestUri);
    }

    [Fact]
    public async Task GetLatestVideoAsync_Returns_Latest_VideoInfo()
    {
        var json = """
    {
        "items": [
        {
            "snippet": {
            "title": "My latest upload",
            "publishedAt": "2026-07-26T10:00:00Z",
            "resourceId": { "videoId": "abc123xyz" }
            }
        }
        ]
    }
    """;

        using var handler = new FakeHttpMessageHandler(json);
        using var http = new HttpClient(handler);
        var settings = TestSettings(channelId: "UC123");

        var result = await YoutubeClient.GetLatestVideoAsync(http, settings, "UU123");

        Assert.NotNull(result);
        Assert.Equal("abc123xyz", result!.VideoId);
        Assert.Equal("My latest upload", result.Title);
        Assert.Equal(DateTime.Parse("2026-07-26T10:00:00Z").ToUniversalTime(), result.PublishedAt.ToUniversalTime());
        Assert.Contains("playlistId=UU123", handler.LastRequestUri);
        Assert.Contains("maxResults=1", handler.LastRequestUri);
    }

    [Fact]
    public async Task GetLatestVideoAsync_Returns_Null_When_UploadsPlaylistId_Is_Blank()
    {
        using var handler = new FakeHttpMessageHandler("{ \"items\": [] }");
        using var http = new HttpClient(handler);
        var settings = TestSettings(channelId: "UC123");

        var result = await YoutubeClient.GetLatestVideoAsync(http, settings, "");

        Assert.Null(result);
        Assert.Null(handler.LastRequestUri);
    }

    [Fact]
    public async Task GetLatestVideoAsync_Returns_Null_When_VideoId_Is_Missing()
    {
        var json = """
    {
        "items": [
        {
            "snippet": {
            "title": "Incomplete upload",
            "publishedAt": "2026-07-26T10:00:00Z",
            "resourceId": { }
            }
        }
        ]
    }
    """;

        using var handler = new FakeHttpMessageHandler(json);
        using var http = new HttpClient(handler);
        var settings = TestSettings(channelId: "UC123");

        var result = await YoutubeClient.GetLatestVideoAsync(http, settings, "UU123");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetChannelAsync_Returns_Null_When_No_Items()
    {
        using var handler = new FakeHttpMessageHandler("{ \"items\": [] }");
        using var http = new HttpClient(handler);
        var settings = TestSettings(handle: "myhandle");

        var result = await YoutubeClient.GetChannelAsync(http, settings);

        Assert.Null(result);
    }

    private static Settings TestSettings(string? channelId = null, string? handle = null) => new(
        YoutubeApiKey: "fake-key",
        YoutubeChannelId: channelId,
        YoutubeChannelHandle: handle,
        AllowedVideoCount: 0,
        AcsConnectionString: "endpoint=https://example.communication.azure.com/;accesskey=fake",
        MailFrom: "DoNotReply@example.azurecomm.net",
        MailTo: "recipient@example.com");

    private sealed class FakeHttpMessageHandler(string responseBody) : HttpMessageHandler
    {
        public string? LastRequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri?.ToString();
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}