using Listem.Services;
using Listem.ViewModel;

namespace Listem.Views;

public partial class CategoryPage
{
    public CategoryPage(string listId)
    {
        InitializeComponent();
        var services = new List<IService?>
        {
            IPlatformApplication.Current?.Services.GetService<ICategoryService>(),
            IPlatformApplication.Current?.Services.GetService<IItemService>(),
        };
        if (services.Any(s => s is null))
            throw new NullReferenceException($"One or more services are null");
        var viewModel = new CategoryViewModel(services!, listId);
        BindingContext = viewModel;
    }

    private void OnEntryUnfocused(object sender, FocusEventArgs e)
    {
        AddCategoryButton.Focus();
    }

    private void ImageButton_OnPressed(object? sender, EventArgs e)
    {
        if (sender is not ImageButton button)
            return;

        button.Source = "bin_primary.png";
    }

    private void ImageButton_OnReleased(object? sender, EventArgs e)
    {
        if (sender is not ImageButton button)
            return;

        button.Source = "bin_neutral.png";
    }
}
