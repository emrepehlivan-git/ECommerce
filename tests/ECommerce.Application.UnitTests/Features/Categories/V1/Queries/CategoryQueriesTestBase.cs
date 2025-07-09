using ECommerce.Application.Features.Categories.V1;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;

namespace ECommerce.Application.UnitTests.Features.Categories.V1.Queries;

public abstract class CategoryQueriesTestBase
{
    protected readonly Mock<ICategoryRepository> CategoryRepositoryMock;
    protected readonly Mock<ILazyServiceProvider> LazyServiceProviderMock;

    protected Mock<ILocalizationHelper> LocalizerMock;

    protected CategoryQueriesTestBase()
    {
        CategoryRepositoryMock = new Mock<ICategoryRepository>();
        LazyServiceProviderMock = new Mock<ILazyServiceProvider>();
        LocalizerMock = new Mock<ILocalizationHelper>();

        LazyServiceProviderMock
            .Setup(x => x.LazyGetRequiredService<ILocalizationHelper>())
            .Returns(LocalizerMock.Object);
    }

    protected void SetupDefaultLocalizationMessages()
    {
        LocalizerMock
            .Setup(x => x[CategoryConsts.NotFound])
            .Returns("Category not found");
    }

    protected void SetupCategoryRepositoryGetByIdAsync(Category? category = null)
    {
        CategoryRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Expression<Func<IQueryable<Category>, IQueryable<Category>>>?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
    }

    protected void SetupCategoryExists(bool exists = true)
    {
        CategoryRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }
}
