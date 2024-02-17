using CommunityToolkit.Maui.Views;
using Listem.Models;
using Listem.Services;
using Listem.Utilities;
using Listem.ViewModel;
using Listem.Views;

// ReSharper disable UnusedMember.Local

namespace Listem.Views;

public partial class ListPage
{
    private const uint AnimationDuration = 400u;
    private bool _isMenuOpen;
    private readonly ListViewModel _viewModel;
    private Entry EntryField { get; set; } = null!;
    private Button AddButton { get; set; } = null!;

    public ListPage(ItemList itemList)
    {
        InitializeComponent();
        var services = new List<IService?>
        {
            IPlatformApplication.Current?.Services.GetService<IStoreService>(),
            IPlatformApplication.Current?.Services.GetService<IItemService>(),
            IPlatformApplication.Current?.Services.GetService<IClipboardService>()
        };
        if (services.Any(s => s is null))
            throw new NullReferenceException($"One or more services are null");
        _viewModel = new ListViewModel(services!, itemList);
        BindingContext = _viewModel;
        AddMenuToAddItems();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await _viewModel.LoadStoresFromDatabase();
            await _viewModel.LoadItemsFromDatabase();
        }
        catch (Exception e)
        {
            Logger.Log($"Failed fetching data from database: {e}");
            Notifier.ShowToast("Failed to load data");
        }
    }

    private void ToolbarItem_CopyOnClicked(object? sender, EventArgs e) =>
        _viewModel.CopyToClipboard();

    private void ToolbarItem_ImportOnClicked(object? sender, EventArgs e) =>
        _viewModel.InsertFromClipboard();

    private async void SettingsGrid_OnTapGridArea(object sender, EventArgs e)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        await CloseMenu(cancellationTokenSource);
    }

    private async void ToolbarItem_OnTapSettings(object sender, EventArgs e)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        if (!_isMenuOpen)
        {
            await OpenSettings(cancellationTokenSource);
            _isMenuOpen = true;
            return;
        }

        await CloseMenu(cancellationTokenSource);
        _isMenuOpen = false;
    }

    private async Task OpenSettings(CancellationTokenSource cancellationTokenSource)
    {
#if WINDOWS || __MACOS__
        var x = (Width - 250) / Width;
        var resize = PageContentGrid.ScaleTo(x, AnimationDuration);
        var move = PageContentGrid.TranslateTo(-Width * ((1 - x) / 2), 0, AnimationDuration);
        var tasks = new List<Task> { resize, move };
#elif __ANDROID__ || __IOS__
        var resize = PageContentGrid.TranslateTo(-Width * 0.25, 0, AnimationDuration);
        var scaleDown = PageContentGrid.ScaleTo(0.75, AnimationDuration);
        var rotate = PageContentGrid.RotateYTo(35, AnimationDuration, Easing.CubicIn);
        var tasks = new List<Task> { resize, scaleDown, rotate };
#endif
        await Task.WhenAll(tasks).WaitAsync(cancellationTokenSource.Token).ConfigureAwait(false);
    }

    private async Task CloseMenu(CancellationTokenSource cancellationTokenSource)
    {
#if WINDOWS || __MACOS__
        var scaleBack = PageContentGrid.ScaleTo(1, AnimationDuration / 2);
        var resize = PageContentGrid.TranslateTo(0, 0, AnimationDuration / 2);
        var tasks = new List<Task> { scaleBack, resize };
#elif __ANDROID__ || __IOS__
        await PageContentGrid.RotateYTo(0, AnimationDuration / 2);
        var fadeIn = PageContentGrid.FadeTo(1, AnimationDuration / 2);
        var scaleBack = PageContentGrid.ScaleTo(1, AnimationDuration / 2);
        var resize = PageContentGrid.TranslateTo(0, 0, AnimationDuration / 2);
        var tasks = new List<Task> { fadeIn, scaleBack, resize };
#endif
        await Task.WhenAll(tasks).WaitAsync(cancellationTokenSource.Token).ConfigureAwait(false);
    }

    private void SwipeItemView_OnInvoked(object? sender, EventArgs e)
    {
        // TODO: Give user feedback through particles or animation
        Logger.Log("OnInvokedSwipeItem");
    }

    private async void CheckBox_OnTaskCompleted(object? sender, CheckedChangedEventArgs e)
    {
        if (sender is not CheckBox { IsChecked: true } checkBox)
            return;

        if (checkBox.BindingContext is not Item item)
            return;

        await _viewModel.RemoveItem(item);
        await Task.Delay(200);
    }

    private void AddMenuToAddItems()
    {
        EntryField = GetEntryField();
        var storePicker = GetStorePicker();
        var quantityGrid = GetQuantityGrid();
        var importantGrid = GetImportantGrid();
        AddButton = GetAddButton();
#if WINDOWS || __MACOS__
        var menuGrid = CreateGridOnDesktop(storePicker, quantityGrid, importantGrid);
#elif __IOS__ || __ANDROID__
        var menuGrid = CreateGridOnMobile(storePicker, quantityGrid, importantGrid);
#endif
        PageContentGrid.Add(menuGrid, 0, 1);
    }

    private Grid CreateGridOnDesktop(IView storePicker, IView quantityGrid, IView importantGrid)
    {
        var menuGrid = new Grid
        {
            RowSpacing = 5,
            Padding = new Thickness(5),
            RowDefinitions = { new RowDefinition { Height = GridLength.Auto } },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(250) },
                new ColumnDefinition { Width = new GridLength(150) },
                new ColumnDefinition { Width = new GridLength(180) },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = new GridLength(150) }
            }
        };

        menuGrid.Add(EntryField, 0);
        menuGrid.Add(storePicker, 1);
        menuGrid.Add(quantityGrid, 2);
        menuGrid.Add(importantGrid, 3);
        menuGrid.Add(AddButton, 4);
        return menuGrid;
    }

    private Grid CreateGridOnMobile(IView storePicker, IView quantityGrid, IView importantGrid)
    {
        var menuGrid = new Grid
        {
            RowSpacing = 5,
            Padding = new Thickness(5),
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(0.375, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(0.375, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(0.25, GridUnitType.Star) }
            }
        };

        menuGrid.Add(EntryField, 0);
        Grid.SetColumnSpan(EntryField, 2);
        menuGrid.Add(AddButton, 2);
        menuGrid.Add(storePicker, 0, 1);
        menuGrid.Add(quantityGrid, 1, 1);
        menuGrid.Add(importantGrid, 2, 1);
        return menuGrid;
    }

    private static Grid GetImportantGrid()
    {
        var importantGrid = new Grid
        {
            Margin = new Thickness(5),
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star }
            }
        };
        var importantLabel = new Label
        {
            Text = "Important:",
            FontSize = 10,
            HorizontalTextAlignment = TextAlignment.Start,
            VerticalTextAlignment = TextAlignment.Center
        };
        importantGrid.Add(importantLabel, 0);
        var importantCheckBox = new CheckBox
        {
            HorizontalOptions = LayoutOptions.Start,
            AutomationId = "MainPageIsImportantCheckBox",
        };
        importantCheckBox.SetBinding(CheckBox.IsCheckedProperty, "NewItem.IsImportant");
        importantGrid.Add(importantCheckBox, 1);
        return importantGrid;
    }

    private Button GetAddButton()
    {
        return new Button
        {
#if WINDOWS || __MACOS__
            HorizontalOptions = LayoutOptions.End,
            WidthRequest = 140,
#elif __ANDROID__ || __IOS__
            HorizontalOptions = LayoutOptions.Fill,
#endif
            Text = "Add",
            AutomationId = "MainPageAddButton",
            Style = (Style)Application.Current!.Resources["StandardButton"],
            Command = new Command(async () =>
            {
                AddButton.IsEnabled = false;
                await _viewModel.AddItemCommand.ExecuteAsync(_viewModel.NewItem);
                EntryField.Focus();
                AddButton.IsEnabled = true;
            }),
        };
    }

    private Grid GetQuantityGrid()
    {
        var quantityGrid = new Grid
        {
            Margin = new Thickness(5),
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            }
        };
        var quantityLabel = new Label
        {
            Text = $"Quantity: {_viewModel.NewItem.Quantity}",
            FontSize = 10,
            VerticalTextAlignment = TextAlignment.Center,
            HorizontalTextAlignment = TextAlignment.Center
        };
        quantityGrid.Add(quantityLabel, 0);
        var quantityStepper = new Stepper
        {
            AutomationId = "MainPageQuantityStepper",
            Minimum = 1,
            Maximum = 99,
            Increment = 1,
            HorizontalOptions = LayoutOptions.Center
        };
        quantityStepper.SetBinding(Stepper.ValueProperty, "NewItem.Quantity");
        quantityStepper.ValueChanged += (_, e) =>
        {
            quantityLabel.Text = $"Quantity: {e.NewValue}";
        };
        quantityGrid.Add(quantityStepper, 1);
        return quantityGrid;
    }

    private Picker GetStorePicker()
    {
        var storePicker = new Picker
        {
#if WINDOWS || __MACOS__
            Title = "",
#elif __ANDROID__ || __IOS__
            Title = "Select store",
#endif
            AutomationId = "MainPageStorePicker",
            TextColor = (Color)Application.Current!.Resources["TextColor"],
            TitleColor = (Color)Application.Current.Resources["PickerTitleColor"],
            HeightRequest = (double)Application.Current.Resources["StandardSwipeItemHeight"],
            Margin = new Thickness(5)
        };
        storePicker.SetBinding(Picker.SelectedItemProperty, "CurrentStore");
        storePicker.SetBinding(Picker.ItemsSourceProperty, "Stores");
        storePicker.SelectedIndexChanged += (sender, _) =>
        {
            if (sender is not Picker picker)
                return;

            if (picker.SelectedItem is not ConfigurableStore store)
                return;

            _viewModel.CurrentStore = store;
            Logger.Log("Current store updated to: " + _viewModel.CurrentStore.Name);
        };
        return storePicker;
    }

    private Entry GetEntryField()
    {
        var entryField = new Entry
        {
            Placeholder = "Enter item name",
            AutomationId = "MainPageEntryField",
            Text = _viewModel.NewItem.Title,
            Margin = new Thickness(5),
            FontSize = 16,
            ReturnCommand = _viewModel.AddItemCommand
        };
        entryField.SetBinding(Entry.TextProperty, "NewItem.Title");
        entryField.Unfocused += (_, _) => AddButton.Focus();
        entryField.Completed += (_, _) => entryField.Focus();
        return entryField;
    }
}
