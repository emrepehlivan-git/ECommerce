# ECommerce Platform - Product Requirements Document (PRD)

## 1. Ürün Genel Bakışı

### 1.1 Vizyon
Modern e-ticaret ihtiyaçlarını karşılayan, ölçeklenebilir, güvenli ve yüksek performanslı bir e-ticaret platformu geliştirmek.

### 1.2 Ürün Tanımı
ECommerce Platform, kullanıcıların ürün katalogunu görüntüleyebileceği, sepete ürün ekleyebileceği, sipariş verebileceği ve yöneticilerin ürün, kategori, sipariş ve kullanıcı yönetimini yapabileceği kapsamlı bir e-ticaret çözümüdür.

### 1.3 Hedef Kitle
- **B2C Müşteriler**: Online alışveriş yapmak isteyen bireysel kullanıcılar
- **İşletmeler**: Ürünlerini online satmak isteyen küçük-orta ölçekli işletmeler
- **Yöneticiler**: Platform yönetimi yapan admin kullanıcılar

## 2. Teknik Mimari

### 2.1 Teknoloji Stack'i
- **Framework**: ASP.NET Core 8.0
- **Dil**: C# 12
- **Veritabanı**: PostgreSQL
- **ORM**: Entity Framework Core 8
- **Mimari**: Onion Architecture (Clean Architecture)
- **CQRS**: MediatR
- **Doğrulama**: FluentValidation
- **Kimlik Doğrulama**: OpenIddict + ASP.NET Identity
- **Önbellekleme**: Redis
- **Loglama**: Serilog + Seq
- **Test**: xUnit, Testcontainers
- **Containerization**: Docker & Docker Compose

### 2.2 Mimari Katmanlar
```
├── Core
│   ├── ECommerce.Domain (Entities, ValueObjects, Events)
│   ├── ECommerce.Application (CQRS, DTOs, Validators, Services)
│   └── ECommerce.SharedKernel (Shared abstractions)
├── Infrastructure
│   ├── ECommerce.AuthServer (OpenIddict authorization server)
│   ├── ECommerce.Infrastructure (External services)
│   └── ECommerce.Persistence (EF Core DbContext, repositories)
└── Presentation
    └── ECommerce.WebAPI (RESTful API)
```

## 3. Ana Özellikler

### 3.1 Kullanıcı Yönetimi
**Amaç**: Güvenli kullanıcı kaydı, girişi ve profil yönetimi

**Fonksiyonel Gereksinimler**:
- Kullanıcı kaydı (email, ad, soyad)
- Giriş/Çıkış işlemleri
- Şifre sıfırlama
- Profil güncelleme
- Kullanıcı adres yönetimi (birden fazla adres)
- Rol bazlı yetkilendirme (Admin, Customer)

**API Endpoint'leri**:
- `POST /api/users/register`
- `POST /api/users/login`
- `GET /api/users/profile`
- `PUT /api/users/profile`
- `GET /api/user-addresses`
- `POST /api/user-addresses`
- `PUT /api/user-addresses/{id}`
- `DELETE /api/user-addresses/{id}`

### 3.2 Ürün Kataloğu
**Amaç**: Ürünlerin organize edilmiş şekilde sunulması

**Fonksiyonel Gereksinimler**:
- Ürün listeleme (sayfalama ile)
- Ürün detay görüntüleme
- Kategori bazlı filtreleme
- Ürün arama
- Stok durumu kontrolü

**Domain Modeli**:
- **Product**: Name, Description, Price, CategoryId, Stock
- **Category**: Name, Products
- **ProductStock**: ProductId, Quantity, Reserve/Release operations

**API Endpoint'leri**:
- `GET /api/products`
- `GET /api/products/{id}`
- `GET /api/categories`
- `GET /api/categories/{id}/products`

### 3.3 Sepet ve Sipariş Yönetimi
**Amaç**: Kullanıcıların ürünleri sepete ekleyip sipariş verebilmesi

