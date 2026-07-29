# YoutubeChannelGuard

Minimal C# console job for Azure Container Apps Jobs. It polls the YouTube Data API for a channel and sends an email alert when the number of public videos exceeds the allowed threshold.

## Features

- Poll by YouTube channel ID or handle.
- Check current public video count.
- Optional latest public upload lookup for better alert emails.
- SMTP support, including Microsoft 365 / Office 365 SMTP.
- Container-ready for Azure Container Apps Jobs.

## Required environment variables

- `YOUTUBE_API_KEY`
- `YOUTUBE_CHANNEL_ID` or `YOUTUBE_CHANNEL_HANDLE`
- `ALLOWED_VIDEO_COUNT` (default `0`)
- `SMTP_HOST`
- `SMTP_PORT` (default `587`)
- `SMTP_USE_SSL` (default `true`)
- `SMTP_USERNAME`
- `SMTP_PASSWORD`
- `MAIL_FROM`
- `MAIL_TO`

## Local run

```bash
cp appsettings.example.env .env
set -a && source .env && set +a
 dotnet run
```

## Build container locally

```bash
docker build -t youtube-channel-guard:latest .
```

## Suggested ACA Job flow

1. Build and push the image to ACR.
2. Create an ACA Job with schedule type.
3. Configure secrets for the YouTube API key and SMTP password.
4. Pass all config values as environment variables.
5. Run every 5 minutes or your preferred cron schedule.

## Example ACA CLI sketch

```bash
az containerapp job create \
  --name youtube-channel-guard \
  --resource-group <rg> \
  --environment <aca-env> \
  --trigger-type Schedule \
  --replica-timeout 180 \
  --replica-retry-limit 1 \
  --parallelism 1 \
  --completion-count 1 \
  --cron-expression "*/5 * * * *" \
  --image <acr>.azurecr.io/youtube-channel-guard:latest \
  --cpu 0.25 \
  --memory 0.5Gi \
  --secrets youtube-api-key=<value> smtp-password=<value> \
  --env-vars \
    YOUTUBE_API_KEY=secretref:youtube-api-key \
    YOUTUBE_CHANNEL_HANDLE=<handle> \
    ALLOWED_VIDEO_COUNT=0 \
    SMTP_HOST=smtp.office365.com \
    SMTP_PORT=587 \
    SMTP_USE_SSL=true \
    SMTP_USERNAME=<account@domain.com> \
    SMTP_PASSWORD=secretref:smtp-password \
    MAIL_FROM=<account@domain.com> \
    MAIL_TO=<recipient@domain.com>
```

## Notes

- `statistics.videoCount` from the YouTube Data API reflects the number of public uploaded videos.
- Version 1 sends an email every run while the public count is above the threshold. Add state storage later if you only want one alert per incident.
- For Microsoft 365, SMTP auth must be allowed for the mailbox or tenant.
