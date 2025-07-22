using ECommerce.Application.Features.Categories.V1;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.Specifications;

namespace ECommerce.Application.UnitTests.Features.Categories.V1.Commands;

public abstract class CategoryCommandsTestBase
{
    protected Category DefaultCategory => Category.Create("Test Category");

    protected Mock<ICategoryRepository> CategoryRepositoryMock;
    protected Mock<ILazyServiceProvider> LazyServiceProviderMock;
    protected Mock<ICacheManager> CacheManagerMock;

    protected Mock<ILocalizationHelper> LocalizerMock;
    protected CategoryCommandsTestBase()
    {
        CategoryRepositoryMock = new Mock<ICategoryRepository>();
        LazyServiceProviderMock = new Mock<ILazyServiceProvider>();
        CacheManagerMock = new Mock<ICacheManager>();
        LocalizerMock = new Mock<ILocalizationHelper>();

        LazyServiceProviderMock
            .Setup(x => x.LazyGetRequiredService<ILocalizationHelper>())
            .Returns(LocalizerMock.Object);

        SetupDefaultLocalizationMessages();
    }

    protected void SetupDefaultLocalizationMessages()
    {
        LocalizerMock
            .Setup(x => x[CategoryConsts.NameMustBeAtLeastCharacters])
            .Returns("Category name must be at least 3 characters long");
        LocalizerMock
            .Setup(x => x[CategoryConsts.NameMustBeLessThanCharacters])
            .Returns("Category name must be less than 100 characters long");
        LocalizerMock
            .Setup(x => x[CategoryConsts.NameExists])
            .Returns("Category with this name already exists");
        LocalizerMock
            .Setup(x => x[CategoryConsts.NotFound])
            .Returns("Category not found");
        LocalizerMock
            .Setup(x => x[CategoryConsts.NameIsRequired])
            .Returns("Category name is required");
    }

    protected void SetupCategoryRepositoryAdd(Category capturedCategory)
    {
        CategoryRepositoryMock
            .Setup(x => x.Add(It.IsAny<Category>()))
            .Callback<Category>(category => capturedCategory = category);
    }

    protected void SetupCategoryExists(bool exists = true)
    {
        CategoryRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);

        // Also setup the specification-based method
        CategoryRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<ISpecification<Category>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }

    protected void SetupCategoryRepositoryGetByIdAsync(Category? category = null)
    {
        CategoryRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Expression<Func<IQueryable<Category>, IQueryable<Category>>>?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
    }

    protected void SetupLocalizedMessage(string message)
    {
        LocalizerMock
            .Setup(x => x[message])
            .Returns(message);
    }
}