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
using SolverCore;

namespace FlowSolver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FlowBoard Game = new FlowBoard();
        CancellationTokenSource TokenSource;

        public MainWindow()
        {
            InitializeComponent();
            Gameboard.Initialize(Game);
            Editor.Game = Game;
        }

        private async void StartSolver()
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            try
            {
                TokenSource = new CancellationTokenSource();
                TokenSource.CancelAfter(2 * 60 * 1000);
                var task = await Solver.Solve(Game, TokenSource.Token);
            }
            catch (OperationCanceledException cancelled)
            {
                Trace.WriteLine(cancelled.ToString());
            }
            s.Stop();
            Trace.WriteLine($"took : {s.Elapsed}");
            UpdateButtons(false);
        }

        private void Solve_Click(object sender, RoutedEventArgs e)
        {
            UpdateButtons(true);
            StartSolver();
        }

        private void UpdateButtons(bool isBusy)
        {
            var visible = isBusy ? Visibility.Hidden : Visibility.Visible;
            var inVisible = !isBusy ? Visibility.Hidden : Visibility.Visible;

            Solve.Visibility = visible;
            Stop.Visibility = inVisible;
            Editor.Visibility = visible;
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            TokenSource.Cancel();
        }
    }
}
