# ECommerce

ASP.NET Core 8 tabanli e-ticaret uygulamasi. Cozum; MVC web arayuzu ve katmanli Web API'den olusur.

## Projeler

- `ECommerce.Web`: MVC tabanli magaza arayuzu.
- `ECommerceAPI`: Web API, Business, DataAccess, DTO ve Entity katmanlari ile test projeleri.

## Gereksinimler

- .NET 8 SDK
- SQL Server veya SQL Server Express
- API ve web icin HTTPS gelistirme sertifikasi (`dotnet dev-certs https --trust`)

## Yapilandirma

API, varsayilan olarak `localhost\SQLEXPRESS` uzerindeki `ECommerceDb` veritabanini kullanir. Baglanti dizesini `ECommerceAPI/ECommerceAPI/appsettings.Development.json` dosyasinda veya `ConnectionStrings__DefaultConnection` ortam degiskeninde belirleyin.

JWT anahtari repoda tutulmaz. Yerel gelistirme icin:

```powershell
dotnet user-secrets set "Jwt:Key" "en-az-32-karakterlik-yerel-gizli-anahtar" --project ECommerceAPI/ECommerceAPI/ECommerceAPI.csproj
```

Yayinda `Jwt__Key`, `Jwt__Issuer` ve `Jwt__Audience` degerlerini secret store veya ortam degiskenleriyle saglayin.

## Calistirma

Once API'yi baslatin:

```powershell
dotnet run --project ECommerceAPI/ECommerceAPI/ECommerceAPI.csproj
```

Ardindan baska bir terminalde MVC web uygulamasini baslatin:

```powershell
dotnet run --project ECommerce.Web/ECommerce.Web/ECommerce.Web.csproj
```

API gelistirme profilinde Scalar dokumantasyonu, web uygulamasi ise `https://localhost:7027` adresinde acilir. API adresi `ECommerce.Web/ECommerce.Web/appsettings.json` icindeki `ApiSettings:BaseUrl` ayarindan gelir.

## Derleme ve test

```powershell
dotnet build ECommerce.slnx
dotnet test ECommerce.slnx
```

Entegrasyon testleri `ECommerceDb_Test` veritabanini silip yeniden olusturur. Bu nedenle test ayarlarinin yalnizca test veritabanina baktigindan emin olun.