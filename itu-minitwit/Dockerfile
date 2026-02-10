FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# 1. Copy everything first (simpler for multi-project debugging)
COPY . .

# 2. Move into the Web project folder
WORKDIR /source/src/MiniTwit.Web

# 3. Restore specifically for this project
RUN dotnet restore

# 4. Publish
# We explicitly point to the .csproj
RUN dotnet publish MiniTwit.Web.csproj -c Release -o /app

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .

# Ensure the app binds to the right port and interface
ENV ASPNETCORE_URLS=http://+:5273
EXPOSE 5273

ENTRYPOINT ["dotnet", "MiniTwit.Web.dll"]