namespace Listem.Mobile.Views;

public partial class WelcomePopup
{
    public WelcomePopup()
    {
        InitializeComponent();
    }

    private void OnButtonClicked(object sender, EventArgs e)
    {
        Close();
    }
}
