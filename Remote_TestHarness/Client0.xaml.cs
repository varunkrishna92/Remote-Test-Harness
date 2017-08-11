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

namespace Remote_TestHarness
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class Client0 : Window
    {
        public Client0()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = "Remote Test Harness Client 0";
            textBox1.Text = Properties.Settings.Default.StartText;
            this.Height = Properties.Settings.Default.WindowHt;
            this.Width = Properties.Settings.Default.WindowWd;
            button1.Width = 100;
            button1.Height = 30;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Properties.Settings.Default.StartText = textBox1.Text;
            Properties.Settings.Default.WindowHt = Height;
            Properties.Settings.Default.WindowWd = Width;
            Properties.Settings.Default.Save();
        }

        private void StartTest(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
