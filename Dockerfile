FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["CodeOrbit.API/CodeOrbit.API.csproj", "CodeOrbit.API/"]
COPY ["CodeOrbit.Application/CodeOrbit.Application.csproj", "CodeOrbit.Application/"]
COPY ["CodeOrbit.Domain/CodeOrbit.Domain.csproj", "CodeOrbit.Domain/"]
COPY ["CodeOrbit.Infrastructure/CodeOrbit.Infrastructure.csproj", "CodeOrbit.Infrastructure/"]
RUN dotnet restore "CodeOrbit.API/CodeOrbit.API.csproj"
COPY . .
WORKDIR "/src/CodeOrbit.API"
RUN dotnet build "CodeOrbit.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CodeOrbit.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CodeOrbit.API.dll"]
