# PozitifWeb.OrderApp

Sipariş Takip & Operasyon Yönetimi REST API

---

## 1) Docker ile çalıştırma (Önerilen)

### Gereksinimler
- Docker Desktop

### Çalıştırma
Proje root dizininde (Dockerfile ve docker-compose.yml’nin bulunduğu dizin):

```bash
docker compose up --build
```

Swagger:
- http://localhost:8080/swagger

> Not: Uygulama açılışta otomatik migration uygular ve veritabanı tablolarını oluşturur.  
> Bu nedenle bilgisayarınızda ayrıca SQL Server kurulu olmasına gerek yoktur.

### Durdurma
```bash
docker compose down
```

### Reset (opsiyonel)
Tüm veritabanı verisini sıfırlamak isterseniz:

```bash
docker compose down -v
```

---

## 2) Local ile çalıştırma (Docker olmadan)

### Gereksinimler
- .NET SDK 10
- MSSQL (LocalDB veya SQL Server)
- EF Core Tools

### appsettings.json
`PozitifWeb.OrderApp.Api/appsettings.json` içindeki connection string’i kendi MSSQL’inize göre ayarlayın.

### Migration & Run
```bash
dotnet restore
dotnet ef database update --project PozitifWeb.OrderApp.Infrastructure --startup-project PozitifWeb.OrderApp.Api
dotnet run --project PozitifWeb.OrderApp.Api
```

Swagger:
- https://localhost:<port>/swagger

---

## Teknolojiler
- ASP.NET Core (.NET 10)
- C# 14
- Entity Framework Core
- MSSQL
- FluentValidation
- Swagger (OpenAPI)
- Docker / Docker Compose
- xUnit (Unit Test)

---

## Mimari
Onion mimari yapı uygulanmıştır:

- **API**: Controller’lar, middleware, Swagger
- **Application**: DTO’lar, servisler, validator’lar, business rule’lar
- **Domain**: Entity ve Enum tanımları
- **Infrastructure**: EF Core DbContext, migration’lar
- **Tests**: Unit testler

---

## İş Kuralları
- Bir sipariş en az 1 adet **OrderItem** içermelidir.
- **TotalAmount**, OrderItems üzerinden hesaplanır (request’ten alınmaz).
- **Cancelled** durumundaki siparişler güncellenemez.
- Aynı müşteri aynı gün içerisinde en fazla **5 sipariş** oluşturabilir.
- Status geçişleri:
  - `Pending → Completed / Cancelled`
  - `Completed → değişemez`

---

## Global Exception Handling
Uygulamada tüm hatalar merkezi olarak `ExceptionHandlingMiddleware` üzerinden yönetilir:

- Validation hataları: `400 BadRequest`
- NotFound hataları: `404 NotFound`
- Business rule ihlalleri: `400 BadRequest`
- Beklenmeyen hatalar: `500 InternalServerError`

---

## Validation
FluentValidation ile request doğrulamaları yapılmaktadır.
