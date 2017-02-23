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
        CancellationTokenSource TokenSource = new CancellationTokenSource();

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
                TokenSource = new CancellationTokenSource(90 * 60 * 1000);

                var timer = Task.Run(() =>
                {
                    while (!TokenSource.Token.IsCancellationRequested)
                    {
                        Thread.Sleep(1000);
                        string clockString = GetElapsedTime(s);
                        var ignore = Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new Action(() => { Clock.Text = clockString; }));
                    }
                });
                var task = await Solver.Solve(Game, TokenSource.Token);

                TokenSource.Cancel();
            }
            catch (OperationCanceledException)
            {
                Trace.WriteLine("Operation cancelled by user");
            }
            s.Stop();
            Trace.WriteLine($"took : {s.Elapsed}");
            Clock.Text = GetElapsedTime(s);
            UpdateButtons(false);
        }

        private static string GetElapsedTime(Stopwatch s)
        {
            var elapsed = new TimeSpan(0, 0, (int)s.Elapsed.TotalSeconds);
            return elapsed.ToString();
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

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (Editor.Visibility == Visibility.Hidden)
            {
                Editor.Visibility = Visibility.Visible;
                Edit.Content = "Edit (done)";
            }
            else
            {
                Editor.Visibility = Visibility.Hidden;
                Edit.Content = "Edit";
            }
        }
    }
}
