// ReSharper disable once RedundantUsingDirective

using CommunityToolkit.Maui.Core.Platform;
using CommunityToolkit.Mvvm.Input;
using Listem.Mobile.Utilities;

namespace Listem.Mobile.Views.Controls;

public partial class StickyEntry
{
    public event EventHandler<string> Submitted;

    public StickyEntry()
    {
        InitializeComponent();
        BindingContext = this;
        Submitted += (_, _) => { };
        SetVisibility(false);
    }

    [RelayCommand]
    private void SubmitInput(ITextInput view)
    {
        if (StickyEntryField.Text.Length == 0)
            return;

        Logger.Log($"Submitting input '{StickyEntryField.Text}'");
        Submitted(this, StickyEntryField.Text);
        StickyEntryField.Text = string.Empty;
        HideKeyboard(view);
        SetVisibility(false);
    }

    [RelayCommand]
    private void CancelInput(ITextInput view)
    {
        Logger.Log($"Cancelling input");
        StickyEntryField.Text = string.Empty;
        HideKeyboard(view);
        SetVisibility(false);
    }

    public void SetVisibility(bool isVisible)
    {
        IsVisible = isVisible;
        if (isVisible)
        {
            StickyEntryField.Focus();
            return;
        }

        StickyEntryField.Unfocus();
    }

    // ReSharper disable once UnusedParameter.Local
    private static void HideKeyboard(ITextInput view)
    {
#if __ANDROID__
        var isKeyboardHidden = view.HideKeyboardAsync(CancellationToken.None);
        Logger.Log("Keyboard hidden: " + isKeyboardHidden);
#endif
    }

    private void StickyEntryField_Unfocused(object? sender, FocusEventArgs e)
    {
        if (sender is ITextInput textInput)
        {
            HideKeyboard(textInput);
        }

        SetVisibility(false);
    }

    private void StickyEntryField_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not Entry entry)
            return;

        if (entry.Text.Length == 0)
        {
            StickyEntrySubmit.Source = "done_neutral.png";
            return;
        }

        StickyEntrySubmit.Source = "done_primary.png";
    }

    public static readonly BindableProperty PlaceholderTextProperty = BindableProperty.Create(
        nameof(PlaceholderText),
        typeof(string),
        typeof(StickyEntry),
        propertyChanged: OnPlaceholderTextChanged
    );

    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    private static void OnPlaceholderTextChanged(
        BindableObject bindable,
        object oldValue,
        object newValue
    )
    {
        var control = (StickyEntry)bindable;
        control.StickyEntryField.Placeholder = (string)newValue;
    }
}
