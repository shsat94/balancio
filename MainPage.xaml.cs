namespace Balancio;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        BalanceChart.Drawable = new BalanceChartDrawable();
    }
}

public class BalanceChartDrawable : IDrawable
{
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float centerX = dirtyRect.Center.X;
        float centerY = dirtyRect.Center.Y;

        float radius = 95;
        float thickness = 25;

        float[] values =
        {
            25000,
            42000,
            30000,
            12000
        };

        float total = values.Sum();

        float startAngle = -90;

        foreach (float value in values)
        {
            float sweepAngle = value / total * 360;

            canvas.StrokeSize = thickness;
            canvas.StrokeColor = Colors.Black;

            canvas.DrawArc(
                centerX - radius,
                centerY - radius,
                radius * 2,
                radius * 2,
                startAngle,
                sweepAngle,
                false,
                false);

            startAngle += sweepAngle;
        }
    }
}