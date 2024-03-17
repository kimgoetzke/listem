using System.Net.Http.Headers;
using System.Net.Http.Json;
using Listem.Mobile.Models;
using Listem.Mobile.Utilities;
using Listem.Shared.Contracts;
using SQLite;
using Models_Category = Listem.Mobile.Models.Category;

namespace Listem.Mobile.Services;

public class OnlineListService : IOnlineListService
{
    private readonly AuthService _authService;
    private readonly HttpClient _httpClient;
    private readonly IDatabaseProvider _db;

    public OnlineListService(IServiceProvider serviceProvider)
    {
        _authService = serviceProvider.GetService<AuthService>()!;
        _db = serviceProvider.GetService<IDatabaseProvider>()!;
        var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>()!;
        _httpClient = httpClientFactory.CreateClient(Constants.HttpClientName);
    }

    public async Task<List<ObservableList>> GetAllAsync()
    {
        if (_authService.IsOnline())
        {
            var token = _authService.CurrentUser.AccessToken;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                token
            );
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
            var response = await _httpClient.GetAsync("/api/lists");
            Logger.Log($"Responded '{response.StatusCode}' to POST /api/lists: {response}");
            if (!response.IsSuccessStatusCode)
            {
                return [];
            }
            var lists = await response.Content.ReadFromJsonAsync<List<ListResponse>>();
            return ConvertToObservableItemLists(lists);
        }

        return [];
    }

    private static List<ObservableList> ConvertToObservableItemLists(List<ListResponse>? lists)
    {
        return lists?.Select(ObservableList.From).ToList() ?? [];
    }

    public async Task CreateOrUpdateAsync(ObservableList observableList)
    {
        var connection = await _db.GetConnection();
        var list = observableList.ToItemList();
        var existingList = await connection
            .Table<List>()
            .Where(i => i.Id == observableList.Id)
            .FirstOrDefaultAsync();
        if (existingList != null)
        {
            await connection.UpdateAsync(list);
            Logger.Log($"Updated list: {list.ToLoggableString()}");
            return;
        }

        await connection.InsertAsync(list);
        Logger.Log($"Added list: {list.ToLoggableString()}");
        await CreateDefaultCategory(connection, list.Id);
    }

    private static async Task CreateDefaultCategory(SQLiteAsyncConnection connection, string listId)
    {
        var existingDefaultCategory = await connection
            .Table<Models_Category>()
            .Where(i => i.ListId == listId && i.Name == ICategoryService.DefaultCategoryName)
            .FirstOrDefaultAsync();

        if (existingDefaultCategory != null)
        {
            Logger.Log($"Default category already exists for list {listId} - skipping creation");
            return;
        }

        var observableCategory = new ObservableCategory(listId)
        {
            Name = ICategoryService.DefaultCategoryName
        };
        var category = observableCategory.ToCategory();
        await connection.InsertAsync(category).ConfigureAwait(false);
        Logger.Log($"Added category '{ICategoryService.DefaultCategoryName}' to list {listId}");
    }

    public async Task DeleteAsync(ObservableList observableList)
    {
        // TODO: Delete all categories and items associated with this list
        Logger.Log($"Removing list: '{observableList.Name}' {observableList.Id}");
        var connection = await _db.GetConnection();
        var list = observableList.ToItemList();
        await connection.DeleteAsync(list);
    }

    public async Task DeleteAllAsync()
    {
        // TODO: Delete all categories and items associated with all lists
        var connection = await _db.GetConnection();
        var allLists = await connection.Table<List>().ToListAsync();
        foreach (var list in allLists)
        {
            await connection.DeleteAsync(list);
        }

        Logger.Log($"Removed all lists");
    }
}
