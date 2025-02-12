#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Kilde.WebAPI/Kilde.WebAPI.csproj", "Kilde.WebAPI/"]
RUN dotnet restore "Kilde.WebAPI/Kilde.WebAPI.csproj"
COPY . .
WORKDIR "/src/Kilde.WebAPI"
RUN dotnet build "Kilde.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Kilde.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Kilde.WebAPI.dll"]