using CommunityToolkit.Mvvm.ComponentModel;
using Listem.Utilities;

namespace Listem.Models;

public partial class Theme : ObservableObject
{
    [ObservableProperty]
    private Settings.Theme _name;

    public override string ToString()
    {
        return Name.ToString();
    }
}
