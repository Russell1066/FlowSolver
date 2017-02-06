using Microsoft.Win32;
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

namespace FlowSolver
{
    /// <summary>
    /// Interaction logic for EditorPanel.xaml
    /// </summary>
    public partial class EditorPanel : UserControl
    {
        public FlowBoard Game { get; set; }
        private const string Extension = ".flow";
        private const string Description = "Flow Files";
        Random Rand = new Random();
        private int ActivePoint { get; set; } = -1;
        private FlowBoard.Endpoints ActiveEndpoint { get; set; }
        public Playfield Viewer { get; internal set; }

        public EditorPanel()
        {
            InitializeComponent();
            IEnumerable<Cell.Color> colors = GetButtonColors();

            foreach (var color in colors)
            {
                var button = new Button()
                {
                    Background = CellView.GetBrushColor(color),
                    Margin = new Thickness(2),

                };

                button.Click += (object sender, RoutedEventArgs e) =>
                {
                    Button_Click(color);
                };

                ColorPicker.Children.Add(button);
            }
        }

        private static IEnumerable<Cell.Color> GetButtonColors()
        {
            return from color in Enum.GetNames(typeof(Cell.Color))
                   where color != Cell.Color.DontUse.ToString() &&
                   color != Cell.Color.Empty.ToString()
                   select (Cell.Color)Enum.Parse(typeof(Cell.Color), color);
        }

        private void Button_Click(Cell.Color color)
        {
            ActivePoint = 1;
            ActiveEndpoint = AddIfMissing(color);
            foreach (var cell in Game.Cells)
            {
                if (cell.CellColor != Cell.Color.Empty && cell.CellColor != color)
                {
                    continue;
                }

                cell.PropertyChanged += Cell_PropertyChanged;
            }
        }

        private void Cell_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var cell = sender as Cell;
            if (e.PropertyName != "Pressed")
            {
                return;
            }

            if (ActivePoint == 1)
            {
                if((cell.Index == Game.PointToIndex(ActiveEndpoint.Pt2)))
                {
                    ActiveEndpoint.Pt2 = ActiveEndpoint.Pt1;
                }
                else
                {
                    Game.Cells[Game.PointToIndex(ActiveEndpoint.Pt1)].ClearEndpoint();
                    Game.Cells[Game.PointToIndex(ActiveEndpoint.Pt1)].PropertyChanged += Cell_PropertyChanged;
                    ActiveEndpoint.Pt1 = Game.IndexToPoint(cell.Index);
                    Game.Cells[Game.PointToIndex(ActiveEndpoint.Pt1)].MakeEndpoint(ActiveEndpoint.FlowColor);
                }
                ++ActivePoint;
            }
            // No, both endpoints can't be in the same place
            else if (cell.Index != Game.PointToIndex(ActiveEndpoint.Pt1))
            {
                Game.Cells[Game.PointToIndex(ActiveEndpoint.Pt2)].ClearEndpoint();
                ActiveEndpoint.Pt2 = Game.IndexToPoint(cell.Index);
                Game.Cells[Game.PointToIndex(ActiveEndpoint.Pt2)].MakeEndpoint(ActiveEndpoint.FlowColor);
                foreach (var c in Game.Cells)
                {
                    c.PropertyChanged -= Cell_PropertyChanged;
                }

                Game.Reset();
            }
        }

        private FlowBoard.Endpoints AddIfMissing(Cell.Color color)
        {
            var existing = GetEndpoint(color);

            if (existing != null)
            {
                return existing;
            }

            var emptyCells = (from cell in Game.Cells
                              where cell.CellColor == Cell.Color.Empty
                              select cell).ToList();

            int index1 = Rand.Next(emptyCells.Count());
            int index2 = Rand.Next(emptyCells.Count());
            while (index2 == index1)
            {
                index2 = Rand.Next(emptyCells.Count());
            }

            var endpoints = new FlowBoard.Endpoints()
            {
                FlowColor = color,
                Pt1 = Game.IndexToPoint(emptyCells[index1].Index),
                Pt2 = Game.IndexToPoint(emptyCells[index2].Index)
            };

            Game.AddEndpoints(endpoints);

            return endpoints;
        }

        private FlowBoard.Endpoints GetEndpoint(Cell.Color color)
        {
            return (from endpoint in Game.Puzzle
                    where endpoint.FlowColor == color
                    select endpoint).FirstOrDefault();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Game?.Reset();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Game?.Clear();
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            if (Game == null)
            {
                return;
            }

            var newBoard = FileUtilities.LoadFromJson<FlowBoard.BoardDefinition>(Description, Extension);
            if (newBoard != null)
            {
                Game.InitializeBoard(newBoard);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (Game == null)
            {
                return;
            }

            FileUtilities.SaveAsJson<FlowBoard.BoardDefinition>(Game.Board, Description, Extension);
        }
    }
}
