using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for CellView.xaml
    /// </summary>
    public partial class CellView : UserControl, INotifyPropertyChanged
    {
        private Cell BackingCell;
        private Rectangle ConnectParent;
        private Rectangle ConnectChild;
        public event PropertyChangedEventHandler PropertyChanged;

        public CellView()
        {
            InitializeComponent();
        }

        public void SetCell(Cell cell)
        {
            BackingCell = cell;
            Brush fillColor = GetBrushColor(cell.CellColor);
            if (cell.IsEndpoint)
            {
                Square.Fill = Brushes.DarkGray;
                Square.Margin = new Thickness(2);
                Circle.Fill = fillColor;
            }
            else
            {
                Square.Fill = fillColor;
                Square.Margin = new Thickness(2);
            }

            cell.PropertyChanged += Cell_PropertyChanged;
        }

        private void Cell_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle, new Action(
                () => UpdateCell(BackingCell.Parent, BackingCell.Child)));
        }

        private void UpdateCell(Cell parent, Cell child)
        {
            // Clear the old one
            if (ConnectParent != null)
            {
                ConnectParent.Visibility = Visibility.Hidden;
                ConnectParent = null;
            }

            if (ConnectChild != null)
            {
                ConnectChild.Visibility = Visibility.Hidden;
                ConnectChild = null;
            }

            // Show the new one
            if (parent != null)
            {
                ConnectParent = GetConnect(BackingCell.Index - parent.Index);
            }

            if (child != null)
            {
                ConnectChild = GetConnect(BackingCell.Index - child.Index);
            }
        }

        public Thickness UpThickness => new Thickness(0, 0, 0, ActualHeight / 2);
        public Thickness DownThickness => new Thickness(0, ActualHeight / 2, 0, 0);
        public Thickness LeftThickness => new Thickness(0, 0, ActualWidth / 2, 0);
        public Thickness RightThickness => new Thickness(ActualWidth / 2, 0, 0, 0);

        private Rectangle GetConnect(int diff)
        {
            Rectangle connection = null;

            // four possibilities
            if (diff == 1) // that means this cell is greater - so point to the left
            {
                connection = ConnectLeft;
            }
            else if (diff == -1) // that means this cell is greater - so point to the left
            {
                connection = ConnectRight;
            }
            else if (diff > 0)
            {
                connection = ConnectUp;
            }
            else
            {
                connection = ConnectDown;
            }

            connection.Fill = GetBrushColor();
            connection.Visibility = Visibility.Visible;

            return connection;
        }

        private SolidColorBrush GetBrushColor()
        {
            return GetBrushColor(BackingCell.CellColor);
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

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            OnPropertyChanged(nameof(UpThickness));
            OnPropertyChanged(nameof(DownThickness));
            OnPropertyChanged(nameof(LeftThickness));
            OnPropertyChanged(nameof(RightThickness));
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
