using System.Text.Json;
using System.Globalization;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;

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
                using var doc = JsonDocument.Parse(json);
                var data = new Dictionary<string, string>();
                FlattenJson(doc.RootElement, string.Empty, data);
                _localizedData[lang] = data;
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

    private static void FlattenJson(JsonElement element, string prefix, Dictionary<string, string> result)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                var newPrefix = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}:{property.Name}";
                FlattenJson(property.Value, newPrefix, result);
            }
        }
        else if (element.ValueKind == JsonValueKind.String)
        {
            result[prefix] = element.GetString() ?? string.Empty;
        }
    }
}
