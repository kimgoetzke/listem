using Listem.Models;
using Listem.ViewModel;

namespace Listem.Views;

public partial class EditListPage
{
    public EditListPage(ObservableItemList observableItemList)
    {
        InitializeComponent();
        BindingContext = new EditListViewModel(observableItemList);
    }
}
