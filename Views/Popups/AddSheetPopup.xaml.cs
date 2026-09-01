using Balancio.Models;
using CommunityToolkit.Maui.Views;

namespace Balancio.Views.Popups;

public partial class AddSheetPopup : Popup<object>
{
    public AddSheetPopup()
    {
        InitializeComponent();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var alias = AliasEntry.Text?.Trim();
        var url = UrlEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(alias) || string.IsNullOrWhiteSpace(url))
        {
            ErrorLabel.Text = "Please fill in both fields.";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            ErrorLabel.Text = "Please enter a valid URL.";
            ErrorLabel.IsVisible = true;
            return;
        }

        await CloseAsync(new SheetConnection { Alias = alias, Url = url });
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await CloseAsync(null);
    }
}