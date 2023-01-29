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

namespace Project_Phoenix_Game_Launcher
{
    /// <summary>
    /// HomeView.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            //WindowContentRendered?.Invoke();
        }
        private void OnWindowContentRendered(object sender, EventArgs e)
        {
            WindowContentRendered?.Invoke();
        }
        public static Action? WindowContentRendered;
        private void Window_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        private void QuitApp(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