**Fonksiyonel Gereksinimler**:
- Sepete ürün ekleme/çıkarma
- Sepet içeriği görüntüleme
- Miktar güncelleme
- Sipariş oluşturma
- Sipariş geçmişi görüntüleme
- Sipariş durumu takibi
- Stok rezervasyonu (Domain Events ile)

**Domain Modeli**:
- **Order**: UserId, OrderDate, Status, TotalAmount, ShippingAddress, BillingAddress
- **OrderItem**: OrderId, ProductId, UnitPrice, Quantity, TotalPrice
- **OrderStatus**: Pending, Processing, Shipped, Delivered, Cancelled

**API Endpoint'leri**:
- `GET /api/orders`
- `GET /api/orders/{id}`
- `POST /api/orders`
- `POST /api/orders/{orderId}/items`
- `DELETE /api/orders/{orderId}/items/{productId}`
- `POST /api/orders/status/{orderId}`

### 3.4 Yetkilendirme ve Güvenlik
**Amaç**: Güvenli erişim kontrolü ve izin yönetimi

**Fonksiyonel Gereksinimler**:
- JWT token bazlı kimlik doğrulama
- Role-based authorization
- Permission-based authorization
- API endpoint'leri için güvenlik

**Permission Yapısı**:
```csharp
- Products: Create, Update, Delete, Manage
- Orders: View, Create, Update, Delete, Manage
- Categories: Create, Update, Delete, Manage
- Users: View, Create, Update, Delete, Manage
- Roles: View, Create, Update, Delete, Manage
```

### 3.5 Adres Yönetimi
**Amaç**: Kullanıcıların birden fazla adres kaydedebilmesi

**Fonksiyonel Gereksinimler**:
- Adres ekleme/güncelleme/silme
- Varsayılan adres belirleme
- Fatura ve kargo adresi ayrımı

**Value Objects**:
- **Address**: Street, City, State, ZipCode, Country
- **FullName**: FirstName, LastName

## 4. Non-Functional Gereksinimler

### 4.1 Performans
- **Önbellekleme**: Redis ile frequently accessed data
- **Sayfalama**: Büyük veri setleri için pagination
- **Async Operations**: Tüm I/O işlemleri asenkron
- **Database İndeksleme**: Kritik sorgular için optimum indeksler

### 4.2 Güvenlik
- **HTTPS**: Tüm iletişim şifreli
- **JWT Tokens**: Stateless authentication
- **Input Validation**: FluentValidation ile
- **SQL Injection**: Parameterized queries

### 4.3 Ölçeklenebilirlik
- **Microservice Ready**: Modüler yapı
- **Distributed Caching**: Redis
- **Container Support**: Docker
- **Database Migrations**: Otomatik şema güncellemeleri

### 4.4 Güvenilirlik
- **Global Exception Handling**: Merkezi hata yönetimi
- **Logging**: Structured logging with Serilog
- **Health Checks**: Sistem sağlık kontrolü
- **Transaction Management**: ACID özellikleri

## 5. API Spesifikasyonu

### 5.1 Authentication Endpoints
```
POST /connect/token - Access token alma
POST /connect/authorize - Yetkilendirme
GET /connect/userinfo - Kullanıcı bilgileri
POST /connect/logout - Çıkış
```

### 5.2 Core Business Endpoints

#### Ürün Yönetimi
```
GET /api/products - Ürün listesi (sayfalama ile)
GET /api/products/{id} - Ürün detayı
POST /api/products - Ürün oluşturma [Admin]
PUT /api/products/{id} - Ürün güncelleme [Admin]
DELETE /api/products/{id} - Ürün silme [Admin]
```

#### Kategori Yönetimi
```
GET /api/categories - Kategori listesi
GET /api/categories/{id} - Kategori detayı
POST /api/categories - Kategori oluşturma [Admin]
PUT /api/categories/{id} - Kategori güncelleme [Admin]
DELETE /api/categories/{id} - Kategori silme [Admin]
```

