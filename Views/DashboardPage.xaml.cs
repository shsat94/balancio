using Balancio.ViewModels;
using Balancio.Models;
using Balancio.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

    public DashboardPage()
    {
        InitializeComponent();

        _viewModel = new DashboardViewModel();
        _googleSheetService = new GoogleSheetService();

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
        const string sheetUrl =
            "https://docs.google.com/spreadsheets/d/e/2PACX-1vRxSKFR7CPl5U3JGS97n1zoTv6YLKiBxQiQwkPt7gtBG2zWkftZxvVPL09dm8hNeYM3lIxTw_Jjt4dm/pub?output=csv";

        try
        {
            // Load data from Google Sheet
            var categories =
                await _googleSheetService.LoadCategoriesAsync(sheetUrl);

            // Replace local data with Google Sheet data
            _viewModel.Balance.Categories.Clear();

            foreach (var category in categories)
            {
                _viewModel.Balance.Categories.Add(category);
            }

            // Assign the same existing colors
            AssignCategoryColors(_viewModel.Balance.Categories);

            // Update chart data
            _chartDrawable.UpdateCategories(
                _viewModel.Balance.Categories);

            BalanceChart.Invalidate();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Google Sheet error: {ex}");
        }

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