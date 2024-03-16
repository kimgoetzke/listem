using System.Net.Http.Headers;
using System.Net.Http.Json;
using Listem.Contracts;
using Listem.Models;
using Listem.Utilities;
using SQLite;
using Category = Listem.Models.Category;
using ItemList = Listem.Models.ItemList;

namespace Listem.Services;

public class OnlineItemListService : IItemListService
{
    private readonly AuthService _authService;
    private readonly HttpClient _httpClient;
    private readonly IDatabaseProvider _db;

    public OnlineItemListService(IServiceProvider serviceProvider)
    {
        _authService = serviceProvider.GetService<AuthService>()!;
        _db = serviceProvider.GetService<IDatabaseProvider>()!;
        var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>()!;
        _httpClient = httpClientFactory.CreateClient(Constants.HttpClientName);
    }

    public async Task<List<ObservableItemList>> GetAllAsync()
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

    private static List<ObservableItemList> ConvertToObservableItemLists(List<ListResponse>? lists)
    {
        return lists?.Select(ObservableItemList.From).ToList() ?? [];
    }

    public async Task CreateOrUpdateAsync(ObservableItemList observableItemList)
    {
        var connection = await _db.GetConnection();
        var list = observableItemList.ToItemList();
        var existingList = await connection
            .Table<ItemList>()
            .Where(i => i.Id == observableItemList.Id)
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
            .Table<Category>()
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

    public async Task DeleteAsync(ObservableItemList observableItemList)
    {
        // TODO: Delete all categories and items associated with this list
        Logger.Log($"Removing list: '{observableItemList.Name}' {observableItemList.Id}");
        var connection = await _db.GetConnection();
        var list = observableItemList.ToItemList();
        await connection.DeleteAsync(list);
    }

    public async Task DeleteAllAsync()
    {
        // TODO: Delete all categories and items associated with all lists
        var connection = await _db.GetConnection();
        var allLists = await connection.Table<ItemList>().ToListAsync();
        foreach (var list in allLists)
        {
            await connection.DeleteAsync(list);
        }

        Logger.Log($"Removed all lists");
    }
}
