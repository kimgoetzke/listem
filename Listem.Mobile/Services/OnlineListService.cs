using System.Net.Http.Json;
using AsyncAwaitBestPractices;
using Listem.Mobile.Models;
using Listem.Mobile.Utilities;
using Listem.Shared.Contracts;
using static Listem.Mobile.Utilities.HttpUtilities;

namespace Listem.Mobile.Services;

public class OnlineListService : IOnlineListService
{
    private readonly HttpClient _httpClient;

    public OnlineListService(IServiceProvider serviceProvider)
    {
        var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>()!;
        var authService = serviceProvider.GetService<AuthService>()!;
        _httpClient = httpClientFactory.CreateClient(Constants.HttpClientName);
        _httpClient.SetDefaultHeaders(authService.CurrentUser.AccessToken);
    }

    public async Task<List<ObservableList>> GetAllAsync()
    {
        var response = await LoggedRequest(() => _httpClient.GetAsync("/api/lists"));
        if (!response.IsSuccessStatusCode)
        {
            await ParseErrorResponse(response);
            return [];
        }
        var lists = await response.Content.ReadFromJsonAsync<List<ListResponse>>();
        return ConvertToObservableItemLists(lists);
    }

    private static List<ObservableList> ConvertToObservableItemLists(List<ListResponse>? lists)
    {
        return lists?.Select(ObservableList.From).ToList() ?? [];
    }

    public async Task CreateOrUpdateAsync(ObservableList observableList)
    {
        HttpResponseMessage? response;
        var content = Content(observableList.ToListRequest());
        if (observableList.Id == null)
        {
            response = await LoggedRequest(() => _httpClient.PostAsync("/api/lists", content));
        }
        else
        {
            var putUri = $"/api/lists/{observableList.Id}";
            response = await LoggedRequest(() => _httpClient.PutAsync(putUri, content));
        }

        if (!response.IsSuccessStatusCode)
        {
            await ParseErrorResponse(response);
        }

        var listResponse = await response.Content.ReadFromJsonAsync<ListResponse>();
        if (listResponse == null)
        {
            Logger.Log("Failed to create list");
            return;
        }

        observableList.Id = listResponse.Id;
        Logger.Log($"Added or updated list: {observableList}");
    }

    public async Task DeleteAsync(ObservableList observableList)
    {
        var deleteUri = $"/api/lists/{observableList.Id}";
        var response = await LoggedRequest(() => _httpClient.DeleteAsync(deleteUri));

        if (!response.IsSuccessStatusCode)
        {
            await ParseErrorResponse(response);
        }
    }

    public async Task DeleteAllAsync()
    {
        var response = await LoggedRequest(() => _httpClient.GetAsync("/api/lists"));
        if (!response.IsSuccessStatusCode)
        {
            await ParseErrorResponse(response);
            return;
        }
        var allLists = await response.Content.ReadFromJsonAsync<List<ListResponse>>();
        foreach (var list in allLists ?? [])
        {
            DeleteAsync(ObservableList.From(list)).SafeFireAndForget();
        }
    }
}
