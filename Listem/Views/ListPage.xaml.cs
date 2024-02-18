using CommunityToolkit.Maui.Views;
using Listem.Models;
using Listem.Services;
using Listem.Utilities;
using Listem.ViewModel;

// ReSharper disable UnusedMember.Local

namespace Listem.Views;

public partial class ListPage
{
    private const uint AnimationDuration = 400u;
    private readonly ListViewModel _viewModel;
    private Entry EntryField { get; set; } = null!;
    private Button AddButton { get; set; } = null!;

    public ListPage(ItemList itemList)
    {
        InitializeComponent();
        var services = new List<IService?>
        {
            IPlatformApplication.Current?.Services.GetService<ICategoryService>(),
            IPlatformApplication.Current?.Services.GetService<IItemService>(),
            IPlatformApplication.Current?.Services.GetService<IClipboardService>()
        };
        if (services.Any(s => s is null))
            throw new NullReferenceException($"One or more services are null");
        _viewModel = new ListViewModel(services!, itemList);
        BindingContext = _viewModel;
        AddMenuToAddItems();
    }

    private void AddMenuToAddItems()
    {
        EntryField = GetEntryField();
        var categoryPicker = GetCategoryPicker();
        var quantityGrid = GetQuantityGrid();
        var importantGrid = GetImportantGrid();
        AddButton = GetAddButton();
#if WINDOWS || __MACOS__
        var menuGrid = CreateGridOnDesktop(categoryPicker, quantityGrid, importantGrid);
#elif __IOS__ || __ANDROID__
        var menuGrid = CreateGridOnMobile(categoryPicker, quantityGrid, importantGrid);
#endif
        PageContentGrid.Add(menuGrid, 0, 1);
    }

    private Grid CreateGridOnDesktop(IView categoryPicker, IView quantityGrid, IView importantGrid)
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
        menuGrid.Add(categoryPicker, 1);
        menuGrid.Add(quantityGrid, 2);
        menuGrid.Add(importantGrid, 3);
        menuGrid.Add(AddButton, 4);
        return menuGrid;
    }

    private Grid CreateGridOnMobile(IView categoryPicker, IView quantityGrid, IView importantGrid)
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
        menuGrid.Add(categoryPicker, 0, 1);
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
            AutomationId = "ListPageIsImportantCheckBox",
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
            AutomationId = "ListPageAddButton",
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
            AutomationId = "ListPageQuantityStepper",
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

    private Picker GetCategoryPicker()
    {
        var categoryPicker = new Picker
        {
#if WINDOWS || __MACOS__
            Title = "",
#elif __ANDROID__ || __IOS__
            Title = "Select category",
#endif
            AutomationId = "ListPageCategoryPicker",
            TextColor = (Color)Application.Current!.Resources["TextColor"],
            TitleColor = (Color)Application.Current.Resources["PickerTitleColor"],
            HeightRequest = (double)Application.Current.Resources["StandardSwipeItemHeight"],
            Margin = new Thickness(5)
        };
        categoryPicker.SetBinding(Picker.SelectedItemProperty, "CurrentCategory");
        categoryPicker.SetBinding(Picker.ItemsSourceProperty, "Categories");
        categoryPicker.SelectedIndexChanged += (sender, _) =>
        {
            if (sender is not Picker picker)
                return;

            if (picker.SelectedItem is not Category category)
                return;

            _viewModel.CurrentCategory = category;
            Logger.Log("Current category updated to: " + _viewModel.CurrentCategory.Name);
        };
        return categoryPicker;
    }

    private Entry GetEntryField()
    {
        var entryField = new Entry
        {
            Placeholder = "Enter item name",
            AutomationId = "ListPageEntryField",
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
