FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Core/LmsAndOnlineCoursesMarketplace.Application/LmsAndOnlineCoursesMarketplace.Application.csproj", "Core/LmsAndOnlineCoursesMarketplace.Application/"]
COPY ["Core/LmsAndOnlineCoursesMarketplace.Domain/LmsAndOnlineCoursesMarketplace.Domain.csproj", "Core/LmsAndOnlineCoursesMarketplace.Domain/"]
COPY ["Infrastrucutre/LmsAndOnlineCoursesMarketplace.Infrastructure/LmsAndOnlineCoursesMarketplace.Infrastructure.csproj", "Infrastrucutre/LmsAndOnlineCoursesMarketplace.Infrastructure/"]
COPY ["Infrastrucutre/LmsAndOnlineCoursesMarketplace.Persistence/LmsAndOnlineCoursesMarketplace.Persistence.csproj", "Infrastrucutre/LmsAndOnlineCoursesMarketplace.Persistence/"]
COPY ["LmsAndOnlineCoursesMarketplace.MVC/LmsAndOnlineCoursesMarketplace.MVC.csproj", "LmsAndOnlineCoursesMarketplace.MVC/"]

RUN dotnet restore "LmsAndOnlineCoursesMarketplace.MVC/LmsAndOnlineCoursesMarketplace.MVC.csproj"

COPY . .

WORKDIR "/src/LmsAndOnlineCoursesMarketplace.MVC"
RUN dotnet build "LmsAndOnlineCoursesMarketplace.MVC.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LmsAndOnlineCoursesMarketplace.MVC.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LmsAndOnlineCoursesMarketplace.MVC.dll"]