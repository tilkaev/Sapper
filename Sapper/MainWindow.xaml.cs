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

namespace Sapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        void Mining()
        {
            var maxMining = (int)((15 + (5 * (hard*10 - 1)))* (width_game_zone * height_game_zone) / 100);
            
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
                playing_field[x, y] = game_elements.Mine;
            }

            
        }

        void FillingField()
        {
            var actions = new List<(int, int)>
            {
                (-1, -1), (0, -1), (1, -1),
                
                (-1, 0), (1, 0),
                
                (-1, 1), (0, 1), (1, 1)
            };
            
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
                        if (playing_field[x,y] != game_elements.Mine)
                        {
                            playing_field[x, y] = (game_elements)((int)playing_field[x, y]+1);
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
            
            playing_field = new game_elements[width_game_zone, height_game_zone];
            for (int x = 0; x < width_game_zone; x++) // Заполнение Cell-ами
            {
                for (int y = 0; y < height_game_zone; y++)
                {
                    playing_field[x, y] = game_elements.Cell;
                }
            }

            Mining();

            FillingField();
            
            for (var x = 0; x < width_game_zone; x++) // output field
            {
                for (var y = 0; y < height_game_zone; y++)
                {
                    char tag;
                    if (playing_field[x, y] == game_elements.Mine)
                        tag = (char)game_elements.Mine;
                    else if (playing_field[x, y] != game_elements.Cell)
                        tag = char.Parse(((int)playing_field[x, y]).ToString());
                    else
                        tag = (char)game_elements.Cell;

                    var btn = new Button()
                    {
                        Height = PIXEL,
                        Width = PIXEL,
                        Background = Brushes.Gray,
                        Tag = tag,
                        Content = "",
                        Name = $"_{x*y}"
                    };
                    btn.Click += Button_OnClick;
                    btn.MouseRightButtonDown += Button_OnMouseRightButtonDown;
                    MainCanvas.Children.Add(btn);
                    Canvas.SetLeft(btn, x*PIXEL);
                    Canvas.SetTop(btn, y*PIXEL);

                    //textBox.Text += playing_field[x, y];
                }
                //textBox.Text += "\n" ;
            }


        }


        private double hard = 0.1;
        private double height_can;
        private double width_can;
        private const int PIXEL = 20;
        private int width_game_zone;
        private int height_game_zone;
        private game_elements[,] playing_field; 
        List<int> mines;
        enum game_elements
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

        public MainWindow()
        {
            InitializeComponent();
            height_can = MainCanvas.Height;
            width_can = MainCanvas.Width;
            width_game_zone = (int)(width_can / PIXEL);
            height_game_zone = (int)(height_can / PIXEL);
            create_field();
        }

        void ShowAllField()
        {
            foreach (Button item in MainCanvas.Children)
            {
                item.IsEnabled = false;
                item.Content = item.Tag;
            }
        }

        void ShowEmptyFields(int index)
        {
            
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            string tag = btn.Tag.ToString();
            string mine_char = char.ToString((char)game_elements.Mine);
            string cell_char = char.ToString((char)game_elements.Cell);
            if (tag == mine_char)
            {
                ShowAllField();
                MessageBox.Show("Game Over");
            }
            else if (tag == cell_char)
            {
                ShowEmptyFields(int.Parse(btn.Name.Substring(1)));
                btn.Content = btn.Tag;
                btn.IsEnabled = false;
            }
            else
            {
                btn.Content = btn.Tag;
                btn.IsEnabled = false;
            }
        }

        private void Button_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var btn = sender as Button;
            string content = btn.Content.ToString();
            btn.Content = content == "" ? "+" : "";
        }
    }
}