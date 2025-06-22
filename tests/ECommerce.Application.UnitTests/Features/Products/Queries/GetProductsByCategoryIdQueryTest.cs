using ECommerce.Application.Features.Products.Queries;
using ECommerce.Application.Features.Products.DTOs;
using Mapster;
using FluentValidation.TestHelper;

namespace ECommerce.Application.UnitTests.Features.Products.Queries;

public sealed class GetProductsByCategoryIdQueryTest : ProductQueriesTestsBase
{
    private readonly GetProductsByCategoryIdQueryHandler Handler;
    private readonly GetProductsByCategoryIdQueryValidator Validator;
    private readonly GetProductsByCategoryIdQuery Query;

    public GetProductsByCategoryIdQueryTest()
    {
        Query = new GetProductsByCategoryIdQuery(DefaultCategory.Id);

        Handler = new GetProductsByCategoryIdQueryHandler(
            ProductRepositoryMock.Object,
            LazyServiceProviderMock.Object);

        Validator = new GetProductsByCategoryIdQueryValidator(
            CategoryRepositoryMock.Object,
            Localizer);
    }

    [Fact]
    public async Task Handle_WithValidCategoryId_ShouldReturnPagedProducts()
    {
        // Arrange
        SetupCategoryExists(true);
        
        var products = new List<Product> { DefaultProduct };
        var pagedResult = new PagedResult<List<ProductDto>>(
            new PagedInfo(1, 10, 1, 1),
            products.Adapt<List<ProductDto>>()
        );

        ProductRepositoryMock
            .Setup(x => x.GetPagedAsync<ProductDto>(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<Expression<Func<IQueryable<Product>, IOrderedQueryable<Product>>>>(),
                It.IsAny<Expression<Func<IQueryable<Product>, IQueryable<Product>>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value.First().Name.Should().Be(DefaultProduct.Name);
    }

    [Fact]
    public async Task Validator_WithNonExistentCategoryId_ShouldHaveValidationError()
    {
        // Arrange
        SetupCategoryExists(false);
        var invalidQuery = new GetProductsByCategoryIdQuery(Guid.NewGuid());

        // Act
        var result = await Validator.TestValidateAsync(invalidQuery);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }

    [Fact]
    public async Task Validator_WithValidCategoryId_ShouldNotHaveValidationError()
    {
        // Arrange
        SetupCategoryExists(true);

        // Act
        var result = await Validator.TestValidateAsync(Query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CategoryId);
    }

    [Fact]
    public async Task Handle_WithEmptyProducts_ShouldReturnEmptyPagedResult()
    {
        // Arrange
        SetupCategoryExists(true);
        
        var emptyPagedResult = new PagedResult<List<ProductDto>>(
            new PagedInfo(1, 10, 0, 0),
            new List<ProductDto>()
        );

        ProductRepositoryMock
            .Setup(x => x.GetPagedAsync<ProductDto>(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<Expression<Func<IQueryable<Product>, IOrderedQueryable<Product>>>>(),
                It.IsAny<Expression<Func<IQueryable<Product>, IQueryable<Product>>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyPagedResult);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }
} 