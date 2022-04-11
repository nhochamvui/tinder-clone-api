#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
ENV ASPNETCORE_ENVIRONMENT="Development"
WORKDIR /app
EXPOSE 5000
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["TinderClone.csproj", "."]
RUN dotnet restore "./TinderClone.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "TinderClone.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TinderClone.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TinderClone.dll"]
#CMD ASPNETCORE_URLS=http://*:$PORT dotnet TinderClone.dll
