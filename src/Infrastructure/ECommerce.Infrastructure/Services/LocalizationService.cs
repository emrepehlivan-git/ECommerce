using System.Text.Json;
using System.Globalization;
using ECommerce.Application.Common.Interfaces;
using ECommerce.SharedKernel;

namespace ECommerce.Infrastructure.Services;

public sealed class LocalizationService : ILocalizationService, ISingletonDependency
{
    private readonly Dictionary<string, Dictionary<string, string>> _localizedData = [];

    public LocalizationService()
    {
        var supportedLanguages = new List<string> { "en", "tr" };
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var localizationPath = Path.Combine(baseDirectory, "Localization");

        foreach (var lang in supportedLanguages)
        {
            var filePath = Path.Combine(localizationPath, $"{lang}.json");
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                _localizedData[lang] = data ?? [];
            }
            else
            {
                _localizedData[lang] = [];
            }
        }
    }

    public string GetLocalizedString(string key)
    {
        var currentCulture = CultureInfo.CurrentCulture.Name;
        return GetLocalizedStringInternal(key, currentCulture);
    }

    public string GetLocalizedString(string key, string language)
    {
        return GetLocalizedStringInternal(key, language);
    }

    private string GetLocalizedStringInternal(string key, string language)
    {
        var primaryLanguage = language.Split(',')[0].Split(';')[0].Split('-')[0].ToLower();

        if (!_localizedData.ContainsKey(primaryLanguage))
        {
            primaryLanguage = "en";
        }

        if (_localizedData.TryGetValue(primaryLanguage, out var translations))
        {
            if (translations.TryGetValue(key, out var value))
                return value;
        }
        return key;
    }
}
