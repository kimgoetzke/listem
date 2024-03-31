using System.Diagnostics.CodeAnalysis;
using AsyncAwaitBestPractices;
using Listem.Mobile.Models;
using Listem.Mobile.ViewModel;
using Microsoft.Extensions.Logging;
#if __ANDROID__ || __IOS__
using CommunityToolkit.Maui.Core;
#endif

// ReSharper disable UnusedMember.Local
namespace Listem.Mobile.Views;

public partial class ListPage
{
  private const uint AnimationDuration = 400u;
  private Entry EntryField { get; set; } = null!;
  private Frame EntryFieldFrame { get; set; } = null!;
  private Picker CategoryPicker { get; set; } = null!;
  private Frame CategoryPickerFrame { get; set; } = null!;
  private Button AddButton { get; set; } = null!;

  private readonly ListViewModel _viewModel;
  private readonly ILogger<ListPage> _logger;

  public ListPage(List list, IServiceProvider serviceProvider)
  {
    InitializeComponent();
    _logger = serviceProvider.GetService<ILogger<ListPage>>()!;
    _viewModel = new ListViewModel(list, serviceProvider);
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
    _viewModel.GetSortedItems();
  }

  [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
  protected override void OnDisappearing()
  {
    base.OnDisappearing();
    _logger.Debug("Removing selected item(s), if applicable");
    foreach (var item in _viewModel.ItemsToDelete)
    {
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
      Padding = new Thickness(5, 15, 5, 5),
      RowDefinitions = { new RowDefinition { Height = GridLength.Auto } },
      ColumnDefinitions =
      {
        new ColumnDefinition { Width = new GridLength(250) },
        new ColumnDefinition { Width = new GridLength(150) },
        new ColumnDefinition { Width = new GridLength(150) },
        new ColumnDefinition { Width = new GridLength(100) },
        new ColumnDefinition { Width = GridLength.Star },
        new ColumnDefinition { Width = new GridLength(150) }
      }
    };

    menuGrid.Add(EntryFieldFrame, 0);
    menuGrid.Add(CategoryPickerFrame, 1);

    if (_viewModel.CurrentList.ListType == ListType.Shopping.ToString())
    {
      var quantityGrid = GetQuantityGrid();
      menuGrid.Add(quantityGrid, 3);
    }
    else
    {
      Grid.SetColumnSpan(CategoryPickerFrame, 2);
    }

    menuGrid.Add(importantGrid, 4);
    menuGrid.Add(AddButton, 5);
    return menuGrid;
  }

  private Grid CreateGridOnMobile(IView importantGrid)
  {
    var menuGrid = new Grid
    {
      RowSpacing = 0,
      Padding = new Thickness(10, 15, 15, 0),
      RowDefinitions =
      {
        new RowDefinition { Height = GridLength.Auto },
        new RowDefinition { Height = GridLength.Auto }
      },
      ColumnDefinitions =
      {
        new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) },
        new ColumnDefinition { Width = new GridLength(0.25, GridUnitType.Star) },
        new ColumnDefinition { Width = new GridLength(0.25, GridUnitType.Star) }
      }
    };

    menuGrid.Add(EntryFieldFrame, 0);
    Grid.SetColumnSpan(EntryFieldFrame, 2);
    menuGrid.Add(AddButton, 2);
    menuGrid.Add(CategoryPickerFrame, 0, 1);

    if (_viewModel.CurrentList.ListType == ListType.Shopping.ToString())
    {
      var quantityGrid = GetQuantityGrid();
      menuGrid.Add(quantityGrid, 1, 1);
    }
    else
    {
      Grid.SetColumnSpan(CategoryPickerFrame, 2);
    }

