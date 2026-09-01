using Balancio.Models;
using CommunityToolkit.Maui.Views;

namespace Balancio.Views.Popups;

public partial class SheetListPopup : Popup<object>
{
    public SheetListPopup(List<SheetConnection> connections)
    {
        InitializeComponent();
        ConnectionsList.ItemsSource = connections;
    }

    private async void OnConnectionTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is SheetConnection connection)
        {
            await CloseAsync(connection);
        }
    }

    private async void OnAddNewClicked(object sender, EventArgs e)
    {
        await CloseAsync("ADD_NEW");
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await CloseAsync(null);
    }
}