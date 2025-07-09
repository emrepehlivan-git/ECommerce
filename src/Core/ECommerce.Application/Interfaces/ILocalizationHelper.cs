namespace ECommerce.Application.Interfaces;

public interface ILocalizationHelper
{
    string this[string key] { get; }
    string this[string key, string language] { get; }
} 