using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Documents.DocumentStructures;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sapper
{
    public partial class MainWindow : Window
    {
        private double Hard = 0.001;
        private double height_can;
        private double width_can;
        private const int PIXEL = 40;
        private int width_game_zone;
        private int height_game_zone;
        private GameElements[,] field; 
        private List<int> mines;

        private enum GameElements
        {
            Mine = '*',
            Cell = 0,
            One,
            Two,
            Three,
            Four,
            Five,
            Six,
            Seven,
            Eight
        }
        
        List<(int, int)> actions = new List<(int, int)>
        {
            (-1, -1), (0, -1), (1, -1),
                
            (-1, 0), (1, 0),
                
            (-1, 1), (0, 1), (1, 1)
        };
        public MainWindow()
        {
            InitializeComponent();
            height_can = MainCanvas.Height;
            width_can = MainCanvas.Width;
            width_game_zone = (int)(width_can / PIXEL);
            height_game_zone = (int)(height_can / PIXEL);
        }

        void start()
        {
            BtnStart.Visibility = Visibility.Collapsed;
            MainCanvas.Children.Clear();
            create_field();
        }

        
        private void Mining() // Минирование
        {
            var maxMining = (int)((15 + (5 * (Hard*10 - 1)))* (width_game_zone * height_game_zone) / 100);
            
            var r = new Random();
            mines = new List<int>();

            for (var i = 0; i < maxMining; i++) // Creating mines
            {
                var rand = r.Next(0, width_game_zone * height_game_zone);
                while (mines.Contains(rand))
                {
                    rand = r.Next(0, width_game_zone * height_game_zone);
                }
                mines.Add(rand);
                
                var x = Math.Abs(rand % width_game_zone);
                var y = Math.Abs(rand / width_game_zone);
                field[x, y] = GameElements.Mine;
            }

            
        }

        private void FillingField()
        {
            foreach (var mine in mines)
            {
                var xMine = Math.Abs(mine % width_game_zone);
                var yMine = Math.Abs(mine / width_game_zone);
                foreach (var action in actions)
                {
                    int x = xMine + action.Item1;
                    int y = yMine + action.Item2;
                    try
                    {
                        if (field[x,y] != GameElements.Mine)
                        {
                            field[x, y] = (GameElements)((int)field[x, y]+1);
                        }
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }
                }

                
            }
        }

        
        void create_field()
        {
            
            field = new GameElements[width_game_zone, height_game_zone];
            for (int x = 0; x < width_game_zone; x++) // Заполнение Cell-ами
            {
                for (int y = 0; y < height_game_zone; y++)
                {
                    field[x, y] = GameElements.Cell;
                }
            }

            Mining();

            FillingField();
            
            for (var x = 0; x < width_game_zone; x++) // draw field
            {
                for (var y = 0; y < height_game_zone; y++)
                {
                    // char content;
                    // if (field[x, y] == GameElements.Mine)
                    //     content = (char)GameElements.Mine;
                    // else if (field[x, y] != GameElements.Cell)
                    //     content = char.Parse(((int)field[x, y]).ToString());
                    // else
                    //     content = ' ';
                    var btn = new Button()
                    {
                        Height = PIXEL,
                        Width = PIXEL,
                        Background = Brushes.Gray,
                        Content = "",
                        Name = GetName(x, y)
                    };
                    btn.Click += Button_OnClick;
                    btn.MouseRightButtonDown += Button_OnMouseRightButtonDown;
                    MainCanvas.Children.Add(btn);
                    Canvas.SetLeft(btn, x*PIXEL);
                    Canvas.SetTop(btn, y*PIXEL);
                }
            }


        }

        
        void ShowAllField()
        {
            foreach (Button item in MainCanvas.Children)
            {
                item.Click -= Button_OnClick;
                item.MouseRightButtonDown -= Button_OnMouseRightButtonDown;
                var (x, y) = GetCoordinate(int.Parse(item.Name.Substring(1)));
                char content;
                if (field[x, y] == GameElements.Mine)
                    content = (char)GameElements.Mine;
                else if (field[x, y] != GameElements.Cell)
                    content = char.Parse(((int)field[x, y]).ToString());
                else
                    content = ' ';
                item.Content = content;
            }
        }
        
        

        string GetName(int x, int y)
        {
            string name;
            if (y == 0)
            {
                name = $"_{x}";
            }
            else
            {
                name = $"_{y*width_game_zone+x}";
            }
            return name;
        }

        private (int, int) GetCoordinate(int index)
        {
            int x = Math.Abs(index % width_game_zone);
            int y = Math.Abs(index / width_game_zone);
            return (x,y);
        }

        Button find_button(string name)
        {
            foreach (Button btn in MainCanvas.Children)
            {
                if (btn.Name == name)
                    return btn;
            }
            return null;
        }


        private bool chek_win = true;
        void CheckWin()
        {
            foreach (Button btn in MainCanvas.Children)
            {
                var (x, y) = GetCoordinate(int.Parse(btn.Name.Substring(1)));
                if (btn.IsEnabled == true & field[x,y] == GameElements.Cell)
                {
                    return;
                }
            }
            if (chek_win)
            {
                chek_win = false;
                ShowAllField();
                BtnStart.Visibility = Visibility.Visible;
                MessageBox.Show("You are win!");
            }
            
        }
        
        async void Open(int index)
        {
            var (x, y) = GetCoordinate(index);
            
            // проверка на выход за границы:
            if (x < 0 || y < 0 || x >= width_game_zone || y >= height_game_zone) 
                return;
            
            // textBox.Text += $"{x} {y}";
            var btn = find_button($"_{index}");
            
            // предотвращение бесконечной рекурсии: если клетка открыта, ничего не делаем
            if (btn.IsEnabled == false) return;
            
            if (field[x,y] == GameElements.Mine) // Mine
            {
                ShowAllField();
                btn.Background = Brushes.Brown;
                BtnStart.Visibility = Visibility.Visible;
                MessageBox.Show("Game Over");
            }
            else if (field[x,y] != GameElements.Cell) // Fields with numbers
            {
                btn.IsEnabled = false;
                btn.Content = char.Parse(((int)field[x, y]).ToString());
            }
            else // Empty Fields
            {
                btn.IsEnabled = false;
                
                foreach (var action in actions)
                {
                    await Task.Delay(50);
                    int x2 = x + action.Item1;
                    int y2 = y + action.Item2;
                    
                    // проверка на выход за границы:
                    if (x2 < 0 || y2 < 0 || x2 >= width_game_zone || y2 >= height_game_zone) 
                         continue;
                    
                    Open(x2, y2);
                }

                CheckWin();
                // Open(x-1, y-1); Open(x, y-1); Open(x+1, y-1);
                // Open(x-1, y  );               Open(x+1, y  );
                // Open(x-1, y+1); Open(x, y+1); Open(x+1, y+1);
            }
        }

        void Open(int x, int y)
        {
            Open(int.Parse(GetName(x,y).Substring(1)));
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn.Content == "+")
                return;
            Open(int.Parse(btn.Name.Substring(1)));
        }
        private void Button_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var btn = sender as Button;
            var content = btn.Content.ToString();
            btn.Content = content == "" ? "+" : "";
        }

        private void BtnStart_OnClick(object sender, RoutedEventArgs e)
        {
            start();
        }
    }
}