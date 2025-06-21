using ECommerce.Application.Features.Categories;
using ECommerce.Application.Helpers;
using ECommerce.Application.Services;

namespace ECommerce.Application.UnitTests.Features.Categories.Queries;

public abstract class CategoryQueriesTestBase
{
    protected readonly Mock<ICategoryRepository> CategoryRepositoryMock;
    protected readonly Mock<ILazyServiceProvider> LazyServiceProviderMock;
    protected readonly Mock<ILocalizationService> LocalizationServiceMock;

    protected LocalizationHelper Localizer;

    protected CategoryQueriesTestBase()
    {
        CategoryRepositoryMock = new Mock<ICategoryRepository>();
        LazyServiceProviderMock = new Mock<ILazyServiceProvider>();
        LocalizationServiceMock = new Mock<ILocalizationService>();

        Localizer = new LocalizationHelper(LocalizationServiceMock.Object);

        LazyServiceProviderMock
            .Setup(x => x.LazyGetRequiredService<LocalizationHelper>())
            .Returns(Localizer);
    }

    protected void SetupDefaultLocalizationMessages()
    {
        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(CategoryConsts.NotFound))
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
