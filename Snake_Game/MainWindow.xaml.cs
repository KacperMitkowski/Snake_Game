using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Snake_Game
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int CELL_WIDTH = 20;
        private const int CELL_HEIGHT = 20;
        private int SnakeSpeed = 400;
        private int SnakeSpeedThreshold = 100;
        private int SnakeLength = 3;
        private int CurrentScore = 0;
        private Random rnd = new Random();
        private enum SnakeDirection { Left, Right, Up, Down };
        private SnakeDirection snakeDirection = SnakeDirection.Right;
        private SolidColorBrush SnakeHeadColor = Brushes.Green;
        private SolidColorBrush SnakeBodyColor = Brushes.GreenYellow;
        private SolidColorBrush FoodColor = Brushes.Red;
        private UIElement SnakeFood = null;
        private List<SnakePart> SnakeParts = new List<SnakePart>();
        private DispatcherTimer GameTickTimer = new DispatcherTimer();
    
        public MainWindow()
        {
            InitializeComponent();
            GameTickTimer.Tick += GameLoop_Step;
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            DrawGameArea();
            StartGame();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            SnakeDirection currentDirection = snakeDirection;
            switch (e.Key)
            {
                case Key.Up:
                    if (snakeDirection != SnakeDirection.Down)
                    {
                        snakeDirection = SnakeDirection.Up;
                    }
                    break;
                case Key.Down:
                    if (snakeDirection != SnakeDirection.Up)
                    {
                        snakeDirection = SnakeDirection.Down;
                    }
                    break;
                case Key.Left:
                    if (snakeDirection != SnakeDirection.Right)
                    {
                        snakeDirection = SnakeDirection.Left;
                    }
                    break;
                case Key.Right:
                    if (snakeDirection != SnakeDirection.Left)
                    {
                        snakeDirection = SnakeDirection.Right;
                    }
                    break;
                case Key.Space:
                    StartGame();
                    break;
            }
            if (snakeDirection != currentDirection)
            {
                MoveSnake();
            }
        }

        private void StartGame()
        {
            SnakeParts.Add(new SnakePart() { Position = new Point(0, 0) });
            GameTickTimer.Interval = TimeSpan.FromMilliseconds(SnakeSpeed);
            DrawSnake();
            DrawSnakeFood();
            GameTickTimer.IsEnabled = true;
        }

        private void GameLoop_Step(object sender, EventArgs e)
        {
            MoveSnake();
        }

        private void DrawGameArea()
        {
            int nextX = 0, nextY = 0, rowCounter = 1;
            bool xIsOdd = true;
            bool gameBoardIsDrawing = true;

            while(gameBoardIsDrawing)
            {
                Rectangle rect = new Rectangle()
                {
                    Width = CELL_WIDTH,
                    Height = CELL_HEIGHT,
                    Fill = xIsOdd ? Brushes.White : Brushes.Black
                };
                GameArea.Children.Add(rect);
                Canvas.SetTop(rect, nextY);
                Canvas.SetLeft(rect, nextX);
                xIsOdd = !xIsOdd;
                nextX += CELL_WIDTH;

                if(nextX >= GameArea.ActualWidth)
                {
                    nextX = 0;
                    nextY += CELL_HEIGHT;
                    rowCounter++;
                    xIsOdd = (rowCounter % 2 != 0);
                }

                if(nextY >= GameArea.ActualHeight)
                {
                    gameBoardIsDrawing = false;
                }
            }
        }

        private void DrawSnake()
        {
            foreach (var SnakePart in SnakeParts)
            {
                if(SnakePart.UiElement == null)
                {
                    SnakePart.UiElement = new Rectangle()
                    {
                        Width = CELL_WIDTH,
                        Height = CELL_HEIGHT,
                        Fill = SnakePart.IsHead ? SnakeHeadColor : SnakeBodyColor
                    };
                    GameArea.Children.Add(SnakePart.UiElement);
                    Canvas.SetTop(SnakePart.UiElement, SnakePart.Position.Y);
                    Canvas.SetLeft(SnakePart.UiElement, SnakePart.Position.X);
                }
            }
        }

        private void MoveSnake()
        {
            while (SnakeParts.Count >= SnakeLength)
            {
                GameArea.Children.Remove(SnakeParts[0].UiElement);
                SnakeParts.RemoveAt(0);
            }
            foreach (var SnakePart in SnakeParts)
            {
                (SnakePart.UiElement as Rectangle).Fill = SnakeBodyColor;
                SnakePart.IsHead = false;
            }

            SnakePart SnakeHead = SnakeParts[SnakeParts.Count - 1];
            double nextX = SnakeHead.Position.X;
            double nextY = SnakeHead.Position.Y;
            
            switch (snakeDirection)
            {
                case SnakeDirection.Left:
                    nextX -= CELL_WIDTH;
                    break;
                case SnakeDirection.Right:
                    nextX += CELL_WIDTH;
                    break;
                case SnakeDirection.Up:
                    nextY -= CELL_HEIGHT;
                    break;
                case SnakeDirection.Down:
                    nextY += CELL_HEIGHT;
                    break;
            }
            SnakeParts.Add(new SnakePart()
            {
                Position = new Point(nextX, nextY),
                IsHead = true
            });
            DrawSnake();
            CheckCollisions();
        }

        private void CheckCollisions()
        {
            SnakePart SnakeHead = SnakeParts[SnakeParts.Count - 1];

            if ((SnakeHead.Position.X == Canvas.GetLeft(SnakeFood)) && (SnakeHead.Position.Y == Canvas.GetTop(SnakeFood)))
            {
                EatSnakeFood();
                return;
            }

            if ((SnakeHead.Position.Y < 0) || (SnakeHead.Position.Y >= GameArea.ActualHeight) ||
            (SnakeHead.Position.X < 0) || (SnakeHead.Position.X >= GameArea.ActualWidth))
            {
                EndGame();
            }

            foreach (SnakePart SnakeBodyPart in SnakeParts.Take(SnakeParts.Count - 1))
            {
                if ((SnakeHead.Position.X == SnakeBodyPart.Position.X) && (SnakeHead.Position.Y == SnakeBodyPart.Position.Y))
                {
                    EndGame();
                }
            }
        }
        private void EndGame()
        {
            GameTickTimer.IsEnabled = false;
            MessageBox.Show("Oooops, you died!\n\nTo start a new game, just press the Space bar...", "SnakeWPF");
        }

        private Point GetFoodPosition()
        {
            int maxX = (int)(GameArea.ActualWidth / CELL_WIDTH);
            int maxY = (int)(GameArea.ActualHeight / CELL_HEIGHT);
            int foodX = rnd.Next(0, maxX) * CELL_WIDTH;
            int foodY = rnd.Next(0, maxY) * CELL_HEIGHT;

            foreach (var snakePart in SnakeParts)
            {
                if ((snakePart.Position.X == foodX) && (snakePart.Position.Y == foodY))
                    return GetFoodPosition();
            }

            return new Point(foodX, foodY);
        }

        private void DrawSnakeFood()
        {
            Point foodPosition = GetFoodPosition();
            SnakeFood = new Ellipse()
            {
                Width = CELL_WIDTH,
                Height = CELL_HEIGHT,
                Fill = FoodColor
            };
            GameArea.Children.Add(SnakeFood);
            Canvas.SetTop(SnakeFood, foodPosition.Y);
            Canvas.SetLeft(SnakeFood, foodPosition.X);
        }

        private void EatSnakeFood()
        {
            SnakeLength++;
            CurrentScore++;
            int timerInterval = Math.Max(SnakeSpeedThreshold, (int)GameTickTimer.Interval.TotalMilliseconds - (CurrentScore * 2));
            GameTickTimer.Interval = TimeSpan.FromMilliseconds(timerInterval);
            GameArea.Children.Remove(SnakeFood);
            DrawSnakeFood();
            UpdateGameStatus();
        }

        private void UpdateGameStatus()
        {
            this.Title = "SnakeWPF - Score: " + CurrentScore + " - Game speed: " + GameTickTimer.Interval.TotalMilliseconds;
        }
    }
}
