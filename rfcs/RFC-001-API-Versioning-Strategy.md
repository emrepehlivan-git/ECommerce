# RFC-001: API Versioning Strategy

**Author**: Development Team  
**Status**: Draft  
**Created**: 2024-12-28  
**Updated**: 2024-12-28  

## Summary

This RFC proposes a comprehensive API versioning strategy for the ECommerce platform to ensure backward compatibility, smooth client transitions, and maintainable API evolution.

## Motivation

As the ECommerce platform grows, we need a clear versioning strategy to:
- Maintain backward compatibility for existing clients
- Enable gradual API evolution without breaking changes
- Provide clear migration paths for API consumers
- Support multiple API versions simultaneously
- Ensure consistent versioning across all endpoints

## Detailed Design

### 1. Versioning Scheme

**Semantic Versioning (SemVer)**: `v{major}.{minor}.{patch}`

- **Major**: Breaking changes that require client updates
- **Minor**: New features, backward compatible
- **Patch**: Bug fixes, backward compatible

**Examples**:
- `v1.0.0` - Initial release
- `v1.1.0` - Added new optional fields
- `v1.1.1` - Bug fix in order calculation
- `v2.0.0` - Breaking change in authentication

### 2. Versioning Methods

#### Primary Method: URL Path Versioning
```
GET /api/v1/products
GET /api/v2/products
POST /api/v1/orders
```

**Advantages**:
- Clear and visible
- Easy to cache
- REST compliant
- Simple routing

#### Alternative Method: Header Versioning (Fallback)
```
GET /api/products
Accept: application/json; version=1.0
```

### 3. Version Support Policy

#### Support Timeline
- **Current Version (v2.x)**: Full support, new features
- **Previous Version (v1.x)**: Maintenance only, critical bugs
- **Legacy Version (v0.x)**: Deprecated, 6-month sunset

#### Deprecation Process
1. **Announcement**: 6 months notice via API headers and documentation
2. **Warning Phase**: Return deprecation warnings in responses
3. **Sunset**: Remove deprecated version

### 4. Implementation Strategy

#### Controller Structure
```csharp
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsV1Controller : BaseApiController
{
    // V1 implementation
}

[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsV2Controller : BaseApiController
{
    // V2 implementation
}
```

#### Shared Business Logic
```csharp
// Shared service for both versions
public class ProductService : IProductService
{
    public async Task<ProductResult> GetProductAsync(Guid id, string version)
    {
        var product = await _repository.GetByIdAsync(id);
        
        return version switch
        {
            "1.0" => product.ToV1Dto(),
            "2.0" => product.ToV2Dto(),
            _ => throw new UnsupportedApiVersionException()
        };
    }
}
```

### 5. Response Headers

All API responses will include version information:

```http
HTTP/1.1 200 OK
API-Version: 1.0
API-Supported-Versions: 1.0, 1.1, 2.0
API-Deprecated-Versions: 0.9
Content-Type: application/json
```

### 6. Documentation Strategy

#### Swagger/OpenAPI Configuration
```csharp
services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-API-Version")
    );
});

services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
```

#### Documentation Per Version
- Separate Swagger documents for each major version
- Migration guides between versions
- Deprecation notices and timelines

### 7. Breaking Change Examples

#### V1 → V2 Breaking Changes
```json
// V1 Response
{
  "id": "123",
  "name": "Product Name",
  "price": 29.99,
  "category": "Electronics"
}

// V2 Response (Breaking: price structure changed)
{
  "id": "123",
  "name": "Product Name",
  "pricing": {
    "amount": 29.99,
    "currency": "USD",
    "taxIncluded": true
  },
  "category": {
    "id": "cat-1",
    "name": "Electronics"
  }
}
```

### 8. Client SDK Support

#### NuGet Package Versioning
```
ECommerce.Client.V1 - v1.x.x
ECommerce.Client.V2 - v2.x.x
```

#### Client Configuration
```csharp
var client = new ECommerceClient(new ECommerceClientOptions 
{
    BaseUrl = "https://api.ecommerce.com",
    ApiVersion = "2.0",
    Timeout = TimeSpan.FromSeconds(30)
});
```

### 9. Testing Strategy

#### Version-Specific Tests
```csharp
[Fact]
public async Task GetProduct_V1_ReturnsCorrectFormat()
{
    var response = await _client.GetAsync("/api/v1/products/123");
    // Assert V1 format
}

[Fact]
public async Task GetProduct_V2_ReturnsCorrectFormat()
{
    var response = await _client.GetAsync("/api/v2/products/123");
    // Assert V2 format
}
```

#### Compatibility Tests
```csharp
[Theory]
[InlineData("1.0")]
[InlineData("1.1")]
[InlineData("2.0")]
public async Task GetProduct_AllVersions_ShouldWork(string version)
{
    var response = await _client.GetAsync($"/api/v{version}/products/123");
    response.Should().BeSuccessful();
}
```

### 10. Migration Path Example

#### V1 to V2 Migration Guide
```markdown
## Breaking Changes in V2

### Price Field Structure
**V1**: `"price": 29.99`
**V2**: `"pricing": { "amount": 29.99, "currency": "USD" }`

**Migration**: Update client code to access `pricing.amount` instead of `price`

### Category Field Structure  
**V1**: `"category": "Electronics"`
**V2**: `"category": { "id": "cat-1", "name": "Electronics" }`

**Migration**: Update client code to access `category.name` instead of `category`
```

## Implementation Plan

### Phase 1: Infrastructure (Sprint 1)
- [ ] Add Microsoft.AspNetCore.Mvc.Versioning
- [ ] Configure API versioning middleware
- [ ] Update Swagger configuration
- [ ] Add version response headers

### Phase 2: V1 Stabilization (Sprint 2)
- [ ] Refactor existing controllers to V1
- [ ] Add version-specific DTOs
- [ ] Update tests for V1
- [ ] Document V1 API

### Phase 3: V2 Development (Sprint 3-4)
- [ ] Implement V2 controllers
- [ ] Create V2 DTOs with breaking changes
- [ ] Add V2 tests
- [ ] Create migration documentation

### Phase 4: Client Support (Sprint 5)
- [ ] Update client SDKs
- [ ] Create version-specific packages
- [ ] Update integration examples

## Alternatives Considered

### Query Parameter Versioning
```
GET /api/products?version=1.0
```
**Rejected**: Harder to cache, not RESTful

### Accept Header Versioning
```
Accept: application/vnd.ecommerce.v1+json
```
**Rejected**: More complex, harder for developers to test

### Subdomain Versioning
```
v1.api.ecommerce.com
v2.api.ecommerce.com
```
**Rejected**: Infrastructure complexity, SSL certificate management

## Risks and Mitigation

### Risk: Version Proliferation
**Mitigation**: Clear deprecation policy, maximum 3 supported versions

### Risk: Maintenance Overhead
**Mitigation**: Shared business logic, automated testing across versions

### Risk: Client Confusion
**Mitigation**: Clear documentation, migration guides, deprecation warnings

## Success Metrics

- Zero breaking changes within major versions
- 95%+ client adoption of new versions within 6 months
- Reduced support tickets related to API changes
- Automated testing coverage >90% across all supported versions

## Future Considerations

- GraphQL versioning strategy for complex queries
- Microservice versioning coordination
- Event versioning for event-driven architecture
- API gateway integration for version routing

---

**Next Steps**:
1. Team review and feedback
2. Technical spike for versioning infrastructure
3. Update development guidelines
4. Implementation planning 