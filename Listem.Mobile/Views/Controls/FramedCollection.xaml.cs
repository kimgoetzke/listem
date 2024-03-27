using System.Windows.Input;

namespace Listem.Mobile.Views.Controls;

public partial class FramedCollection
{
  public FramedCollection()
  {
    InitializeComponent();
  }

  public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
    nameof(ItemsSource),
    typeof(IEnumerable<string>),
    typeof(FramedCollection),
    propertyChanged: OnItemsSourceChanged
  );

  public IEnumerable<string> ItemsSource
  {
    get => (IEnumerable<string>)GetValue(ItemsSourceProperty);
    set => SetValue(ItemsSourceProperty, value);
  }

  private static void OnItemsSourceChanged(
    BindableObject bindable,
    object oldValue,
    object newValue
  )
  {
    var control = (FramedCollection)bindable;
    control.ItemsCollectionView.ItemsSource = (IEnumerable<string>)newValue;
  }

  public static readonly BindableProperty DataTypeProperty = BindableProperty.Create(
    nameof(DataType),
    typeof(Type),
    typeof(FramedCollection)
  );

  public Type DataType
  {
    get => (Type)GetValue(DataTypeProperty);
    set => SetValue(DataTypeProperty, value);
  }

  public static readonly BindableProperty SwipeCommandProperty = BindableProperty.Create(
    nameof(SwipeCommand),
    typeof(ICommand),
    typeof(FramedCollection)
  );

  public ICommand SwipeCommand
  {
    get => (ICommand)GetValue(SwipeCommandProperty);
    set => SetValue(SwipeCommandProperty, value);
  }
}
