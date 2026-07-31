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
    public static Settings Load(bool useAppSettings = false)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory);

        if (useAppSettings)
        {
            builder.AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: false);
        }

        builder.AddEnvironmentVariables();

        var configuration = builder.Build();

        var settings = configuration.GetSection("AppSettings").Get<Settings>()
            ?? throw new InvalidOperationException("Failed to load settings.");

        return settings;
    }
}

public sealed record ChannelInfo(string ChannelId, string Title, int PublicVideoCount, string UploadsPlaylistId);
public sealed record VideoInfo(string VideoId, string Title, DateTime PublishedAt);
