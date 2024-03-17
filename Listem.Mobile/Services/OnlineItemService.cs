using System.Net.Http.Json;
using Listem.Mobile.Models;
using Listem.Mobile.Utilities;
using Listem.Shared.Contracts;
using static Listem.Mobile.Utilities.HttpUtilities;

namespace Listem.Mobile.Services;

public class OnlineItemService : IOnlineItemService
{
    private readonly HttpClient _httpClient;

    public OnlineItemService(IServiceProvider serviceProvider)
    {
        var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>()!;
        var authService = serviceProvider.GetService<AuthService>()!;
        _httpClient = httpClientFactory.CreateClient(Constants.HttpClientName);
        _httpClient.SetDefaultHeaders(authService.CurrentUser.AccessToken);
    }

    public async Task<List<ObservableItem>> GetAllByListIdAsync(string listId)
    {
        var getUri = $"/api/lists/{listId}/items";
        var response = await LoggedRequest(() => _httpClient.GetAsync(getUri));
        var items = await response.Content.ReadFromJsonAsync<List<ItemResponse>>();
        return ConvertToObservableItems(items);
    }

    private static List<ObservableItem> ConvertToObservableItems(List<ItemResponse>? items)
    {
        return items?.Select(ObservableItem.From).ToList() ?? [];
    }

    public async Task CreateOrUpdateAsync(ObservableItem observableItem)
    {
        HttpResponseMessage? response;
        var content = Content(observableItem.ToItemRequest());
        if (observableItem.Id == null)
        {
            var postUri = $"/api/lists/{observableItem.ListId}/items";
            response = await LoggedRequest(() => _httpClient.PostAsync(postUri, content));
        }
        else
        {
            var putUri = $"/api/lists/{observableItem.ListId}/items/{observableItem.Id}";
            response = await LoggedRequest(() => _httpClient.PutAsync(putUri, content));
        }

        if (!response.IsSuccessStatusCode)
        {
            await ParseErrorResponse(response);
        }

        var itemResponse = await response.Content.ReadFromJsonAsync<CategoryResponse>();
        if (itemResponse == null)
        {
            Logger.Log("Failed to create item");
            return;
        }

        observableItem.Id = itemResponse.Id;
        Logger.Log($"Added or updated item: {observableItem}");
    }

    public async Task DeleteAsync(ObservableItem observableItem)
    {
        var uri = $"/api/lists/{observableItem.ListId}/items/{observableItem.Id}";
        var response = await LoggedRequest(() => _httpClient.DeleteAsync(uri));

        if (!response.IsSuccessStatusCode)
        {
            await ParseErrorResponse(response);
        }
    }

    public async Task DeleteAllByListIdAsync(string listId)
    {
        var uri = $"/api/lists/{listId}/items";
        var response = await LoggedRequest(() => _httpClient.DeleteAsync(uri));

        if (!response.IsSuccessStatusCode)
        {
            await ParseErrorResponse(response);
        }
    }

    public Task UpdateAllToDefaultCategoryAsync(string listId)
    {
        Logger.Log("Updating all items to use default category is a no-op when using the API");
        return Task.CompletedTask;
    }

    public Task UpdateAllToCategoryAsync(string categoryName, string listId)
    {
        Logger.Log(
            $"Updating all items use category '{categoryName}' is a no-op when using the API"
        );
        return Task.CompletedTask;
    }
}
