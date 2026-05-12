using CommunityToolkit.Mvvm.ComponentModel;

namespace CasinoProject.Models;

public partial class Laser : ObservableObject
{
    public double X { get; set; }

    [ObservableProperty]
    private double _y;

    public double Width { get; set; } = 4;
    public double Height { get; set; } = 15;

    public Laser(double x, double y)
    {
        X = x;
        Y = y;
    }
}
