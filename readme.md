# 🛒 E-Commerce Application

A comprehensive and scalable e-commerce application built with modern .NET technologies. This project includes all essential features of an enterprise-level e-commerce platform and modern software development practices.

## 🌟 Features

### 🔐 Authentication and Authorization

- **OpenIddict** based OAuth 2.0 / OpenID Connect implementation
- **ASP.NET Identity** for user management
- **JWT Token** based authentication
- **Role-based authorization** (RBAC)
- **Permission-based access control** (Granular permissions)
- **Multiple authentication flows** support (Authorization Code, Client Credentials)

### 👥 User Management

- User registration and login system
- **Multiple address management** (default address support)
- **Profile management** and updates
- **Role-based** user classification

### 📦 Product Management

- **Category-based** product organization
- **Stock tracking** and reservation system
- **Price management** (Money pattern)
- **Product status** control (active/inactive)
- **Advanced search** and filtering

### 🛒 Cart and Order System

- **Real-time cart** management
- **Business rules** with cart constraints (max items, quantity, amount)
- **Domain Events** for stock reservation
- **Order status** tracking
- **Address management** (shipping and billing addresses)

### 🏗️ Architecture and Design

- **Onion Architecture** (Clean Architecture)
- **Domain Driven Design** (DDD) principles
- **CQRS** pattern with MediatR
- **Repository** and **Unit of Work** patterns
- **Specification** pattern for complex queries
- **Domain Events** handling

### 📊 Observability and Monitoring

- **OpenTelemetry** with distributed tracing
- **Jaeger** integration for trace visualization
- **Serilog** with structured logging
- **Seq** log aggregation platform
- **Custom metrics** (business and technical)
- **Health checks** and monitoring endpoints

### 💾 Data Management

- **PostgreSQL** database
- **Entity Framework Core** ORM
- **Fluent API** configurations
- **Database migrations** management
- **Connection pooling** and optimization

### ⚡ Performance and Caching

- **Hybrid Caching** (Memory + Redis)
- **L1 Cache**: In-memory for fast access
- **L2 Cache**: Redis for distributed scenarios
- **Pattern-based cache invalidation**
- **Cache-aside** pattern implementation

### 🌍 Internationalization

- **Multi-language support** (TR/EN)
- **JSON-based localization** files
- **Culture-aware** formatting
- **Centralized localization** management

### 🧪 Testing Strategy

- **Unit Tests** (xUnit, Moq, FluentAssertions)
- **Integration Tests** with Testcontainers
- **Repository pattern** testing
- **API endpoint** testing
- **Test coverage** optimization

### 🔒 Security

- **HTTPS** enforcement
- **CORS** policy management
- **Input validation** (FluentValidation)
- **SQL injection** protection
- **Authentication** and **authorization** middleware
- **Security headers** implementation

### 📈 DevOps and Deployment

- **Docker** containerization
- **Docker Compose** for multi-service orchestration
- **Development** and **production** configurations
- **Volume management** for data persistence
- **Health monitoring** setup

## 🏛️ Architecture Details

### Layer Structure

```
📁 src/
├── 🎯 Core/
│   ├── ECommerce.Domain          → Entities, Value Objects, Domain Events
│   ├── ECommerce.Application     → CQRS, DTOs, Validators, Business Logic
│   └── ECommerce.SharedKernel    → Shared abstractions, Base classes
├── 🔧 Infrastructure/
│   ├── ECommerce.AuthServer      → OpenIddict authorization server
│   ├── ECommerce.Infrastructure  → External services, Email, Caching
│   └── ECommerce.Persistence     → EF Core, Repositories, Database
└── 🌐 Presentation/
    └── ECommerce.WebAPI          → REST API controllers, Middleware
```

### 🔄 CQRS and MediatR Pipeline

```csharp
Request → ValidationBehavior → CacheBehavior → TracingBehavior → TransactionalBehavior → Handler
```

**Pipeline Behaviors:**

- **Validation**: Automatic validation with FluentValidation
- **Caching**: Automatic cache with ICacheableRequest interface
- **Tracing**: Request tracking with OpenTelemetry
- **Transaction**: DB transaction management with ITransactionalRequest

