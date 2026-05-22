FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN groupadd -r app && useradd -r -g app app && chown -R app:app /app
USER app
COPY --from=build --chown=app:app /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=3s --start-period=15s --retries=3 \
  CMD wget --spider --quiet http://localhost:8080/healthz || exit 1
ENTRYPOINT ["dotnet", "SecureDocumentPortal.Api.dll"]
