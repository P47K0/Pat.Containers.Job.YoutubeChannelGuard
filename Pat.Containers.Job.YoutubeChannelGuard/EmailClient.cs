using Azure;
using Azure.Communication.Email;
using Pat.Containers.Jobs.YoutubeChannelGuard.Models;

namespace Pat.Containers.Jobs.YoutubeChannelGuard;

public static class EmailClient
{
    public static async Task SendAsync(Settings settings, string subject, string body)
    {
        var client = new Azure.Communication.Email.EmailClient(settings.AcsConnectionString);
        var message = new EmailMessage(
            senderAddress: settings.MailFrom,
            content: new EmailContent(subject)
            {
                PlainText = body
            },
            recipients: new EmailRecipients(new[]
            {
                new EmailAddress(settings.MailTo)
            }));

        EmailSendOperation operation = await client.SendAsync(WaitUntil.Completed, message);
        Console.WriteLine($"ACS Email operation id: {operation.Id}");
    }
}
