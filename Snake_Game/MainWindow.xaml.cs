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

namespace Snake_Game
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int CELL_WIDTH = 20;
        private const int CELL_HEIGHT = 20;
        private SolidColorBrush SnakeHeadColor = Brushes.Green;
        private SolidColorBrush SnakeBodyColor = Brushes.GreenYellow;
        private List<SnakePart> SnakeParts = new List<SnakePart>();
        private enum SnakeDirection { Left, Right, Up, Down};
        private SnakeDirection snakeDirection = SnakeDirection.Right;
        private int SnakeLength = 3;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            DrawGameArea();
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
                }
                GameArea.Children.Add(SnakePart.UiElement);
                Canvas.SetTop(SnakePart.UiElement, SnakePart.Position.Y);
                Canvas.SetLeft(SnakePart.UiElement, SnakePart.Position.X);
            }
        }
    }
}
