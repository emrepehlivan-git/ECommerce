using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;

namespace ECommerce.Application.Helpers;

public class LocalizationHelper(ILocalizationService localizationService) : ILocalizationHelper, IScopedDependency
{
    public string this[string key] => localizationService.GetLocalizedString(key);
    public string this[string key, string language] => localizationService.GetLocalizedString(key, language);
}