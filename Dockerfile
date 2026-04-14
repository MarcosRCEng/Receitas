FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src

COPY ["src/Backend/MyRecipeBook.API/MyRecipeBook.API.csproj", "src/Backend/MyRecipeBook.API/"]
COPY ["src/Backend/MyRecipeBook.Application/MyRecipeBook.Application.csproj", "src/Backend/MyRecipeBook.Application/"]
COPY ["src/Backend/MyRecipeBook.Domain/MyRecipeBook.Domain.csproj", "src/Backend/MyRecipeBook.Domain/"]
COPY ["src/Backend/MyRecipeBook.Infrastructure/MyRecipeBook.Infrastructure.csproj", "src/Backend/MyRecipeBook.Infrastructure/"]
COPY ["src/Shared/MyRecipeBook.Communication/MyRecipeBook.Communication.csproj", "src/Shared/MyRecipeBook.Communication/"]
COPY ["src/Shared/MyRecipeBook.Exceptions/MyRecipeBook.Exceptions.csproj", "src/Shared/MyRecipeBook.Exceptions/"]

RUN dotnet restore "src/Backend/MyRecipeBook.API/MyRecipeBook.API.csproj"

FROM restore AS build
ARG BUILD_CONFIGURATION=Release

COPY src/ ./src/

WORKDIR /src/src/Backend/MyRecipeBook.API
RUN dotnet build "MyRecipeBook.API.csproj" -c $BUILD_CONFIGURATION --no-restore

FROM build AS publish
RUN dotnet publish "MyRecipeBook.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-build /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "MyRecipeBook.API.dll"]
