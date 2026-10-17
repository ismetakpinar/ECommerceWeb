<div align="center">

# 🛒 ECommerce

### ASP.NET Core 8 ile Geliştirilmiş E-Ticaret Web ve API Çözümü

MVC mağaza arayüzünü, katmanlı Web API ve SQL Server veri katmanıyla bir araya getiren e-ticaret uygulaması.

![.NET](https://img.shields.io/badge/.NET-8-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12-239120?style=for-the-badge&logo=csharp)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-CC2927?style=for-the-badge&logo=microsoftsqlserver)
![Tests](https://img.shields.io/badge/Tests-xUnit-success?style=for-the-badge)

</div>

---

# 📖 Proje Hakkında

ECommerce; ürün kataloğu, sepet, hesap, ödeme ve sipariş akışlarını içeren bir ASP.NET Core 8 çözümüdür. Web arayüzü API ile HTTP üzerinden haberleşir; API iş kurallarını ve SQL Server erişimini katmanlı mimaride yönetir.

# 🚀 Öne Çıkan Özellikler

- ASP.NET Core 8 MVC mağaza arayüzü
- Katmanlı ASP.NET Core Web API
- Ürün ve kategori kataloğu, arama ve sayfalama
- JWT kimlik doğrulama, refresh token ve rol tabanlı yetkilendirme
- Sepet, adres, checkout ve sipariş akışları
- Entity Framework Core ve SQL Server
- Repository, Unit of Work ve Specification yaklaşımları
- FluentValidation ve merkezi hata yönetimi
- Serilog, rate limiting ve health checks
- Swagger ve Scalar API dokümantasyonu
- Unit ve integration test projeleri

---

# 🏗️ Mimari

```text
		     Web Tarayıcısı
			    │
			    ▼
		 ECommerce.Web (MVC)
			    │ HTTP
			    ▼
		    ECommerceAPI
			    │
	  ┌─────────────┴─────────────┐
	  │                           │
 Business Layer              Data Access Layer
	  │                           │
	  └─────────────┬─────────────┘
			    │
		    SQL Server
```

---

# 📂 Proje Yapısı

```text
ECommerce
├── ECommerce.Web
│   └── ECommerce.Web
└── ECommerceAPI
    ├── ECommerceAPI
    ├── ECommerceAPI.Business
    ├── ECommerceAPI.DataAccess
    ├── ECommerceAPI.Dto
    ├── ECommerceAPI.Entity
    ├── ECommerceAPI.Tests
    └── ECommerceAPI.IntegrationTests
```

---

# 🔐 Kimlik Doğrulama

API'de JWT Bearer authentication kullanılır. Desteklenen işlemler:

- Kullanıcı kaydı ve girişi
- Refresh token yenileme
- Oturum kapatma
- Role-based authorization

# 📦 Modüller

- Authentication
- Product ve Category
- Customer ve Address
- Cart ve Checkout
- Order

---

# ⚙️ Kullanılan Teknolojiler

| Teknoloji | Kullanım |
|-----------|----------|
| ASP.NET Core 8 | MVC web ve Web API |
| Entity Framework Core | ORM ve migration yönetimi |
| SQL Server | İlişkisel veritabanı |
| AutoMapper | Nesne dönüşümleri |
| FluentValidation | İstek doğrulama |
| Serilog | Yapılandırılmış loglama |
| xUnit, Moq | Test altyapısı |
| Docker Compose | API geliştirme ortamı |

---

# 🐳 Docker ile Çalıştırma

API'nin Docker yapılandırmasını kullanmak için:

```powershell
cd ECommerceAPI
docker compose up --build
```

# 🚀 Kurulum ve Çalıştırma

## Gereksinimler

- .NET 8 SDK
- SQL Server veya SQL Server Express
- HTTPS geliştirme sertifikası: `dotnet dev-certs https --trust`

## Yapılandırma

API varsayılan olarak `localhost\SQLEXPRESS` üzerindeki `ECommerceDb` veritabanını kullanır. Ortama göre `ConnectionStrings__DefaultConnection` değerini ayarlayın.

JWT anahtarı repoda tutulmaz. Yerel geliştirme için User Secrets kullanın:

```powershell
dotnet user-secrets set "Jwt:Key" "en-az-32-karakterlik-yerel-gizli-anahtar" --project ECommerceAPI/ECommerceAPI/ECommerceAPI.csproj
```

Yayında `Jwt__Key`, `Jwt__Issuer` ve `Jwt__Audience` değerlerini secret store veya ortam değişkenleriyle sağlayın.

## Uygulamayı Başlatma

Önce API'yi başlatın:

```powershell
dotnet run --project ECommerceAPI/ECommerceAPI/ECommerceAPI.csproj --launch-profile https
```

API varsayılan olarak `https://localhost:7120` adresinde çalışır. Başka bir terminalde web uygulamasını başlatın:

```powershell
dotnet run --project ECommerce.Web/ECommerce.Web/ECommerce.Web.csproj --launch-profile https
```

Mağaza `https://localhost:7027` adresindedir. Web API adresi `ECommerce.Web/ECommerce.Web/appsettings.json` içindeki `ApiSettings:BaseUrl` ayarından gelir.

---

# 🧪 Testler

Çözümde unit ve integration test projeleri bulunur. Testleri çalıştırmak için:

```powershell
dotnet test ECommerce.slnx
```

> Integration testleri `ECommerceDb_Test` veritabanını her test öncesinde silip yeniden oluşturur. Test ayarlarının gerçek veya geliştirme veritabanına yönelmediğinden emin olun.

---

# 📄 Lisans

MIT. Ayrıntılar için [LICENSE](ECommerceAPI/LICENSE) dosyasına bakın.