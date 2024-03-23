using System.Net.Http.Json;
using Listem.Mobile.Models;
using Listem.Mobile.Utilities;
using Listem.Shared.Contracts;
using static Listem.Mobile.Utilities.HttpUtilities;

namespace Listem.Mobile.Services;

public class ApiCategoryService : IApiCategoryService
{
    private ObservableCategory? _defaultCategory;
    private readonly HttpClient _httpClient;

    public ApiCategoryService(IServiceProvider serviceProvider)
    {
        var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>()!;
        var authService = serviceProvider.GetService<AuthService>()!;
        _httpClient = httpClientFactory.CreateClient(Constants.HttpClientName);
        _httpClient.SetDefaultHeaders(authService.CurrentUser.AccessToken);
    }

    public async Task<ObservableCategory> GetDefaultCategory(string listId)
    {
        if (_defaultCategory == null)
        {
            var uri = $"/api/lists/{listId}/categories";
            var response = await LoggedRequest(() => _httpClient.GetAsync(uri));
            var categories = await response.Content.ReadFromJsonAsync<List<CategoryResponse>>();
            if (categories != null)
            {
                var defaultCategory = categories.First(c =>
                    c.Name == Shared.Constants.DefaultCategoryName
                );
                _defaultCategory = ObservableCategory.From(defaultCategory);
            }
        }

        if (_defaultCategory == null)
            throw new NullReferenceException("This list does not have default category");

        return _defaultCategory;
    }

    public async Task<List<ObservableCategory>> GetAllByListIdAsync(string listId)
    {
        var response = await LoggedRequest(
            () => _httpClient.GetAsync($"/api/lists/{listId}/categories")
        );
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryResponse>>();
        return ConvertToObservableCategories(categories);
    }

    private static List<ObservableCategory> ConvertToObservableCategories(
        List<CategoryResponse>? categories
    )
    {
        return categories?.Select(ObservableCategory.From).ToList() ?? [];
    }

    public async Task CreateOrUpdateAsync(ObservableCategory observableCat)
    {
        HttpResponseMessage? response;
        var content = Content(observableCat.ToCategoryRequest());
        if (observableCat.Id == null)
        {
            var postUri = $"/api/lists/{observableCat.ListId}/categories";
            response = await LoggedRequest(() => _httpClient.PostAsync(postUri, content));
        }
        else
        {
            var putUri = $"/api/lists/{observableCat.ListId}/categories/{observableCat.Id}";
            response = await LoggedRequest(() => _httpClient.PutAsync(putUri, content));
        }

        if (!response.IsSuccessStatusCode)
        {
            await ParseErrorResponse(response);
        }

        var categoryResponse = await response.Content.ReadFromJsonAsync<CategoryResponse>();
        if (categoryResponse == null)
        {
            Logger.Log("Failed to create category");
            return;
        }

        observableCat.Id = categoryResponse.Id;
        Logger.Log($"Added or updated category: {observableCat}");
    }

    public async Task DeleteAsync(ObservableCategory observableCategory)
    {
        var uri = $"/api/lists/{observableCategory.ListId}/categories/{observableCategory.Id}";
        var response = await LoggedRequest(() => _httpClient.DeleteAsync(uri));

        if (!response.IsSuccessStatusCode)
        {
            await ParseErrorResponse(response);
        }
    }

    public async Task DeleteAllByListIdAsync(string listId)
    {
        var uri = $"/api/lists/{listId}/categories";
        var response = await LoggedRequest(() => _httpClient.DeleteAsync(uri));

        if (!response.IsSuccessStatusCode)
        {
            await ParseErrorResponse(response);
        }
    }
}
