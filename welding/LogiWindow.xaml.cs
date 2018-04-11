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
using System.Windows.Shapes;
using System.IO.Ports;

namespace welding
{
    /// <summary>
    /// Логика взаимодействия для LogiWindow.xaml
    /// </summary>
    public partial class LogiWindow : Window
    {
        SerialPort sp = new SerialPort();
       

        public LogiWindow()
        {
            InitializeComponent();
            

        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            

        }

        private void add_port_Click(object sender, RoutedEventArgs e)
        {
            sp.PortName = "COM5";
            sp.BaudRate = 921600;
            sp.DtrEnable = true;
            sp.Open();
            if (sp.IsOpen == true) { MessageBox.Show("подключились", "подключились", MessageBoxButton.OK, MessageBoxImage.Warning); }
            else { MessageBox.Show("НЕподключились", "НЕподключились", MessageBoxButton.OK, MessageBoxImage.Warning); }

            
        }

        private void send_Click(object sender, RoutedEventArgs e)
        {
            string send = "1";
            sp.Write(send);
            
        }

        private void accept_Click(object sender, RoutedEventArgs e)
        {
            string a = sp.ReadExisting();
            MessageBox.Show(a,"То, что приняли", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
