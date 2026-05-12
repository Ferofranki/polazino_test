using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CasinoProject.Models;

public partial class Block : ObservableObject
{
    [ObservableProperty]
    private double _x;

    [ObservableProperty]
    private double _y;

    [ObservableProperty]
    private double _width;

    [ObservableProperty]
    private double _height;

    [ObservableProperty]
    private IBrush _colorBrush;

    [ObservableProperty]
    private bool _isVisible = true;

    public Block(double x, double y, double width, double height, IBrush colorBrush)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        ColorBrush = colorBrush;
    }
}
