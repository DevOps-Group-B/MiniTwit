# Build Stage (stays the same)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source
COPY . .
WORKDIR /source/src/MiniTwit.Web
RUN dotnet restore && dotnet publish -c Release -o /app

# Stage 2: Runtime  
# "chiseled" images have no shell and a much smaller attack surface
FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled
WORKDIR /app
COPY --from=build /app .

# Run as a non-privileged user (Chiseled images do this by default, but it's good to be explicit)
USER $APP_UID

ENV ASPNETCORE_URLS=http://+:5273
EXPOSE 5273
ENTRYPOINT ["dotnet", "MiniTwit.Web.dll"]