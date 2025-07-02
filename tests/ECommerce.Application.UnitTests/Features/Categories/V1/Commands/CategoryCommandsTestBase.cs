using ECommerce.Application.Features.Categories.V1;
using ECommerce.Application.Helpers;
using ECommerce.Application.Services;

namespace ECommerce.Application.UnitTests.Features.Categories.V1.Commands;

public abstract class CategoryCommandsTestBase
{
    protected Category DefaultCategory => Category.Create("Test Category");

    protected Mock<ICategoryRepository> CategoryRepositoryMock;
    protected Mock<ILazyServiceProvider> LazyServiceProviderMock;
    protected Mock<ILocalizationService> LocalizationServiceMock;
    protected Mock<ICacheManager> CacheManagerMock;

    protected LocalizationHelper Localizer;

    protected CategoryCommandsTestBase()
    {
        CategoryRepositoryMock = new Mock<ICategoryRepository>();
        LazyServiceProviderMock = new Mock<ILazyServiceProvider>();
        LocalizationServiceMock = new Mock<ILocalizationService>();
        CacheManagerMock = new Mock<ICacheManager>();
        Localizer = new LocalizationHelper(LocalizationServiceMock.Object);

        LazyServiceProviderMock
            .Setup(x => x.LazyGetRequiredService<LocalizationHelper>())
            .Returns(Localizer);

        SetupDefaultLocalizationMessages();
    }

    protected void SetupDefaultLocalizationMessages()
    {
        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(CategoryConsts.NameMustBeAtLeastCharacters))
            .Returns("Category name must be at least 3 characters long");
        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(CategoryConsts.NameMustBeLessThanCharacters))
            .Returns("Category name must be less than 100 characters long");
        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(CategoryConsts.NameExists))
            .Returns("Category with this name already exists");
        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(CategoryConsts.NotFound))
            .Returns("Category not found");
        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(CategoryConsts.NameIsRequired))
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
    }

    protected void SetupCategoryRepositoryGetByIdAsync(Category? category = null)
    {
        CategoryRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Expression<Func<IQueryable<Category>, IQueryable<Category>>>?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
    }

    protected void SetupLocalizedMessage(string message)
    {
        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(message))
            .Returns(message);
    }
}