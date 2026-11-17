# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG VERSION=0.0.0-dev
WORKDIR /src

# Copy only project files first so the restore layer is cached
COPY OCID.slnx ./
COPY OCID.Api/OCID.Api.csproj OCID.Api/
COPY OCID.Application/OCID.Application.csproj OCID.Application/
COPY OCID.Domain/OCID.Domain.csproj OCID.Domain/
COPY OCID.Infrastructure/OCID.Infrastructure.csproj OCID.Infrastructure/
COPY OCID.Messaging/OCID.Messaging.csproj OCID.Messaging/
RUN dotnet restore OCID.Api/OCID.Api.csproj

COPY . .
RUN dotnet publish OCID.Api/OCID.Api.csproj -c Release -o /app/publish --no-restore -p:Version=${VERSION}

# ---------- Production stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS production
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "OCID.Api.dll"]