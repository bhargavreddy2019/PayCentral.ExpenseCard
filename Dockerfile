FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/PayCentral.WebApi/PayCentral.WebApi.csproj", "src/PayCentral.WebApi/"]
COPY ["src/PayCentral.Application/PayCentral.Application.csproj", "src/PayCentral.Application/"]
COPY ["src/PayCentral.Domain/PayCentral.Domain.csproj", "src/PayCentral.Domain/"]
COPY ["src/PayCentral.Infrastructure/PayCentral.Infrastructure.csproj", "src/PayCentral.Infrastructure/"]
RUN dotnet restore "src/PayCentral.WebApi/PayCentral.WebApi.csproj"
COPY . .
WORKDIR "/src/src/PayCentral.WebApi"
RUN dotnet build "PayCentral.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PayCentral.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PayCentral.WebApi.dll"]