using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CasinoProject.Core;
using CasinoProject.Models;

namespace CasinoProject.ViewModels;

public partial class ArkanoidViewModel : ObservableObject, IDisposable
{
    // Game area dimensions
    private const double CanvasWidth = 800;
    private const double CanvasHeight = 600;

    [ObservableProperty]
    private double _paddleX = 350;
    private const double PaddleY = 550;
    [ObservableProperty]
    private double _paddleWidth = 100;
    private const double DefaultPaddleWidth = 100;
    private const double PaddleHeight = 20;
    private const double PaddleSpeed = 30;







    [ObservableProperty]
    private ObservableCollection<Block> _blocks = new();
    [ObservableProperty]
    private ObservableCollection<Ball> _balls = new();

    [ObservableProperty]
    private ObservableCollection<Bonus> _bonuses = new();

    [ObservableProperty]
    private ObservableCollection<Laser> _lasers = new();

    [ObservableProperty]
    private bool _hasLasers;

    [ObservableProperty]
    private bool _isSticky;

    private Random _random = new Random();


    [ObservableProperty]
    private int _betAmount = 100;

    [ObservableProperty]
    private int _currentWinnings = 0;

    [ObservableProperty]
    private bool _isGameRunning = false;

    [ObservableProperty]
    private string _gameStatusMessage = "Place a bet to play!";

    [ObservableProperty]
    private bool _showGameStatus = true;

    public int SessionBalance => SessionManager.Instance.Balance;

    private DispatcherTimer _gameTimer;

