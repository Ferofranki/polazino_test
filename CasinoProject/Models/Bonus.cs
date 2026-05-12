using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CasinoProject.Models;

public enum BonusType
{
    Expand,
    Slow,
    MultiBall,
    Cash,
    Sticky,
    Lasers
}

public partial class Bonus : ObservableObject
{
    [ObservableProperty]
    private double _x;

    [ObservableProperty]
    private double _y;

    [ObservableProperty]
    private double _size = 30;

    public BonusType Type { get; set; }

    [ObservableProperty]
    private IBrush _colorBrush;

    [ObservableProperty]
    private string _text;

    public Bonus(double x, double y, BonusType type)
    {
        X = x;
        Y = y;
        Type = type;

        switch(type)
        {
            case BonusType.Expand: ColorBrush = Brushes.LightBlue; Text = "E"; break;
            case BonusType.Slow: ColorBrush = Brushes.Orange; Text = "S"; break;
            case BonusType.MultiBall: ColorBrush = Brushes.LightGreen; Text = "M"; break;
            case BonusType.Cash: ColorBrush = Brushes.Gold; Text = "$"; break;
            case BonusType.Sticky: ColorBrush = Brushes.Pink; Text = "G"; break;
            case BonusType.Lasers: ColorBrush = Brushes.Red; Text = "L"; break;
            default: ColorBrush = Brushes.White; Text = "?"; break;
        }
    }
}
