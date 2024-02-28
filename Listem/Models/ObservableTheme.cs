using CommunityToolkit.Mvvm.ComponentModel;
using Listem.Utilities;

namespace Listem.Models;

public partial class ObservableTheme : ObservableObject
{
    [ObservableProperty]
    private ThemeHandler.Theme _name;

    public override string ToString()
    {
        return Name.ToString();
    }
}
