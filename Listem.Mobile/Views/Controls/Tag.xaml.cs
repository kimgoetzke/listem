using System.Windows.Input;

namespace Listem.Mobile.Views.Controls;

public partial class Tag
{
  public static readonly BindableProperty TagColorProperty = BindableProperty.Create(
    nameof(TagColor),
    typeof(Color),
    typeof(Tag)
  );
  public static readonly BindableProperty CommandProperty = BindableProperty.Create(
    nameof(Command),
    typeof(ICommand),
    typeof(Tag)
  );
  public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
    nameof(CommandParameter),
    typeof(object),
    typeof(Tag)
  );
  public static readonly BindableProperty TextProperty = BindableProperty.Create(
    nameof(Text),
    typeof(string),
    typeof(Tag)
  );
  public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
    nameof(TextColor),
    typeof(Color),
    typeof(Tag)
  );

  public Color TagColor
  {
    get => (Color)GetValue(TagColorProperty);
    set => SetValue(TagColorProperty, value);
  }

  public ICommand Command
  {
    get => (ICommand)GetValue(CommandProperty);
    set => SetValue(CommandProperty, value);
  }

  public object CommandParameter
  {
    get => GetValue(CommandParameterProperty);
    set => SetValue(CommandParameterProperty, value);
  }

  public string Text
  {
    get => (string)GetValue(TextProperty);
    set => SetValue(TextProperty, value);
  }

  public Color TextColor
  {
    get => (Color)GetValue(TextColorProperty);
    set => SetValue(TextColorProperty, value);
  }

  public Tag()
  {
    InitializeComponent();
  }
}
