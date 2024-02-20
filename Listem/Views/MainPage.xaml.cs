using AsyncAwaitBestPractices;
using Listem.Utilities;
using Listem.ViewModel;

// ReSharper disable UnusedMember.Local

namespace Listem.Views;

public partial class MainPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        viewModel.LoadItemLists().SafeFireAndForget();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (!Settings.FirstRun)
            return;

        Logger.Log("First time running this application");
        Settings.FirstRun = false;
    }
}
