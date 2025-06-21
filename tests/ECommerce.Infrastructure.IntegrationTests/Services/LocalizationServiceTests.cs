namespace ECommerce.Infrastructure.IntegrationTests.Services;

public class LocalizationServiceTests
{
    private readonly LocalizationService _localizationService = new();

    [Fact]
    public void GetLocalizedString_ShouldReturnEnglishText_WhenLanguageSpecified()
    {
        var result = _localizationService.GetLocalizedString("AuthMessages:LoginSuccess", "en");
        result.Should().Be("Login successful.");
    }

    [Fact]
    public void GetLocalizedString_ShouldFallbackToEnglish_WhenLanguageMissing()
    {
        var result = _localizationService.GetLocalizedString("AuthMessages:LoginSuccess", "de");
        result.Should().Be("Login successful.");
    }

    [Fact]
    public void GetLocalizedString_ShouldReturnKey_WhenKeyMissing()
    {
        var result = _localizationService.GetLocalizedString("Unknown:Key", "en");
        result.Should().Be("Unknown:Key");
    }
}
