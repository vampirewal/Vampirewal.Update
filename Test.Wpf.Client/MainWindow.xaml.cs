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
using Vampirewal.Core.Components;
using Vampirewal.Core.Interface;
using Vampirewal.Update.Client.Extension;

namespace Test.Wpf.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IDialogMessage dialog = new VampirewalDialog();
            dialog.OpenUpdateWindow("XX更新程序","127.0.0.1","9999","1.0.0.1","aaaa");
        }
    }
}
