using Balancio.Models;
using System.Globalization;

namespace Balancio.Services;

public class GoogleSheetService
{
    private readonly HttpClient _httpClient;

    public GoogleSheetService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<List<Category>> LoadCategoriesAsync(string sheetUrl)
    {
        var csv = await _httpClient.GetStringAsync(sheetUrl);

        var categories = new List<Category>();

        var lines = csv
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Skip header
        foreach (var line in lines.Skip(1))
        {
            var columns = ParseCsvLine(line);

            if (columns.Count < 3)
                continue;

            if (!decimal.TryParse(
                    columns[1],
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var monthlyAmount))
            {
                continue;
            }

            if (!decimal.TryParse(
                    columns[2],
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var totalAmount))
            {
                continue;
            }

            categories.Add(new Category
            {
                Name = columns[0].Trim(),
                MonthlyAmount = monthlyAmount,
                TotalAmount = totalAmount
            });
        }

        return categories;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = "";
        var insideQuotes = false;

        foreach (var character in line)
        {
            if (character == '"')
            {
                insideQuotes = !insideQuotes;
                continue;
            }

            if (character == ',' && !insideQuotes)
            {
                result.Add(current);
                current = "";
                continue;
            }

            current += character;
        }

        result.Add(current);

        return result;
    }
}