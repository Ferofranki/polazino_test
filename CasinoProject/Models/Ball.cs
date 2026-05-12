using CommunityToolkit.Mvvm.ComponentModel;

namespace CasinoProject.Models;

public partial class Ball : ObservableObject
{
    [ObservableProperty]
    private double _x;

    [ObservableProperty]
    private double _y;

    public double VelocityX { get; set; }
    public double VelocityY { get; set; }

    public double Size { get; set; } = 20;

    [ObservableProperty]
    private bool _isStuck;

    // Offset from paddle center when stuck
    public double StuckOffsetX { get; set; }

    public Ball(double x, double y, double velocityX, double velocityY)
    {
        X = x;
        Y = y;
        VelocityX = velocityX;
        VelocityY = velocityY;
        IsStuck = false;
    }
}
