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
using System.Windows.Threading;
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

        private void Board_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FlowBoard.Cells))
            {
                if (Board != null)
                {
                    Board.PropertyChanged -= Board_PropertyChanged;
                }

                Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(()=> Initialize(Board)));
            }
        }

        public void Initialize(FlowBoard board)
        {
            Board = board;
            Board.PropertyChanged += Board_PropertyChanged;
            Field.Columns = Board.Width;
            Field.Rows = Board.Height;
            Field.Children.Clear();

            for (int i = 0; i < Field.Columns * Field.Rows; ++i)
            {
                var cell = Board.Cells[i];
                var element = new CellView();
                element.SetCell(cell);

                element.MouseDown += (object sender, MouseButtonEventArgs e) =>
                {
                    cell.Press();
                };

                Field.Children.Add(element);
            }
        }
    }
}
