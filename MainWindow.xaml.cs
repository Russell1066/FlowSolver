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
        FlowBoard game = new FlowBoard();
        Solver ai;

        public MainWindow()
        {
            InitializeComponent();
            Gameboard.Initialize(game);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() => StartSolver());
        }

        private void StartSolver()
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            ai = new Solver(game);
            s.Stop();
            Trace.WriteLine($"took : {s.Elapsed}");
        }
    }
}
