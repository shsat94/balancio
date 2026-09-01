using Balancio.Models;
using Balancio.Services;

namespace Balancio.ViewModels;

public class DashboardViewModel
{
    private readonly GoogleSheetService _googleSheetService;

    public BalanceData Balance { get; private set; } = new();

    public DashboardViewModel()
    {
        _googleSheetService = new GoogleSheetService();
    }
}