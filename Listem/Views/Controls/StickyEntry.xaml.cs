// ReSharper disable once RedundantUsingDirective
using CommunityToolkit.Maui.Core.Platform;
using CommunityToolkit.Mvvm.Input;
using Listem.Utilities;

namespace Listem.Views.Controls;

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

    public void SetVisibility(bool isVisible)
    {
        IsVisible = isVisible;
        if (isVisible)
        {
            StickyEntryField.Focus();
        }
        else
        {
            StickyEntryField.Unfocus();
        }
    }

    [RelayCommand]
    private void SubmitInput(ITextInput view)
    {
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
}
