﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using System.Xml.Serialization;
using System.Diagnostics;
using System.Collections;

namespace Snake_Game
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int CELL_WIDTH = 20;
        private const int CELL_HEIGHT = 20;
        private const int MAX_HIGHSCORE_LIST_ENTRY_COUNT = 5;
        private const string FILE_WITH_HIGHSCORE_LIST = "snake_highscorelist.xml";
        private Hashtable options = new Hashtable();
        private Random rnd = new Random();
        private int CurrentSnakeLength;
        private int CurrentScore = 0;
        private SnakeDirection snakeDirection;
        private UIElement SnakeFood;
        private List<SnakePart> SnakeParts = new List<SnakePart>();
        private DispatcherTimer GameTickTimer = new DispatcherTimer();
        public ObservableCollection<SnakeHighscore> HighscoreList { get; set; } = new ObservableCollection<SnakeHighscore>();
        public MainWindow()
        {
            InitializeComponent();
            GameTickTimer.Tick += GameLoop_Step;
            LoadHighscoreList();
            InitializeOptions();
        }

        private void InitializeOptions()
        {
            options.Add("SnakeSpeed", 400.0);
            options.Add("SnakeSpeedThreshold", 100);
            options.Add("SnakeInitialLength", 3);
            options.Add("SnakeHeadColor", Brushes.Green);
            options.Add("SnakeBodyColor", Brushes.GreenYellow);
            SnakeFood = new Ellipse() { Width = CELL_WIDTH, Height = CELL_HEIGHT, Fill = Brushes.Red };
            snakeDirection = SnakeDirection.Right;
        }

        private void StartGame()
        {
            bdrHighscoreList.Visibility = Visibility.Collapsed;
            bdrEndOfGame.Visibility = Visibility.Collapsed;

            foreach (SnakePart snakeBodyPart in SnakeParts)
            {
                if (snakeBodyPart.UiElement != null)
                {
                    GameArea.Children.Remove(snakeBodyPart.UiElement);
                }
            }
            SnakeParts.Clear();
            if (SnakeFood != null)
            {
                GameArea.Children.Remove(SnakeFood);
            }

            CurrentScore = 0;
            CurrentSnakeLength = (int)options["SnakeInitialLength"];
            snakeDirection = SnakeDirection.Right;
            SnakeParts.Add(new SnakePart() { Position = new Point(0, 0) });
            GameTickTimer.Interval = TimeSpan.FromMilliseconds((double)options["SnakeSpeed"]);

            DrawSnake();
            DrawSnakeFood();
            UpdateGameStatus();

            GameTickTimer.IsEnabled = true;
        }

        private void RestartGame()
        {
            Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
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
                        Fill = SnakePart.IsHead ? (SolidColorBrush)options["SnakeHeadColor"] : (SolidColorBrush)options["SnakeBodyColor"]
                    };
                    GameArea.Children.Add(SnakePart.UiElement);
                    Canvas.SetTop(SnakePart.UiElement, SnakePart.Position.Y);
                    Canvas.SetLeft(SnakePart.UiElement, SnakePart.Position.X);
                }
            }
        }

        private void MoveSnake()
        {
            while (SnakeParts.Count >= CurrentSnakeLength)
            {
                GameArea.Children.Remove(SnakeParts[0].UiElement);
                SnakeParts.RemoveAt(0);
            }
            foreach (var SnakePart in SnakeParts)
            {
                (SnakePart.UiElement as Rectangle).Fill = (SolidColorBrush)options["SnakeBodyColor"];
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

            if ((SnakeHead.Position.Y < 0) || (SnakeHead.Position.Y >= GameArea.ActualHeight) || (SnakeHead.Position.X < 0) || (SnakeHead.Position.X >= GameArea.ActualWidth))
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
            bool isNewHighscore = false;
            if (CurrentScore > 0)
            {
                int lowestHighscore = (this.HighscoreList.Count > 0 ? this.HighscoreList.Min(x => x.Score) : 0);
                if ((CurrentScore > lowestHighscore) || (this.HighscoreList.Count < MAX_HIGHSCORE_LIST_ENTRY_COUNT))
                {
                    bdrNewHighscore.Visibility = Visibility.Visible;
                    txtPlayerName.Focus();
                    isNewHighscore = true;
                }
            }
            if (!isNewHighscore)
            {
                tbFinalScore.Text = CurrentScore.ToString();
                bdrEndOfGame.Visibility = Visibility.Visible;
            }
            GameTickTimer.IsEnabled = false;
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
            GameArea.Children.Add(SnakeFood);
            Canvas.SetTop(SnakeFood, foodPosition.Y);
            Canvas.SetLeft(SnakeFood, foodPosition.X);
        }

        private void EatSnakeFood()
        {
            CurrentSnakeLength++;
            CurrentScore++;
            int howMuchSpeedIncreases = 30;
            int timerInterval = Math.Max((int)options["SnakeSpeedThreshold"], (int)GameTickTimer.Interval.TotalMilliseconds - (howMuchSpeedIncreases));
            GameTickTimer.Interval = TimeSpan.FromMilliseconds(timerInterval);
            GameArea.Children.Remove(SnakeFood);
            DrawSnakeFood();
            UpdateGameStatus();
        }

        private void UpdateGameStatus()
        {
            this.ScoreTextBlock.Text = CurrentScore.ToString();
            this.SpeedTextBlock.Text = GameTickTimer.Interval.TotalSeconds.ToString();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            DrawGameArea();
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
                    bdrWelcomeMessage.Visibility = Visibility.Collapsed;
                    StartGame();
                    break;
                case Key.Escape:
                    this.Close();
                    break;
                case Key.N:
                    RestartGame();
                    break;
            }
            if (snakeDirection != currentDirection)
            {
                MoveSnake();
            }
        }

        

        private void GameLoop_Step(object sender, EventArgs e)
        {
            MoveSnake();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnAddToHighscoreList_Click(object sender, RoutedEventArgs e)
        {
            int newIndex = 0;
            if ((this.HighscoreList.Count > 0) && (CurrentScore < this.HighscoreList.Max(x => x.Score)))
            {
                SnakeHighscore justAbove = this.HighscoreList.OrderByDescending(x => x.Score).First(x => x.Score >= CurrentScore);
                if (justAbove != null)
                {
                    newIndex = this.HighscoreList.IndexOf(justAbove) + 1;
                }
            }
            this.HighscoreList.Insert(newIndex, new SnakeHighscore()
            {
                PlayerName = txtPlayerName.Text,
                Score = CurrentScore
            });
            while (this.HighscoreList.Count > MAX_HIGHSCORE_LIST_ENTRY_COUNT)
            {
                this.HighscoreList.RemoveAt(MAX_HIGHSCORE_LIST_ENTRY_COUNT);
            }

            SaveHighscoreList();
            bdrNewHighscore.Visibility = Visibility.Collapsed;
            bdrHighscoreList.Visibility = Visibility.Visible;
        }
        private void BtnShowHighscoreList_Click(object sender, RoutedEventArgs e)
        {
            bdrWelcomeMessage.Visibility = Visibility.Collapsed;
            bdrHighscoreList.Visibility = Visibility.Visible;
        }

        private void LoadHighscoreList()
        {
            if (File.Exists(FILE_WITH_HIGHSCORE_LIST))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<SnakeHighscore>));
                using (Stream reader = new FileStream(FILE_WITH_HIGHSCORE_LIST, FileMode.Open))
                {
                    List<SnakeHighscore> tempList = (List<SnakeHighscore>)serializer.Deserialize(reader);
                    this.HighscoreList.Clear();
                    foreach (var item in tempList.OrderByDescending(x => x.Score))
                    {
                        this.HighscoreList.Add(item);
                    }
                }
            }
        }
        private void SaveHighscoreList()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<SnakeHighscore>));
            using (Stream writer = new FileStream(FILE_WITH_HIGHSCORE_LIST, FileMode.Create))
            {
                serializer.Serialize(writer, this.HighscoreList);
            }
        }
    }
}
