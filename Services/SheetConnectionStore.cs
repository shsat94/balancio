using System.Text.Json;
using Balancio.Models;
using Microsoft.Maui.Storage;

namespace Balancio.Services;

public class SheetConnectionStore
{
    private const string ConnectionsKey = "saved_sheet_connections";
    private const string LastUsedKey = "last_used_sheet_url";

    public List<SheetConnection> GetAll()
    {
        var json = Preferences.Default.Get(ConnectionsKey, string.Empty);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<SheetConnection>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<SheetConnection>>(json)
                   ?? new List<SheetConnection>();
        }
        catch
        {
            return new List<SheetConnection>();
        }
    }

    public void Add(SheetConnection connection)
    {
        var all = GetAll();
        all.Add(connection);

        var json = JsonSerializer.Serialize(all);
        Preferences.Default.Set(ConnectionsKey, json);
    }
    public void Delete(SheetConnection connection)
    {
        var all = GetAll();
        all.RemoveAll(c => c.Url == connection.Url);

        var json = JsonSerializer.Serialize(all);
        Preferences.Default.Set(ConnectionsKey, json);
    }

    public string? GetLastUsedUrl() =>
        Preferences.Default.Get(LastUsedKey, (string?)null);

    public void SetLastUsedUrl(string url) =>
        Preferences.Default.Set(LastUsedKey, url);
}