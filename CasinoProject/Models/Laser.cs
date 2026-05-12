using CommunityToolkit.Mvvm.ComponentModel;

namespace CasinoProject.Models;

public partial class Laser : ObservableObject
{
    [ObservableProperty]
    private double _x;

    [ObservableProperty]
    private double _y;

    [ObservableProperty]
    private double _width = 4;

    [ObservableProperty]
    private double _height = 15;

    public Laser(double x, double y)
    {
        X = x;
        Y = y;
    }
}
