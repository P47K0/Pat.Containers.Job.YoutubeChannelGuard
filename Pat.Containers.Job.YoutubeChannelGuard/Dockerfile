FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Pat.Containers.Job.YoutubeChannelGuard.csproj", "Pat.Containers.Job.YoutubeChannelGuard/]
RUN dotnet restore "Pat.Containers.Job.YoutubeChannelGuard.csproj"

COPY . .

# Build
WORKDIR "/src/Pat.Containers.Job.YoutubeChannelGuard"
RUN dotnet build "Pat.Containers.Job.YoutubeChannelGuard.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Pat.Containers.Job.YoutubeChannelGuard.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Pat.Containers.Job.YoutubeChannelGuard.dll"]
