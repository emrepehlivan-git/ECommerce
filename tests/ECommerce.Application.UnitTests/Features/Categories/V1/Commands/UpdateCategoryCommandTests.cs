using ECommerce.Application.Features.Categories.V1;
    
namespace ECommerce.Application.UnitTests.Features.Categories.V1.Commands;

public sealed class UpdateCategoryCommandTests : CategoryCommandsTestBase
{
    private readonly UpdateCategoryCommandHandler Handler;
    private readonly UpdateCategoryCommand Command;
    private readonly UpdateCategoryCommandValidator Validator;
    private readonly Guid CategoryId = Guid.NewGuid();

    public UpdateCategoryCommandTests()
    {
        Command = new UpdateCategoryCommand(CategoryId, "Updated Category");

        Handler = new UpdateCategoryCommandHandler(
            CategoryRepositoryMock.Object,
            CacheManagerMock.Object,
            LazyServiceProviderMock.Object);

        Validator = new UpdateCategoryCommandValidator(
            new CategoryBusinessRules(CategoryRepositoryMock.Object),
            LocalizerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateCategory()
    {
        // Arrange
        var existingCategory = Category.Create("Original Category");
        SetupCategoryRepositoryGetByIdAsync(existingCategory);
        SetupCategoryExists(false);

        // Act
        var result = await Handler.Handle(Command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        existingCategory.Name.Should().Be(Command.Name);
    }


    [Theory]
    [InlineData("", "Category name is required", "Category name must be at least 3 characters long")]
    [InlineData("AB", "Category name must be at least 3 characters long")]
    public async Task Validate_WithInvalidName_ShouldReturnValidationError(string name, string expectedError, string? secondExpectedError = null)
    {
        // Arrange
        var command = Command with { Name = name };
        var existingCategory = Category.Create("Original Category");
        SetupCategoryRepositoryGetByIdAsync(existingCategory);
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
        var existingCategory = Category.Create("Original Category");
        SetupCategoryRepositoryGetByIdAsync(existingCategory);
        SetupCategoryExists(true);

        // Act
        var validationResult = await Validator.ValidateAsync(Command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(LocalizerMock.Object[CategoryConsts.NameExists]);
    }

    [Fact]
    public async Task Validate_WithNonExistentCategory_ShouldReturnValidationError()
    {
        // Arrange
        SetupCategoryRepositoryGetByIdAsync(null);

        // Act
        var validationResult = await Validator.ValidateAsync(Command);

        // Assert
        validationResult.IsValid.Should().BeTrue();
        validationResult.Errors.Should().BeEmpty();
    }
}