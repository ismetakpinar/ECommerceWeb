<div align="center">

# 🛒 ECommerceAPI

### ASP.NET Core 8 ile Geliştirilmiş Katmanlı Mimariye Sahip E-Ticaret Web API Projesi

Modern yazılım geliştirme prensipleri kullanılarak geliştirilen, güvenlik, sürdürülebilirlik ve test edilebilirlik odaklı örnek bir **E-Ticaret Backend** projesidir.

![.NET](https://img.shields.io/badge/.NET-8-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12.0-239120?style=for-the-badge&logo=csharp)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoftsqlserver)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker)
![Tests](https://img.shields.io/badge/Tests-355%20Passed-success?style=for-the-badge)
![License](https://img.shields.io/badge/License-MIT-blue?style=for-the-badge)

</div>

---

# 📖 Proje Hakkında

**ECommerceAPI**, ASP.NET Core 8 kullanılarak geliştirilmiş katmanlı mimariye sahip örnek bir **E-Ticaret Web API** projesidir.

Proje geliştirilirken;

- Temiz kod prensipleri
- SOLID prensipleri
- Katmanlı mimari
- Güvenli kimlik doğrulama
- Performans
- Test edilebilirlik

ön planda tutulmuştur.

---

# 🚀 Öne Çıkan Özellikler

- ASP.NET Core 8 Web API
- Katmanlı Mimari (N-Tier Architecture)
- Entity Framework Core
- SQL Server
- JWT Authentication
- Refresh Token Rotation
- Role Based Authorization
- Repository Pattern
- Unit Of Work Pattern
- Specification Pattern
- AutoMapper
- FluentValidation
- Global Exception Middleware
- Standart API Response Yapısı
- Soft Delete
- Global Query Filters
- Pagination
- IMemoryCache
- Serilog Logging
- Docker Desteği
- Swagger
- Scalar API Dokümantasyonu
- Unit Test
- Integration Test

---

# 🏗️ Mimari

```text
                   Client
                      │
                      ▼
          ASP.NET Core Web API
                      │
        ┌─────────────┴─────────────┐
        │                           │
 Business Layer              Data Access Layer
        │                           │
        └─────────────┬─────────────┘
                      │
                 Entity Layer
                      │
                 SQL Server
```

---

# 📂 Proje Yapısı

```text
ECommerceAPI
│
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

Projede **JWT Bearer Authentication** kullanılmaktadır.

Desteklenen işlemler:

- Kullanıcı Kaydı
- Kullanıcı Girişi
- Refresh Token
- Logout

---

# 📦 Modüller

- Authentication
- Product
- Category
- Customer
- Address
- Cart
- Order

---

# 🧪 Testler

Projede kapsamlı **Unit Test** ve **Integration Test** altyapısı bulunmaktadır.

| Test Türü | Durum |
|-----------|:-----:|
| Unit Test | ✅ |
| Integration Test | ✅ |
| Toplam Test | **355** |
| Başarılı | **355** |
| Başarısız | **0** |

---

# ⚙️ Kullanılan Teknolojiler

| Teknoloji | Açıklama |
|------------|----------|
| ASP.NET Core 8 | Web API |
| Entity Framework Core | ORM |
| SQL Server | Veritabanı |
| AutoMapper | Nesne Dönüştürme |
| FluentValidation | Doğrulama |
| Serilog | Loglama |
| Docker | Container |
| Swagger | API Dokümantasyonu |
| Scalar | API Dokümantasyonu |
| xUnit | Unit Test |
| Moq | Mock Framework |
| FluentAssertions | Test Doğrulamaları |

---

# 🐳 Docker ile Çalıştırma

```bash
docker compose up --build
```

---

# 🚀 Kurulum

### Repoyu klonlayın

```bash
git clone https://github.com/KULLANICI_ADIN/ECommerceAPI.git
```

### Proje klasörüne girin

```bash
cd ECommerceAPI
```

### NuGet paketlerini yükleyin

```bash
dotnet restore
```

### Veritabanını oluşturun

```bash
dotnet ef database update
```

### Uygulamayı çalıştırın

```bash
dotnet run
```

---

# 🧪 Testleri Çalıştırma

```bash
dotnet test
```

---

# 📌 Yazılım Prensipleri

Bu proje geliştirilirken aşağıdaki yaklaşımlar uygulanmıştır.

- Clean Architecture
- SOLID Prensipleri
- Repository Pattern
- Unit Of Work Pattern
- Specification Pattern
- Dependency Injection
- DTO Kullanımı
- AutoMapper
- FluentValidation
- Merkezi Hata Yönetimi
- Merkezi Loglama
- Soft Delete
- Pagination
- Docker Desteği
- Unit Test
- Integration Test

---

# 🛣️ Yol Haritası

Bu proje geliştirilmeye devam etmektedir.

Planlanan geliştirmeler:

- [ ] ASP.NET Core MVC Yönetim Paneli
- [ ] ASP.NET Core MVC E-Ticaret Web Arayüzü
- [ ] Redis Cache Desteği
- [ ] Hangfire ile Arka Plan Görevleri
- [ ] Dashboard ve Raporlama
- [ ] GitHub Actions ile CI/CD sürecinin geliştirilmesi

---

# 👨‍💻 Geliştirici

Bu proje, ASP.NET Core ekosisteminde modern backend geliştirme yaklaşımlarını uygulamak ve kendimi geliştirmek amacıyla hazırlanmıştır.

Görüş, öneri ve katkılarınızı memnuniyetle karşılarım.

---

# 📄 Lisans

Bu proje **MIT License** ile lisanslanmıştır.

---

<div align="center">

### ⭐ Projeyi faydalı bulduysanız yıldız vermeyi unutmayın.

</div>