### 🛡️ Domain Events System

```csharp
// Domain Events Examples
- StockReservedEvent      → Stock reservation
- StockNotReservedEvent   → Stock release
- CartItemAddedEvent      → Cart item addition
- OrderPlacedEvent        → Order creation
```

### 📊 Business Rules

```csharp
// Cart Business Rules
- Max 50 items per cart
- Max 99 quantity per item
- Max $50,000 total amount
- Product availability check
- Stock sufficiency validation

// Order Business Rules
- Address validation
- Stock reservation
- Payment validation
- Status transition rules
```

## 🛠️ Technology Stack

### 🎯 Core Technologies

- **.NET 8.0** - Latest framework
- **C# 12** - Modern language features
- **ASP.NET Core** - Web framework

### 🗄️ Database & ORM

- **PostgreSQL** - Primary database
- **Entity Framework Core 8** - ORM
- **Npgsql** - PostgreSQL provider

### 🔑 Authentication & Authorization

- **OpenIddict 6.0** - OAuth/OIDC server
- **ASP.NET Identity** - User management
- **JWT Bearer** tokens

### 📨 Communication

- **MediatR** - CQRS implementation
- **FluentValidation** - Input validation
- **Mapster** - Object mapping

### 💾 Caching & Performance

- **Redis** - Distributed caching
- **Memory Cache** - L1 caching
- **HybridCacheManager** - Custom implementation

### 📊 Observability

- **OpenTelemetry** - Distributed tracing
- **Jaeger** - Trace visualization
- **Serilog** - Structured logging
- **Seq** - Log aggregation

### 🧪 Testing

- **xUnit** - Test framework
- **Moq** - Mocking framework
- **FluentAssertions** - Assertion library
- **Testcontainers** - Integration testing

### 🐳 DevOps

- **Docker** - Containerization
- **Docker Compose** - Multi-service orchestration

## 🚀 Getting Started

### 📋 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Git](https://git-scm.com/)

### 💻 Quick Setup

```bash
# Clone the repository
git clone https://github.com/emrepehlivan-git/ECommerce.git
cd ECommerce

# Start all services with Docker
docker-compose up -d

# Alternative: Start only infrastructure services
docker-compose up -d ecommerce.db ecommerce.redis ecommerce.seq ecommerce.jaeger
```

### 🔧 Manual Setup (Development)

```bash
# Apply database migrations
dotnet ef database update --project src/Infrastructure/ECommerce.Persistence

# Start Auth Server
dotnet run --project src/Infrastructure/ECommerce.AuthServer

# Start API (new terminal)
dotnet run --project src/Presentation/ECommerce.WebAPI
```

## 🌐 Access URLs

### 📱 Application Services

| Service         | URL                           | Description         |
| --------------- | ----------------------------- | ------------------- |
| **API**         | http://localhost:4000         | REST API endpoints  |
| **Auth Server** | https://localhost:5002        | OAuth/OIDC provider |
| **Swagger UI**  | http://localhost:4000/swagger | API documentation   |

### 📊 Monitoring and Logs

| Tool        | URL                    | Username          | Password |
| ----------- | ---------------------- | ----------------- | -------- |
| **Jaeger**  | http://localhost:16686 | -                 | -        |
| **Seq**     | http://localhost:5341  | -                 | -        |
| **PgAdmin** | http://localhost:8082  | admin@example.com | admin    |

### 🗄️ Database

| Service        | Host      | Port | Database  | Username | Password |
| -------------- | --------- | ---- | --------- | -------- | -------- |
| **PostgreSQL** | localhost | 5432 | ecommerce | postgres | postgres |
| **Redis**      | localhost | 6379 | -         | -        | -        |

## 📚 API Usage

### 🔐 Authentication Flow

```bash
# 1. Authorization Code Flow (via Swagger)
# Swagger UI → Authorize → Login with credentials

# 2. Client Credentials Flow
curl -X POST https://localhost:5002/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials&client_id=api&client_secret=api-secret&scope=api"
```

