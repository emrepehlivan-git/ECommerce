# ECommerce-App

A modular and scalable e-commerce application built with modern .NET technologies. The system covers essential e-commerce features such as user management, product catalog, shopping cart, and order processing.

## Features

- **User Management**: Registration, login, and role-based authorization
- **Product Catalog**: Product listing and detail pages
- **Shopping Cart**: Add, remove, and update items in cart
- **Order Processing**: Place and view past orders
- **Authentication & Authorization**: Integrated OpenIddict with ASP.NET Identity
- **Admin and Customer Roles**: Secure access to features based on user roles

## Tech Stack

- **Framework**: ASP.NET Core 8.0
- **Language**: C# 12
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core 8
- **Architecture**: Onion Architecture
- **CQRS**: MediatR
- **Validation**: FluentValidation
- **Authentication**: OpenIddict + ASP.NET Identity (Token & Cookie based)
- **Logging**: Serilog with structured logging
- **Observability**: OpenTelemetry with Jaeger, Prometheus, Grafana
- **Testing**: xUnit, Testcontainers
- **Dependency Injection**: ILazyServiceProvider pattern
- **API**: RESTful services using Controllers
- **DevOps**: Docker & Docker Compose

## Architecture Overview

```
/src
 ├── Core
 │   ├── ECommerce.Domain                          → Entities, ValueObjects
 │   ├── ECommerce.Application                     → CQRS, DTOs, Validators, Services
 │   └── ECommerce.SharedKernel                    → Shared abstractions (Logging, Interfaces, Extensions)
 ├── Infrastructure
 │   ├── ECommerce.AuthServer                      → OpenIddict authorization server
 │   ├── ECommerce.Infrastructure                  → External services (email, integrations)
 │   └── ECommerce.Persistence                     → EF Core DbContext, configurations
 ├── Presentation
 │   └── ECommerce.WebAPI                          → RESTful API layer
 └── Tests
     ├── ECommerce.Application.UnitTests           → Application layer unit tests
     ├── ECommerce.Domain.UnitTests                → Domain layer unit tests
     ├── ECommerce.Infrastructure.IntegrationTests → Infrastructure integration tests
     └── ECommerce.WebAPI.IntegrationTests         → API integration tests
```

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- [Docker](https://www.docker.com/) (for optional containerization)
- PostgreSQL (if not using Docker)

### Installation

```bash
git clone https://github.com/emrepehlivan-git/ECommerce-App.git
cd ECommerce-App
```

### Quick Start with Observability

```bash
# Option 1: Use the quick start script
./start-observability.sh

# Option 2: Direct Docker Compose
docker-compose up -d
```

### Access URLs

**Application:**
- **API**: http://localhost:4000
- **Auth Server**: https://localhost:5002
- **Swagger Documentation**: http://localhost:4000/swagger
- **API Metrics**: http://localhost:4000/metrics

**Observability Stack:**
- **Jaeger (Distributed Tracing)**: http://localhost:16686
- **Prometheus (Metrics)**: http://localhost:9090
- **Grafana (Dashboards)**: http://localhost:3000 (admin/admin)
- **Seq (Structured Logs)**: http://localhost:5341

**Database & Tools:**
- **PostgreSQL**: localhost:5432 (postgres/postgres)
- **PgAdmin**: http://localhost:8082 (admin@example.com/admin)
- **Redis**: localhost:6379

### Local Development

```bash
# Apply Database Migrations
dotnet ef database update --project src/Infrastructure/ECommerce.Persistence

# Run Auth Server
dotnet run --project src/Infrastructure/ECommerce.AuthServer

# Run API
dotnet run --project src/Presentation/ECommerce.WebAPI
```

## Running Tests

```bash
dotnet test
```

## License

This project is licensed under the MIT License. See the [LICENSE](./LICENSE) file for details.
