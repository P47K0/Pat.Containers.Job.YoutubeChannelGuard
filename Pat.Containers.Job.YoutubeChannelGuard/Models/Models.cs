using Microsoft.Extensions.Configuration;

namespace Pat.Containers.Jobs.YoutubeChannelGuard.Models;

public sealed record Settings(
    string YoutubeApiKey,
    string? YoutubeChannelId,
    string? YoutubeChannelHandle,
    int AllowedVideoCount,
    string AcsConnectionString,
    string MailFrom,
    string MailTo)
{
    public static Settings Load()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var settings = configuration.GetSection("AppSettings").Get<Settings>()
            ?? throw new InvalidOperationException("Failed to load settings.");

        return settings;
    }
}

public sealed record ChannelInfo(string ChannelId, string Title, int PublicVideoCount, string UploadsPlaylistId);
public sealed record VideoInfo(string VideoId, string Title, DateTime PublishedAt);
