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

## Mimari (Katmanlı Yapı / Onion Yaklaşımı)

Bu projede **katmanlı (Onion benzeri) bir yapı** uygulanmıştır. Amaç; sorumlulukları ayrıştırmak, test edilebilirliği artırmak ve bağımlılıkları yönetilebilir hale getirmektir.

### Katmanlar

- **API (Presentation Layer)**  
  Controller’lar, Swagger/OpenAPI, middleware ve JSON ayarlarını içerir.  
  İş mantığı içermez; request alır, application servislerini çağırır ve response döner.

- **Application (Use Case Layer)**  
  İş kuralları, servisler (`CustomerService`, `OrderService`), DTO’lar ve FluentValidation doğrulamaları burada bulunur.  
  Uygulamanın ana akışı bu katmanda yönetilir.

- **Domain (Core Layer)**  
  Entity ve enum tanımları yer alır (örn. `Order`, `OrderItem`, `OrderStatus`).  
  Domain katmanı dış bağımlılık içermez.

- **Infrastructure (Data Access Layer)**  
  EF Core `DbContext`, migration’lar ve veritabanı erişim detayları bu katmanda bulunur.  
  Application katmanı veri erişimini `IAppDbContext` üzerinden yapar.

### Bağımlılık yönü
Bağımlılıklar “dıştan içe” olacak şekilde tasarlanmıştır:

`API → Application → Domain`

Infrastructure katmanı ise veri erişimi için kullanılır ve DI üzerinden uygulamaya entegre edilmiştir.

> Not: Entity sayısı az olduğu için AutoMapper kullanılmamış; DTO dönüşleri manuel mapping / projection (Select) ile yapılmıştır.

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

---

## Varsayımlar
- **TotalAmount** alanı sistem tarafından **OrderItems** üzerinden hesaplanır; client request’inden `totalAmount` alınmaz.
- `OrderDate` request’te gönderilmezse sistem saati baz alınarak **UTC** ile otomatik atanır.
- “Aynı gün içerisinde max 5 sipariş” kuralı, `OrderDate.Date` baz alınarak gün başlangıcı/bitişi üzerinden hesaplanmıştır (UTC).
- Projede entity sayısı az olduğu için **AutoMapper kullanılmamıştır**. DTO mapping işlemleri **manual mapping / projection (Select)** ile yapılmıştır.
- Case gereksinimlerinde **Authentication/Authorization** istenmediği için kullanıcı bilgisine bağlı audit alanları (`CreatedBy`, `UpdatedBy`, `DeletedBy`) eklenmemiştir.
- **Soft delete** gereksinimlerde olmadığı için uygulanmamıştır (`IsDeleted`, `DeletedDate` gibi alanlar eklenmemiştir). Silme operasyonu da ayrıca tanımlanmamıştır.
- Status güncelleme sadece `PATCH /api/orders/{id}/status` endpoint’i üzerinden yapılır; sipariş kalemlerini güncelleme/silme gibi ek endpoint’ler case kapsamı dışında tutulmuştur.
- `Cancelled` durumundaki siparişler için yalnızca status değişikliği engellenmiştir; listeleme ve detay görüntüleme serbesttir.
- Docker ile çalıştırma senaryosunda SQL Server kurulumu gerekmeyeceği varsayılmış, MSSQL container üzerinden çalışacak şekilde yapılandırılmıştır. Uygulama açılışta otomatik migration uygular.