### 📦 Basic API Examples

```bash
# List categories
GET /api/Category

# List products (with pagination)
GET /api/Product?PageNumber=1&PageSize=10

# Add item to cart
POST /api/Cart
{
  "productId": "guid",
  "quantity": 2
}

# Create order
POST /api/Order
{
  "userId": "guid",
  "items": [
    {
      "productId": "guid",
      "quantity": 1
    }
  ],
  "shippingAddress": { ... },
  "billingAddress": { ... }
}
```

## 🧪 Running Tests

```bash
# Run all tests
dotnet test

# Specific test project
dotnet test tests/ECommerce.Application.UnitTests/
dotnet test tests/ECommerce.Infrastructure.IntegrationTests/
dotnet test tests/ECommerce.WebAPI.IntegrationTests/

# With coverage report
dotnet test --collect:"XPlat Code Coverage"
```

## 📊 Monitoring and Observability

### 🔍 Distributed Tracing

- **Jaeger UI**: http://localhost:16686
- **Service Map**: Visualize inter-service dependencies
- **Request Tracing**: End-to-end request tracking
- **Performance Metrics**: Response time, error rate

### 📝 Structured Logging

- **Seq Dashboard**: http://localhost:5341
- **Log Levels**: Debug, Info, Warning, Error, Fatal
- **Correlation IDs**: Request tracking across services
- **Structured Data**: JSON formatted logs

### 📈 Custom Metrics

```csharp
// Business Metrics
- orders_total: Total number of orders
- order_value: Order value distribution
- product_views_total: Product views
- user_registrations_total: User registrations

// Technical Metrics
- database_query_duration: DB query times
- cache_hits_total: Cache hit rates
- api_requests_total: API request metrics
```

## 🔧 Configuration

### 🌍 Environment Variables

```bash
# Database
ConnectionStrings__DefaultConnection="Host=localhost;Database=ecommerce;Username=postgres;Password=postgres"
ConnectionStrings__Redis="localhost:6379"

# Authentication
Authentication__Authority="https://localhost:5002"
Authentication__Audience="api"

# Observability
OpenTelemetry__ServiceName="ECommerce.WebAPI"
OpenTelemetry__Jaeger__AgentHost="localhost"
OpenTelemetry__OTLP__Endpoint="http://localhost:4317"

# Logging
LoggingOptions__SeqUrl="http://localhost:5341"
```

### ⚙️ Configuration Files

- `appsettings.json` - Production settings
- `appsettings.Development.json` - Development overrides
- `compose.yaml` - Development environment
- `compose.production.yml` - Production environment

## 🚀 Production Deployment

### 🐳 Docker Production Build

```bash
# Run in production environment
docker-compose -f compose.production.yml up -d

# Monitor logs
docker-compose logs -f ecommerce.webapi
docker-compose logs -f ecommerce.authserver
```

### 🔒 Security Considerations

- **HTTPS**: SSL certificates must be configured
- **Secrets**: Use environment variables
- **CORS**: Add production domains
- **Rate Limiting**: Set limits for API endpoints
- **WAF**: Web Application Firewall recommended

## 🤝 Contributing

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Commit** your changes (`git commit -m 'Add amazing feature'`)
4. **Push** to the branch (`git push origin feature/amazing-feature`)
5. **Open** a Pull Request

### 📋 Development Guidelines

- Follow **Clean Code** principles
- Write **unit tests**
- Add **XML documentation**
- Use **Conventional Commits**
- Participate in **code review** process

## 📄 License

This project is licensed under the [MIT License](./LICENSE).

## 🆘 Support and Contact

- **Issues**: [GitHub Issues](https://github.com/emrepehlivan-git/ECommerce/issues)
- **Discussions**: [GitHub Discussions](https://github.com/emrepehlivan-git/ECommerce/discussions)
- **Email**: [Developer](mailto:emrepehlivan.dev@gmail.com)

## 🔗 Useful Links

- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [OpenIddict Documentation](https://documentation.openiddict.com/)
- [Docker Documentation](https://docs.docker.com/)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)

---

⭐ **If you like this project, please give it a star!**
