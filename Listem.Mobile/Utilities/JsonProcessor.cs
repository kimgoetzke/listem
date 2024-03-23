using System.Text.Json;

namespace Listem.Mobile.Utilities;

public static class JsonProcessor
{
    private static JsonSerializerOptions JsonOptions =>
        new() { PropertyNameCaseInsensitive = true, };

    public static async Task<T> ThrowIfNull<T>(Func<Task<T?>> request)
    {
        try
        {
            var result = await request.Invoke();

            if (result is null)
                throw new InvalidOperationException("Failed to deserialise JSON: result is null");

            return result;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to deserialise JSON: {ex.Message}");
        }
    }

    public static async Task<T?> FromFile<T>(string fileName)
        where T : class
    {
        await using var fs = await FileSystem.Current.OpenAppPackageFileAsync(fileName);
        using StreamReader reader = new(fs);
        var fileContent = await reader.ReadToEndAsync();

        return JsonSerializer.Deserialize<T>(fileContent, JsonOptions);
    }

    public static T? FromString<T>(string json)
        where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static async Task<T?> FromSecureStorage<T>(string key)
        where T : class
    {
        return await SecureStorage.Default.GetAsync(Constants.User) is not { } str
            ? null
            : FromString<T>(str);
    }
}
