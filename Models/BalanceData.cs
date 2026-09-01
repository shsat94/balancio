using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Balancio.Models;

public class BalanceData : INotifyPropertyChanged
{
    public ObservableCollection<Category> Categories { get; set; } = new();

    public decimal TotalBalance =>
        Categories.Sum(x => x.TotalAmount);

    public decimal MonthlyTotal =>
        Categories.Sum(x => x.MonthlyAmount);

    public BalanceData()
    {
        Categories.CollectionChanged += (_, __) =>
        {
            OnPropertyChanged(nameof(TotalBalance));
            OnPropertyChanged(nameof(MonthlyTotal));
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}