    private void OnSessionManagerPropertyChanged(object? s, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SessionManager.Instance.Balance))
        {
            OnPropertyChanged(nameof(SessionBalance));
        }
    }

    public ArkanoidViewModel()
    {
        _gameTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        _gameTimer.Tick += GameLoop;

        SessionManager.Instance.PropertyChanged += OnSessionManagerPropertyChanged;
    }

    public void Dispose()
    {
        SessionManager.Instance.PropertyChanged -= OnSessionManagerPropertyChanged;
        _gameTimer?.Stop();
    }

    [RelayCommand]
    private void StartGame()
    {
        if (IsGameRunning) return;

        if (BetAmount <= 0)
        {
            GameStatusMessage = "Bet must be greater than 0!";
            ShowGameStatus = true;
            return;
        }

        if (SessionManager.Instance.TryDeduct(BetAmount))
        {
            InitializeLevel();
            IsGameRunning = true;
            ShowGameStatus = false;
            CurrentWinnings = 0;
            _gameTimer.Start();
        }
        else
        {
            GameStatusMessage = "Not enough funds!";
            ShowGameStatus = true;
        }
    }

    private void InitializeLevel()
    {
        PaddleWidth = DefaultPaddleWidth;
        HasLasers = false;
        IsSticky = false;
        PaddleX = (CanvasWidth - PaddleWidth) / 2;

        Balls.Clear();
        Balls.Add(new Ball((CanvasWidth - 20) / 2, PaddleY - 20 - 5, 5, -5));

        Bonuses.Clear();
        Lasers.Clear();

        Blocks.Clear();

        // Create some blocks
        int rows = 5;
        int cols = 10;
        double blockWidth = 70;
        double blockHeight = 30;
        double padding = 10;
        double startX = (CanvasWidth - (cols * blockWidth + (cols - 1) * padding)) / 2;
        double startY = 50;

        string[] colors = { "#ff007f", "#11998e", "#f5c518", "#38ef7d", "#3a1c71" };

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Blocks.Add(new Block(
                    startX + col * (blockWidth + padding),
                    startY + row * (blockHeight + padding),
                    blockWidth,
                    blockHeight,
                    Avalonia.Media.Brush.Parse(colors[row % colors.Length])
                ));
            }
        }
    }

        private void GameLoop(object? sender, EventArgs e)
    {
        // Update Lasers
        for (int i = Lasers.Count - 1; i >= 0; i--)
        {
            var laser = Lasers[i];
            laser.Y -= 10; // Laser speed
            if (laser.Y < 0)
            {
                Lasers.RemoveAt(i);
                continue;
            }

            // Laser block collision
            bool hit = false;
            foreach (var block in Blocks.Where(b => b.IsVisible))
            {
                if (laser.X + laser.Width >= block.X && laser.X <= block.X + block.Width &&
                    laser.Y + laser.Height >= block.Y && laser.Y <= block.Y + block.Height)
                {
                    block.IsVisible = false;
                    CurrentWinnings += (int)(BetAmount * 0.1);
                    SpawnBonus(block.X + block.Width / 2, block.Y + block.Height / 2);
                    hit = true;
                    break;
                }
            }
            if (hit)
            {
                Lasers.RemoveAt(i);
            }
        }

        // Update Bonuses
        for (int i = Bonuses.Count - 1; i >= 0; i--)
        {
            var bonus = Bonuses[i];
            bonus.Y += 3; // Bonus fall speed

            // Paddle collision
            if (bonus.Y + bonus.Size >= PaddleY && bonus.Y <= PaddleY + PaddleHeight &&
                bonus.X + bonus.Size >= PaddleX && bonus.X <= PaddleX + PaddleWidth)
            {
                ApplyBonus(bonus.Type);
                Bonuses.RemoveAt(i);
                continue;
            }

            // Missed bonus
            if (bonus.Y > CanvasHeight)
            {
                Bonuses.RemoveAt(i);
            }
        }

        // Update Balls
        for (int i = Balls.Count - 1; i >= 0; i--)
        {
            var ball = Balls[i];

            if (ball.IsStuck) continue;

            ball.X += ball.VelocityX;
            ball.Y += ball.VelocityY;

            // Wall collisions
            if (ball.X <= 0 || ball.X + ball.Size >= CanvasWidth)
            {
                ball.VelocityX *= -1;
                ball.X = Math.Max(0, Math.Min(ball.X, CanvasWidth - ball.Size)); // Keep in bounds
            }
            if (ball.Y <= 0)
            {
                ball.VelocityY *= -1;
                ball.Y = 0;
            }

            // Paddle collision
            if (ball.VelocityY > 0 &&
                ball.Y + ball.Size >= PaddleY && ball.Y + ball.Size <= PaddleY + PaddleHeight + Math.Abs(ball.VelocityY) &&
                ball.X + ball.Size >= PaddleX && ball.X <= PaddleX + PaddleWidth)
            {
                if (IsSticky)
                {
                    ball.IsStuck = true;
                    ball.StuckOffsetX = ball.X - (PaddleX + PaddleWidth / 2);
                    ball.Y = PaddleY - ball.Size;
                }
                else
                {
                    ball.VelocityY *= -1;
                    double hitPoint = (ball.X + ball.Size / 2) - (PaddleX + PaddleWidth / 2);
                    ball.VelocityX = hitPoint * 0.15;
                    ball.Y = PaddleY - ball.Size;
                }
            }

            // Block collisions
            foreach (var block in Blocks.Where(b => b.IsVisible))
            {
                if (ball.X + ball.Size >= block.X && ball.X <= block.X + block.Width &&
                    ball.Y + ball.Size >= block.Y && ball.Y <= block.Y + block.Height)
                {
                    block.IsVisible = false;
                    CurrentWinnings += (int)(BetAmount * 0.1);
                    SpawnBonus(block.X + block.Width / 2, block.Y + block.Height / 2);
                    ball.VelocityY *= -1;
                    break;
                }
            }

            // Fall below screen
            if (ball.Y > CanvasHeight)
            {
                Balls.RemoveAt(i);
            }
        }

        // Check lose condition
        if (Balls.Count == 0)
        {
            EndGame(false);
            return;
        }

        // Check win condition
        if (!Blocks.Any(b => b.IsVisible))
        {
            EndGame(true);
        }
    }

    private void SpawnBonus(double x, double y)
    {
        if (_random.NextDouble() < 0.10) // 10% chance
        {
            Array values = Enum.GetValues(typeof(BonusType));
            BonusType randomType = (BonusType)values.GetValue(_random.Next(values.Length))!;
            Bonuses.Add(new Bonus(x, y, randomType));
        }
    }

    private void ApplyBonus(BonusType type)
    {
        switch (type)
        {
            case BonusType.Expand:
                PaddleWidth = Math.Min(200, PaddleWidth + 40);
                break;
            case BonusType.Slow:
                foreach (var b in Balls)
                {
                    b.VelocityX *= 0.7;
                    b.VelocityY *= 0.7;
                }
                break;
            case BonusType.MultiBall:
                if (Balls.Count > 0)
                {
                    var sourceBall = Balls[0];
                    Balls.Add(new Ball(sourceBall.X, sourceBall.Y, sourceBall.VelocityX - 2, sourceBall.VelocityY - 1));
                    Balls.Add(new Ball(sourceBall.X, sourceBall.Y, sourceBall.VelocityX + 2, sourceBall.VelocityY - 1));
                }
                else
                {
                    Balls.Add(new Ball(PaddleX + PaddleWidth/2, PaddleY - 25, -3, -5));
                    Balls.Add(new Ball(PaddleX + PaddleWidth/2, PaddleY - 25, 3, -5));
                }
                break;
            case BonusType.Cash:
                CurrentWinnings += BetAmount; // Give 100% of bet as instant bonus
                break;
            case BonusType.Sticky:
                IsSticky = true;
                break;
            case BonusType.Lasers:
                HasLasers = true;
                break;
        }
    }

    private void EndGame(bool isWin)
    {
        _gameTimer.Stop();
        IsGameRunning = false;

        SessionManager.Instance.AddWinnings(CurrentWinnings);

        if (isWin)
        {
            GameStatusMessage = $"You Won! Earned: {CurrentWinnings}$";
        }
        else
        {
            GameStatusMessage = $"Game Over! Earned: {CurrentWinnings}$";
        }

        ShowGameStatus = true;
    }

    public void MovePaddleLeft()
    {
        if (IsGameRunning)
        {
            PaddleX = Math.Max(0, PaddleX - PaddleSpeed);
            UpdateStuckBalls();
        }
    }

    public void MovePaddleRight()
    {
        if (IsGameRunning)
        {
            PaddleX = Math.Min(CanvasWidth - PaddleWidth, PaddleX + PaddleSpeed);
            UpdateStuckBalls();
        }
    }

    private void UpdateStuckBalls()
    {
        foreach (var ball in Balls.Where(b => b.IsStuck))
        {
            ball.X = PaddleX + PaddleWidth / 2 + ball.StuckOffsetX;
        }
    }

    public void SpaceAction()
    {
        if (!IsGameRunning) return;

        bool unstuckAny = false;
        foreach (var ball in Balls.Where(b => b.IsStuck))
        {
            ball.IsStuck = false;
            unstuckAny = true;
            // Launch slightly outwards depending on offset
            ball.VelocityX = ball.StuckOffsetX * 0.15;
            ball.VelocityY = -5; // Base launch speed
        }

        if (!unstuckAny && HasLasers)
        {
            // Shoot lasers from edges of paddle
            Lasers.Add(new Laser(PaddleX, PaddleY));
            Lasers.Add(new Laser(PaddleX + PaddleWidth - 4, PaddleY));
        }
    }

    [RelayCommand]
    private void ReturnToMenu()
    {
        if (IsGameRunning)
        {
            _gameTimer.Stop();
            // Optional: If you leave during game, give current winnings?
            // Requirements didn't specify leaving early, let's just add winnings
            SessionManager.Instance.AddWinnings(CurrentWinnings);
        }
        WeakReferenceMessenger.Default.Send(new NavigationMessage("Menu"));
    }
}
