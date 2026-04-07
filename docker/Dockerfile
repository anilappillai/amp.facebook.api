FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Amp.Facebook.Api/Amp.Facebook.Api.csproj", "src/Amp.Facebook.Api/"]
RUN dotnet restore "src/Amp.Facebook.Api/Amp.Facebook.Api.csproj"

COPY . .
WORKDIR "/src/src/Amp.Facebook.Api"
RUN dotnet build "Amp.Facebook.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Amp.Facebook.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Run as non-root user for EKS security best practices
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Amp.Facebook.Api.dll"]
