using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using static FlowSolver.Cell;

namespace FlowSolver
{
    /// <summary>
    /// Interaction logic for Playfield.xaml
    /// </summary>
    public partial class Playfield : UserControl
    {
        private FlowBoard Board;

        public Playfield()
        {
            InitializeComponent();
        }

        public void Initialize(FlowBoard board)
        {
            Board = board;
            Field.Columns = Board.Width;
            Field.Rows = Board.Height;
            Field.Children.Clear();

            for (int i = 0; i < Field.Columns * Field.Rows; ++i)
            {
                var element = new Rectangle()
                {
                    Margin = new Thickness(2)
                };
                var brush = Brushes.DarkGray;
                brush = GetBrushColor(Board.Cells[i].CellColor);
                element.Fill = brush;
                Board.Cells[i].PropertyChanged += Cell_PropertyChanged;

                Field.Children.Add(element);
            }
        }

        private void Cell_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateCell((sender as Cell).Index);
        }

        public void UpdateCell(int index)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                Debug.Assert(index >= 0 && index < Field.Children.Count);
                (Field.Children[index] as Rectangle).Fill = GetBrushColor(Board.Cells[index].CellColor);
            }               
            ));

        }

        private static SolidColorBrush GetBrushColor(Cell.Color sourceColor)
        {
            switch (sourceColor)
            {
                case Cell.Color.Blue:
                    return Brushes.Blue;

                case Cell.Color.Cyan:
                    return Brushes.Cyan;

                case Cell.Color.Green:
                    return Brushes.Green;

                case Cell.Color.Magenta:
                    return Brushes.Magenta;

                case Cell.Color.Orange:
                    return Brushes.Orange;

                case Cell.Color.Red:
                    return Brushes.Red;

                case Cell.Color.Yellow:
                    return Brushes.Yellow;

                case Cell.Color.DontUse:
                    return Brushes.White;

                default:
                    return Brushes.DarkGray;
            }
        }
    }
}
