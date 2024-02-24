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
    private Frame EntryFieldFrame { get; set; } = null!;
    private Picker CategoryPicker { get; set; } = null!;
    private Frame CategoryPickerFrame { get; set; } = null!;
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
        EntryFieldFrame = GetFrameForEntryField();
        CategoryPicker = GetCategoryPicker();
        CategoryPickerFrame = GetFrameForCategoryPicker();
        var isImportantGrid = GetImportantGrid();
        AddButton = GetAddButton();
#if WINDOWS || __MACOS__
        var menuGrid = CreateGridOnDesktop(isImportantGrid);
#elif __IOS__ || __ANDROID__
        var menuGrid = CreateGridOnMobile(isImportantGrid);
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

    private Grid CreateGridOnDesktop(IView importantGrid)
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

        menuGrid.Add(EntryFieldFrame, 0);
        menuGrid.Add(CategoryPickerFrame, 1);
        Grid.SetColumnSpan(CategoryPickerFrame, 2);
        menuGrid.Add(importantGrid, 3);
        menuGrid.Add(AddButton, 4);
        return menuGrid;
    }

    private Grid CreateGridOnMobile(IView importantGrid)
    {
        var menuGrid = new Grid
        {
            RowSpacing = 0,
            Padding = new Thickness(10, 5, 15, 0),
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

        menuGrid.Add(EntryFieldFrame, 0);
        Grid.SetColumnSpan(EntryFieldFrame, 2);
        menuGrid.Add(AddButton, 2);
        menuGrid.Add(CategoryPickerFrame, 0, 1);
        Grid.SetColumnSpan(CategoryPickerFrame, 2);
        menuGrid.Add(importantGrid, 2, 1);
        return menuGrid;
    }

    private static Grid GetImportantGrid()
    {
        var isImportantGrid = new Grid { Margin = new Thickness(5) };
        var isImportantLabel = new Label
        {
            Text = "Important",
            FontSize = (double)Application.Current!.Resources["FontSizeS"],
            FontFamily = "MulishSemiBold",
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, 10, 0, 0)
        };
        isImportantGrid.Add(isImportantLabel, 0);
        var isImportantSwitch = new Switch
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Start,
            Scale = 0.8,
            AutomationId = "ListPageIsImportantSwitch",
            Margin = new Thickness(0, 10, 0, 0)
        };
        isImportantSwitch.SetBinding(Switch.IsToggledProperty, "NewObservableItem.IsImportant");
        isImportantGrid.Add(isImportantSwitch, 0);
        return isImportantGrid;
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
            Margin = new Thickness(5, 0),
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
            Margin = new Thickness(10, 0, 0, 0)
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

    private Frame GetFrameForCategoryPicker()
    {
        var searchImage = new Image
        {
            Source = "search_tertiary.png",
            HeightRequest = 14,
            WidthRequest = 14,
            VerticalOptions = LayoutOptions.Center
        };

        var categoryPickerGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star }
            },
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.Fill,
            Padding = new Thickness(0)
        };
        categoryPickerGrid.Add(searchImage, 0);
        categoryPickerGrid.Add(CategoryPicker, 1);

        return new Frame
        {
            CornerRadius = 10,
            HeightRequest = 40,
            Content = categoryPickerGrid,
            BorderColor = Colors.Transparent,
            Margin = new Thickness(10, 0),
            Padding = new Thickness(10, 0),
            HasShadow = false,
            BackgroundColor = (Color)Application.Current!.Resources["Gray100"],
            AutomationId = "ListPageCategoryFrame"
        };
    }

    private Frame GetFrameForEntryField()
    {
        return new Frame
        {
            CornerRadius = 10,
            HeightRequest = 40,
            Content = EntryField,
            BorderColor = Colors.Transparent,
            Margin = new Thickness(10, 0),
            Padding = new Thickness(10, 0),
            HasShadow = false,
            BackgroundColor = (Color)Application.Current!.Resources["Gray100"],
            AutomationId = "ListPageEntryFrame"
        };
    }

    private Entry GetEntryField()
    {
        var entryField = new Entry
        {
            Placeholder = "Enter item name",
            AutomationId = "ListPageEntryField",
            FontFamily = "MulishLight",
            Text = _viewModel.NewObservableItem.Title,
            Margin = new Thickness(0),
            FontSize = (double)Application.Current!.Resources["FontSizeM"],
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