#### Sipariş Yönetimi
```
GET /api/orders - Sipariş listesi
GET /api/orders/{id} - Sipariş detayı
GET /api/orders/user/{userId} - Kullanıcıya ait siparişler
POST /api/orders - Sipariş oluşturma
POST /api/orders/{orderId}/items - Sepete ürün ekleme
DELETE /api/orders/{orderId}/items/{productId} - Sepetten ürün çıkarma
POST /api/orders/status/{orderId} - Sipariş durumu güncelleme [Admin]
```

## 6. Veri Modeli

### 6.1 Ana Entities
```csharp
// User (Identity)
public class User : IdentityUser<Guid>
{
    public FullName FullName { get; set; }
    public bool IsActive { get; set; }
    public ICollection<UserAddress> Addresses { get; set; }
}

// Product
public class Product : AuditableEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Price Price { get; set; }
    public Guid CategoryId { get; set; }
    public Category Category { get; set; }
    public ProductStock Stock { get; set; }
}

// Order
public class Order : AuditableEntity
{
    public Guid UserId { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public Address ShippingAddress { get; set; }
    public Address BillingAddress { get; set; }
    public ICollection<OrderItem> Items { get; set; }
}
```

### 6.2 Value Objects
```csharp
public record Price(decimal Value);
public record Address(string Street, string City, string State, string ZipCode, string Country);
public record FullName(string FirstName, string LastName);
```

## 7. Deployment ve DevOps

### 7.1 Container Yapısı
- **ecommerce.webapi**: Ana API servisi
- **ecommerce.authserver**: Kimlik doğrulama servisi
- **ecommerce.db**: PostgreSQL veritabanı
- **ecommerce.redis**: Redis cache
- **ecommerce.seq**: Log management
- **ecommerce.pgadmin**: Database yönetimi

### 7.2 Environment Configuration
```yaml
Development:
  - API: http://localhost:4000
  - Auth: https://localhost:5002
  - Database: PostgreSQL
  - Cache: Redis
  - Logging: Seq

Production:
  - HTTPS enforced
  - Environment variables
  - Secrets management
  - Health monitoring
```

## 8. Test Stratejisi

### 8.1 Test Türleri
- **Unit Tests**: Domain logic, business rules
- **Integration Tests**: Database operations, API endpoints
- **Behavior Tests**: User scenarios
- **Container Tests**: Testcontainers ile gerçek database

### 8.2 Test Coverage
- Domain Entities: %100
- Application Services: %90+
- API Controllers: %85+
- Repository Operations: %90+

## 9. Güvenlik Gereksinimleri

### 9.1 Authentication & Authorization
- OpenIddict ile OAuth 2.0 / OpenID Connect
- JWT tokens (Access + Refresh)
- Role-based ve Permission-based authorization
- Secure password policies

### 9.2 Data Protection
- HTTPS only communication
- Sensitive data encryption
- Input validation ve sanitization
- SQL injection prevention

## 10. İzleme ve Logging

### 10.1 Logging Strategy
- Structured logging (JSON format)
- Multiple sinks: Console, File, Seq
- Correlation IDs
- Performance metrics
- Error tracking

### 10.2 Monitoring
- Health checks
- Performance counters
- Database connection monitoring
- Cache hit/miss ratios

## 11. Gelecek Özellikler (Roadmap)

### Phase 2
- Ödeme entegrasyonu
- Kargo takip sistemi
- Ürün yorumları ve değerlendirmeleri
- Kampanya ve indirim sistemi

### Phase 3
- Mobil uygulama
- AI destekli ürün önerileri
- Multi-vendor marketplace
- Advanced analytics dashboard

## 12. Success Metrics

### 12.1 Technical Metrics
- API Response time < 200ms
- 99.9% uptime
- Cache hit ratio > 80%
- Database query optimization

### 12.2 Business Metrics
- User registration rate
- Order completion rate
- Average order value
- Customer satisfaction score

---

**Doküman Versiyonu**: 1.0  
**Son Güncelleme**: 2024  
**Hazırlayan**: AI Assistant  
**Durum**: Aktif Geliştirme 