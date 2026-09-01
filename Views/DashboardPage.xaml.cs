using Balancio.Models;
using Balancio.Services;
using Balancio.ViewModels;
using Balancio.Views.Popups;
using CommunityToolkit.Maui.Extensions;

namespace Balancio.Views;

public partial class DashboardPage : ContentPage
{
    // Rich, distinct palette — each category gets one, in order.
    private static readonly Color[] SegmentColors =
    {
        Color.FromArgb("#FF7A45"), // orange
        Color.FromArgb("#4D9DFF"), // blue
        Color.FromArgb("#3FD6A0"), // green
        Color.FromArgb("#B18BFF"), // purple
        Color.FromArgb("#FF5C7A"), // red/pink
        Color.FromArgb("#FFD166"), // yellow
        Color.FromArgb("#4EE0D8"), // teal
    };

    private readonly DashboardChartDrawable _chartDrawable;
    private readonly DashboardViewModel _viewModel;
    private readonly GoogleSheetService _googleSheetService;
    private readonly SheetConnectionStore _connectionStore;
    private bool _isLoading;
    private int _loadRequestId;

    public DashboardPage()
    {
        InitializeComponent();

        _viewModel = new DashboardViewModel();
        _googleSheetService = new GoogleSheetService();
        _connectionStore = new SheetConnectionStore();

        BindingContext = _viewModel;

        _chartDrawable = new DashboardChartDrawable(
            _viewModel.Balance.Categories,
            SegmentColors);

        BalanceChart.Drawable = _chartDrawable;
    }

    private static void AssignCategoryColors(
        IReadOnlyList<Category> categories)
    {
        for (var i = 0; i < categories.Count; i++)
        {
            categories[i].ColorHex =
                SegmentColors[i % SegmentColors.Length];
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Google Sheet URL

        var lastUsedUrl = _connectionStore.GetLastUsedUrl();
        var sheetUrl = _connectionStore.GetAll().Any(c => c.Url == lastUsedUrl)
            ? lastUsedUrl ?? ""
            : "";
        await LoadSheetDataAsync(sheetUrl);

        RootStack.TranslationY = 24;

        _ = RootStack.FadeTo(
            1,
            400,
            Easing.CubicOut);

        _ = RootStack.TranslateTo(
            0,
            0,
            400,
            Easing.CubicOut);

        await AnimateChartAsync();

        PulseTotal();
    }

    private async Task<bool> LoadSheetDataAsync(string sheetUrl)
    {
        var requestId = Interlocked.Increment(ref _loadRequestId);
        SetLoading(true);

        try
        {
            _viewModel.Balance.Categories.Clear();
            CategoryHeaders.IsVisible = false;

            if (string.IsNullOrWhiteSpace(sheetUrl))
            {
                _chartDrawable.UpdateCategories(_viewModel.Balance.Categories);
                BalanceChart.Invalidate();
                return true;
            }

            var categories = await _googleSheetService.LoadCategoriesAsync(sheetUrl);

            if (requestId != _loadRequestId)
            {
                return false;
            }

            foreach (var category in categories)
            {
                _viewModel.Balance.Categories.Add(category);
            }

            CategoryHeaders.IsVisible = _viewModel.Balance.Categories.Count > 0;

            AssignCategoryColors(_viewModel.Balance.Categories);

            _chartDrawable.UpdateCategories(_viewModel.Balance.Categories);
            BalanceChart.Invalidate();

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Google Sheet error: {ex}");
            await DisplayAlert("Couldn't load sheet", "Check the URL and try again.", "OK");
            return false;
        }
        finally
        {
            if (requestId == _loadRequestId)
            {
                SetLoading(false);
            }
        }
    }

    private void SetLoading(bool isLoading)
    {
        _isLoading = isLoading;
        LoadingOverlay.IsVisible = isLoading;
        ManageSheetsButton.IsEnabled = !isLoading;
    }

    private async void OnManageSheetsClicked(object sender, EventArgs e)
    {
        if (_isLoading)
        {
            return;
        }

        var currentUrl = _connectionStore.GetLastUsedUrl() ?? "";
        var listPopup = new SheetListPopup(_connectionStore, currentUrl);
        var result = (await this.ShowPopupAsync<object>(listPopup)).Result;

        if (result is SheetConnection selected)
        {
            if (await LoadSheetDataAsync(selected.Url))
            {
                _connectionStore.SetLastUsedUrl(selected.Url);
            }
        }
        else if (result is string signal && signal == "ADD_NEW")
        {
            var addPopup = new AddSheetPopup();
            var addResult = (await this.ShowPopupAsync<object>(addPopup)).Result;

            if (addResult is SheetConnection newConnection)
            {
                _connectionStore.Add(newConnection);

                if (await LoadSheetDataAsync(newConnection.Url))
                {
                    _connectionStore.SetLastUsedUrl(newConnection.Url);
                }
            }
        }
    }

    private Task AnimateChartAsync()
    {
        var tcs = new TaskCompletionSource<bool>();

        var animation = new Animation(v =>
        {
            _chartDrawable.Progress = (float)v;
            BalanceChart.Invalidate();
        },
        0,
        1,
        Easing.CubicOut);

        animation.Commit(
            this,
            "ChartSweep",
            16,
            900,
            finished: (_, __) =>
                tcs.TrySetResult(true));

        return tcs.Task;
    }

    private async void PulseTotal()
    {
        await TotalLabel.ScaleTo(
            1.08,
            150,
            Easing.CubicOut);

        await TotalLabel.ScaleTo(
            1.0,
            150,
            Easing.CubicIn);
    }
}

public sealed class DashboardChartDrawable : IDrawable
{
    private IReadOnlyList<Category> _categories;
    private readonly Color[] _palette;

