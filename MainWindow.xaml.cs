using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FlowBoard Game = new FlowBoard();

        public MainWindow()
        {
            InitializeComponent();
            Gameboard.Initialize(Game);
            Editor.Game = Game;
            Editor.Viewer = Gameboard;
        }

        private void StartSolver()
        {
            Trace.WriteLine($"");
            Stopwatch s = new Stopwatch();
            s.Start();
            bool solved = Solver.Solve(Game);
            s.Stop();
            Trace.WriteLine($"took : {s.Elapsed}");
            Dispatcher.InvokeAsync(new Action(() => EnableButtons(true)));
        }

        private void Solve_Click(object sender, RoutedEventArgs e)
        {
            EnableButtons(false);
            Task.Run(() => StartSolver());
        }

        private void EnableButtons(bool isEnabled)
        {
            Solve.IsEnabled = isEnabled;
            Editor.Visibility = isEnabled ? Visibility.Visible : Visibility.Hidden;
        }

    }
}
