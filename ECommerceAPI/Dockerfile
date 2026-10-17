# -----------------------------
# 1. RUNTIME
# -----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app

EXPOSE 8080


# -----------------------------
# 2. BUILD
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Önce yalnızca proje dosyalarını kopyalıyoruz.
# Böylece NuGet restore katmanı Docker cache tarafından kullanılabilir.
COPY ["ECommerceAPI/ECommerceAPI.csproj", "ECommerceAPI/"]
COPY ["ECommerceAPI.Business/ECommerceAPI.Business.csproj", "ECommerceAPI.Business/"]
COPY ["ECommerceAPI.DataAccess/ECommerceAPI.DataAccess.csproj", "ECommerceAPI.DataAccess/"]
COPY ["ECommerceAPI.Dto/ECommerceAPI.Dto.csproj", "ECommerceAPI.Dto/"]
COPY ["ECommerceAPI.Entity/ECommerceAPI.Entity.csproj", "ECommerceAPI.Entity/"]

RUN dotnet restore "ECommerceAPI/ECommerceAPI.csproj"

# Solution içindeki tüm kaynak kodları kopyalıyoruz.
COPY . .

WORKDIR "/src/ECommerceAPI"

RUN dotnet build "ECommerceAPI.csproj" \
    -c Release \
    -o /app/build \
    --no-restore


# -----------------------------
# 3. PUBLISH
# -----------------------------
FROM build AS publish

RUN dotnet publish "ECommerceAPI.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false


# -----------------------------
# 4. FINAL
# -----------------------------
FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ECommerceAPI.dll"]