    /// <summary>
    /// 0..1 — drives the entrance sweep animation.
    /// </summary>
    public float Progress { get; set; } = 1f;

    public DashboardChartDrawable(
        IReadOnlyList<Category> categories,
        Color[] palette)
    {
        _categories = categories;
        _palette = palette;
    }

    public void UpdateCategories(
        IReadOnlyList<Category> categories)
    {
        _categories = categories;
    }

    public void Draw(
        ICanvas canvas,
        RectF dirtyRect)
    {
        const float radius = 95;
        const float thickness = 25;

        var total = _categories.Sum(
            category => category.TotalAmount);

        if (total <= 0)
        {
            return;
        }

        var bounds = new RectF(
            dirtyRect.Center.X - radius,
            dirtyRect.Center.Y - radius,
            radius * 2,
            radius * 2);

        // Faint full-circle track behind the segments
        canvas.StrokeColor =
            Color.FromArgb("#26FFFFFF");

        canvas.StrokeSize = thickness;
        canvas.StrokeLineCap = LineCap.Round;

        canvas.DrawArc(
            bounds,
            -90,
            269.999f,
            false,
            false);

        canvas.DrawArc(
            bounds,
            180,
            89.999f,
            false,
            false);

        var startAngle = -90f;
        var maxSweep = 360f * Progress;
        var swept = 0f;

        for (var index = 0;
             index < _categories.Count;
             index++)
        {
            var fullSweep =
                (float)(
                    _categories[index].TotalAmount
                    / total
                    * 360m);

            if (fullSweep <= 0)
            {
                continue;
            }

            var remaining = maxSweep - swept;

            var sweepAngle = MathF.Max(
                0,
                MathF.Min(
                    fullSweep,
                    remaining));

            swept += fullSweep;

            if (sweepAngle > 0)
            {
                canvas.StrokeColor =
                    _palette[
                        index % _palette.Length];

                canvas.StrokeSize = thickness;
                canvas.StrokeLineCap = LineCap.Round;

                canvas.DrawArc(
                    bounds,
                    startAngle,
                    startAngle + sweepAngle,
                    false,
                    false);
            }

            startAngle += fullSweep;
        }
    }
}