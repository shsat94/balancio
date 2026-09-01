using Balancio.Models;
using Balancio.Services;
using CommunityToolkit.Maui.Views;

namespace Balancio.Views.Popups;

public partial class SheetListPopup : Popup<object>
{
    private readonly SheetConnectionStore _store;
    private readonly string? _currentUrl;

    public SheetListPopup(SheetConnectionStore store, string? currentUrl)
    {
        InitializeComponent();
        _store = store;
        _currentUrl = currentUrl;
        RefreshList();
    }

    private void RefreshList()
    {
        var items = _store.GetAll()
            .Select(c => new SheetConnection
            {
                Alias = c.Alias,
                Url = c.Url,
                IsCurrent = c.Url == _currentUrl
            })
            .ToList();

        ConnectionsList.ItemsSource = items;
    }

    private async void OnConnectionTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is SheetConnection item)
        {
            await CloseAsync(new SheetConnection { Alias = item.Alias, Url = item.Url });
        }
    }

    private void OnDeleteClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is SheetConnection item)
        {
            _store.Delete(new SheetConnection { Alias = item.Alias, Url = item.Url });
            RefreshList();
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