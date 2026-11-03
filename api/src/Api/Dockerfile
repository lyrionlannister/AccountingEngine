FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/Api/Api.csproj src/Api/
COPY src/Accounting/Domain/Accounting.Domain.csproj src/Accounting/Domain/
COPY src/Accounting/Accounting.csproj src/Accounting/
COPY src/Auth/Domain/Auth.Domain.csproj src/Auth/Domain/
COPY src/Auth/Auth.csproj src/Auth/
COPY src/Companies/Domain/Companies.Domain.csproj src/Companies/Domain/
COPY src/Companies/Companies.csproj src/Companies/
COPY src/Users/Domain/Users.Domain.csproj src/Users/Domain/
COPY src/Users/Users.csproj src/Users/
COPY src/Shared/Shared.csproj src/Shared/
RUN dotnet restore src/Api/Api.csproj

COPY src/ src/
RUN dotnet publish src/Api/Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]
