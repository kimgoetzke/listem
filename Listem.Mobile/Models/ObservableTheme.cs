using CommunityToolkit.Mvvm.ComponentModel;
using Listem.Mobile.Utilities;

namespace Listem.Mobile.Models;

public partial class ObservableTheme : ObservableObject
{
    [ObservableProperty]
    private ThemeHandler.Theme _name;

    public override string ToString()
    {
        return Name.ToString();
    }
}
