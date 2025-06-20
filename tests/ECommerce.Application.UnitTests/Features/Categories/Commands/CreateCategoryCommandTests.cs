using ECommerce.Application.Features.Categories;
using ECommerce.Application.Features.Categories.Commands;

namespace ECommerce.Application.UnitTests.Features.Categories.Commands;

public sealed class CreateCategoryCommandTests : CategoryCommandsTestBase
{
    private readonly CreateCategoryCommandHandler Handler;
    private readonly CreateCategoryCommand Command;
    private readonly CreateCategoryCommandValidator Validator;

    public CreateCategoryCommandTests()
    {
        Command = new CreateCategoryCommand("Test Category");

        Handler = new CreateCategoryCommandHandler(
            CategoryRepositoryMock.Object,
            CacheManagerMock.Object,
            LazyServiceProviderMock.Object);

        Validator = new CreateCategoryCommandValidator(
            new CategoryBusinessRules(CategoryRepositoryMock.Object),
            Localizer);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateCategory()
    {
        // Arrange
        SetupCategoryRepositoryAdd(DefaultCategory);
        SetupCategoryExists(false);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(DefaultCategory.Id);
    }

    [Theory]
    [InlineData("", "Category name is required", "Category name must be at least 3 characters long")]
    [InlineData("AB", "Category name must be at least 3 characters long")]
    public async Task Validate_WithInvalidName_ShouldReturnValidationError(string name, string expectedError, string? secondExpectedError = null)
    {
        // Arrange
        var command = Command with { Name = name };
        SetupCategoryExists(false);

        // Act
        var validationResult = await Validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();

        if (secondExpectedError != null)
        {
            validationResult.Errors.Should().HaveCount(2);
            validationResult.Errors.Should().Contain(x => x.ErrorMessage == expectedError);
            validationResult.Errors.Should().Contain(x => x.ErrorMessage == secondExpectedError);
        }
        else
        {
            validationResult.Errors.Should().ContainSingle()
                .Which.ErrorMessage.Should().Be(expectedError);
        }
    }

    [Fact]
    public async Task Validate_WithExistingName_ShouldReturnValidationError()
    {
        // Arrange
        SetupCategoryExists(true);

        // Act
        var validationResult = await Validator.ValidateAsync(Command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Category with this name already exists");
    }
}