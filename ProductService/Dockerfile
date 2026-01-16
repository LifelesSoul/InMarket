FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["ProductService/ProductService.csproj", "ProductService/"]
COPY ["ProductService.BLL/ProductService.BLL.csproj", "ProductService.BLL/"]
COPY ["ProductService.DAL/ProductService.DAL.csproj", "ProductService.DAL/"]
COPY ["ProductService.Domain/ProductService.Domain.csproj", "ProductService.Domain/"]

RUN dotnet restore "ProductService/ProductService.csproj"

COPY . .

WORKDIR "/src/ProductService"
RUN dotnet build "ProductService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ProductService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

EXPOSE 8080

USER app

ENTRYPOINT ["dotnet", "ProductService.dll"]
