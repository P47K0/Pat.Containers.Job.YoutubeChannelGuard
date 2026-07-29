using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using Pat.Containers.Jobs.YoutubeChannelGuard.Models;
using Pat.Containers.Jobs.YoutubeChannelGuard;

internal static partial class Program
{
    public static async Task<int> Main()
    {
        try
        {
            var settings = Settings.Load();
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

            var channel = await YoutubeClient.GetChannelAsync(http, settings);
            if (channel is null)
            {
                Console.Error.WriteLine("Channel not found or YouTube API returned no items.");
                return 2;
            }

            Console.WriteLine($"Channel: {channel.Title}");
            Console.WriteLine($"Public video count: {channel.PublicVideoCount}");
            Console.WriteLine($"Allowed video count: {settings.AllowedVideoCount}");

            if (channel.PublicVideoCount <= settings.AllowedVideoCount)
            {
                Console.WriteLine("No alert needed.");
                return 0;
            }

            var latestVideo = await YoutubeClient.GetLatestVideoAsync(http, settings, channel.UploadsPlaylistId);
            var subject = $"[ALERT] YouTube public video count exceeded for {channel.Title}";
            var body = BuildEmailBody(channel, latestVideo, settings);

            await EmailClient.SendAsync(settings, subject, body);
            Console.WriteLine("Alert email sent.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.ToString());
            return 1;
        }
    }

    private static string BuildEmailBody(ChannelInfo channel, VideoInfo? latestVideo, Settings settings)
    {
        var sb = new StringBuilder();
        sb.AppendLine("A YouTube channel exceeded the allowed number of public videos.");
        sb.AppendLine();
        sb.AppendLine($"Detected UTC: {DateTime.UtcNow:O}");
        sb.AppendLine($"Channel title: {channel.Title}");
        sb.AppendLine($"Channel id: {channel.ChannelId}");
        sb.AppendLine($"Channel url: https://www.youtube.com/channel/{channel.ChannelId}");
        sb.AppendLine($"Public video count: {channel.PublicVideoCount}");
        sb.AppendLine($"Allowed video count: {settings.AllowedVideoCount}");
        if (latestVideo is not null)
        {
            sb.AppendLine();
            sb.AppendLine("Latest public upload:");
            sb.AppendLine($"Title: {latestVideo.Title}");
            sb.AppendLine($"Published UTC: {latestVideo.PublishedAt:O}");
            sb.AppendLine($"Url: https://www.youtube.com/watch?v={latestVideo.VideoId}");
        }
        sb.AppendLine();
        sb.AppendLine("This message was sent by YoutubeChannelGuard running as an Azure Container Apps Job.");
        return sb.ToString();
    }
}
