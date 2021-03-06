﻿using System;
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
using System.Windows.Threading;
using SolverCore;

namespace FlowSolver
{
    /// <summary>
    /// Interaction logic for CellView.xaml
    /// </summary>
    public partial class CellView : UserControl, INotifyPropertyChanged
    {
        public Cell BackingCell { get; private set; }
        private Rectangle ConnectParent;
        private Rectangle ConnectChild;
        public event PropertyChangedEventHandler PropertyChanged;
        private DateTime nextUpdate = DateTime.MinValue;
        private const int PREUPDATEDELAY = 10;
        private const int PREUPDATEOFFSET = 1;

        public CellView()
        {
            InitializeComponent();
        }

        public void SetCell(Cell cell)
        {
            BackingCell = cell;
            UpdateEndpoint(cell);
            UpdateCell(cell.Parent, cell.Child);

            cell.PropertyChanged += Cell_PropertyChanged;
        }

        private void UpdateEndpoint(Cell cell)
        {
            Brush fillColor = GetBrushColor(cell.CellColor);
            Square.Fill = Brushes.DarkGray;
            Square.Margin = new Thickness(2);
            if (cell.IsEndpoint)
            {
                Circle.Fill = fillColor;
                Circle.Visibility = Visibility.Visible;
            }
            else
            {
                Circle.Visibility = Visibility.Hidden;
            }
        }

        private void Cell_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var cell = sender as Cell;
            if (e.PropertyName == nameof(Cell.IsEndpoint))
            {
                UpdateEndpoint(cell);
                return;
            }

            // These happen a lot - don't let them stack up
            // at the end, the fact that 100ms will have passed 
            if (DateTime.Now > nextUpdate)
            {
                var delay = Task.Delay(PREUPDATEDELAY);
                nextUpdate = DateTime.Now.AddMilliseconds(PREUPDATEDELAY - PREUPDATEOFFSET);
                var updateCell = new Action(() => UpdateCell(BackingCell.Parent, BackingCell.Child));
                var updateCellTask = new Action<Task>((t) => Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, updateCell));
                delay.ContinueWith(updateCellTask);
            }
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

        public static SolidColorBrush GetBrushColor(Cell.Color sourceColor)
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

                case Cell.Color.Brown:
                    return Brushes.Brown;

                case Cell.Color.DarkBlue:
                    return Brushes.DarkBlue;

                case Cell.Color.Gold:
                    return Brushes.Gold;

                case Cell.Color.DarkCyan:
                    return Brushes.DarkCyan;

                case Cell.Color.Silver:
                    return Brushes.Silver;

                case Cell.Color.White:
                    return Brushes.White;

                case Cell.Color.DarkGreen:
                    return Brushes.DarkGreen;

                case Cell.Color.DarkPurple:
                    return Brushes.DarkMagenta;

                case Cell.Color.LimeGreen:
                    return Brushes.LimeGreen;

                case Cell.Color.DontUse:
                    return Brushes.Black;

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
