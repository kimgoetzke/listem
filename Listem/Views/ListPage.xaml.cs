using System.Diagnostics.CodeAnalysis;
using AsyncAwaitBestPractices;
using CommunityToolkit.Maui.Core;
using Listem.Models;
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

    public ListPage(ObservableItemList observableItemList)
    {
        InitializeComponent();
        _viewModel = new ListViewModel(observableItemList);
        BindingContext = _viewModel;
        InitialiseMenuToAddItems();
    }

    private void InitialiseMenuToAddItems()
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

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    protected override void OnAppearing()
    {
        base.OnAppearing();

#if __ANDROID__ || __IOS__
        var statusBarColor = (Color)Application.Current!.Resources["BackgroundColorAccent"];
        CommunityToolkit.Maui.Core.Platform.StatusBar.SetColor(statusBarColor);
        CommunityToolkit.Maui.Core.Platform.StatusBar.SetStyle(StatusBarStyle.DarkContent);
#endif
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        foreach (var item in _viewModel.ItemsToDelete)
        {
            Logger.Log($"Removing item: {item.Title} from list");
            _viewModel.RemoveItemCommand.ExecuteAsync(item).SafeFireAndForget();
        }

#if __ANDROID__ || __IOS__
        var statusBarColor = (Color)Application.Current!.Resources["StatusBarColor"];
        CommunityToolkit.Maui.Core.Platform.StatusBar.SetColor(statusBarColor);
        CommunityToolkit.Maui.Core.Platform.StatusBar.SetStyle(StatusBarStyle.DarkContent);
#endif
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
        importantCheckBox.SetBinding(CheckBox.IsCheckedProperty, "NewObservableItem.IsImportant");
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
            Command = new Command(() =>
            {
                AddButton.IsEnabled = false;
                _viewModel.NewObservableItem.Title = EntryField.Text;
                _viewModel
                    .AddItemCommand.ExecuteAsync(_viewModel.NewObservableItem)
                    .SafeFireAndForget();
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
            Text = $"Quantity: {_viewModel.NewObservableItem.Quantity}",
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
        quantityStepper.SetBinding(Stepper.ValueProperty, "NewObservableItem.Quantity");
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

            if (picker.SelectedItem is not ObservableCategory category)
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
            Text = _viewModel.NewObservableItem.Title,
            Margin = new Thickness(5),
            FontSize = 16,
            ReturnCommand = _viewModel.AddItemCommand
        };
        entryField.SetBinding(Entry.TextProperty, "NewObservableItem.Title");
        entryField.Unfocused += (_, _) => AddButton.Focus();
        entryField.Completed += (_, _) =>
        {
            _viewModel.NewObservableItem.Title = entryField.Text;
            entryField.Focus();
        };
        return entryField;
    }
}
