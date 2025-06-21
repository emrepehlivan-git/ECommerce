namespace ECommerce.Application.Services;

public interface ILocalizationService
{
    string GetLocalizedString(string key, string language);
    string GetLocalizedString(string key);
}