    menuGrid.Add(importantGrid, 2, 1);
    return menuGrid;
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
      WidthRequest = 75,
#endif
      Text = "Add",
      Margin = new Thickness(5, 0),
      AutomationId = "ListPageAddButton",
      Style = (Style)Application.Current!.Resources["GradientButton"],
      Command = new Command(() =>
      {
        AddButton.IsEnabled = false;
        _viewModel.NewItemName = EntryField.Text;
        _viewModel.AddItemCommand.ExecuteAsync(null).SafeFireAndForget();
        EntryField.Focus();
        AddButton.IsEnabled = true;
      }),
    };
  }

  private Picker GetCategoryPicker()
  {
    var categoryPicker = new Picker
    {
      Title = "Select category",
      AutomationId = "ListPageCategoryPicker",
      TextColor = (Color)Application.Current!.Resources["TextColor"],
      TitleColor = (Color)Application.Current.Resources["PickerTitleColor"],
      HeightRequest = (double)Application.Current.Resources["StandardSwipeItemHeight"],
      Margin = new Thickness(0),
      HorizontalOptions = LayoutOptions.Fill
    };
    categoryPicker.SetBinding(Picker.ItemsSourceProperty, "Categories");
    categoryPicker.SetBinding(Picker.SelectedItemProperty, "CurrentCategory");
    categoryPicker.SelectedIndexChanged += (sender, _) =>
    {
      if (sender is not Picker picker)
        return;

      if (picker.SelectedItem is not Category category)
        return;

      _viewModel.CurrentCategory = category;
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
      ColumnSpacing = 5,
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
      BackgroundColor = (Color)Application.Current!.Resources["BackgroundColor"],
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
      BackgroundColor = (Color)Application.Current!.Resources["BackgroundColor"],
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
      Text = _viewModel.NewItemName,
      Margin = new Thickness(0),
      FontSize = (double)Application.Current!.Resources["FontSizeM"],
      ReturnCommand = _viewModel.AddItemCommand
    };
    entryField.SetBinding(Entry.TextProperty, "NewItemName");
    entryField.Unfocused += (_, _) => AddButton.Focus();
    entryField.Completed += (_, _) =>
    {
      _viewModel.NewItemName = entryField.Text;
      entryField.Focus();
    };
    return entryField;
  }

  private Grid GetQuantityGrid()
  {
    var quantityGrid = new Grid { Margin = new Thickness(5) };
    var quantityLabel = new Label
    {
      Text = $"Quantity: {_viewModel.NewItemQuantity}",
      FontSize = (double)Application.Current!.Resources["FontSizeS"],
      FontFamily = "MulishSemiBold",
      VerticalTextAlignment = TextAlignment.Start,
      HorizontalTextAlignment = TextAlignment.Center,
      Margin = new Thickness(0, 8, 0, 0)
    };
    quantityGrid.Add(quantityLabel, 0);
    var quantityStepper = new Stepper
    {
      AutomationId = "ListPageQuantityStepper",
      Minimum = 1,
      Maximum = 99,
      Increment = 1,
      Scale = 0.6,
      HorizontalOptions = LayoutOptions.Center,
      Margin = new Thickness(0, 15, 0, 0)
    };
    quantityStepper.SetBinding(Stepper.ValueProperty, "NewItemQuantity");
    quantityStepper.ValueChanged += (_, e) =>
    {
      quantityLabel.Text = $"Quantity: {e.NewValue}";
    };
    quantityGrid.Add(quantityStepper, 0);
    return quantityGrid;
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
      Margin = new Thickness(0, 8, 0, 0)
    };
    isImportantGrid.Add(isImportantLabel, 0);
    var isImportantSwitch = new Switch
    {
      HorizontalOptions = LayoutOptions.Center,
      VerticalOptions = LayoutOptions.Start,
      Scale = 0.8,
      AutomationId = "ListPageIsImportantSwitch",
      Margin = new Thickness(0, 15, 0, 0)
    };
    isImportantSwitch.SetBinding(Switch.IsToggledProperty, "NewItemIsImportant");
    isImportantGrid.Add(isImportantSwitch, 0);
    return isImportantGrid;
  }
}
