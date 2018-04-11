using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Windows.Threading;
namespace welding

  
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int start = 0;           // определяет включина или выключена программа (0 - стоп, 1- сторт, 2 реверс)
        int mode = 0;            // режим работы (0000- проверка связи, 0001 - режим "Ручной", 0010 - режим "По току сварки", 0011 - режим "По току проволоки", 0100 - режим "Циклограмма") 
        int auto_revers = 0;     // вкл., выкл. автоматический реверс 
        int id_mess = 1;         // идентификатор сообщения
        int contr_sum;           // контрольная сумма

        byte[] enter_mess = new byte[9];
        byte[] send_mess = new byte[9];
        string txt = "";
        string txt1 = "";
        int qqq;
       
        int error_connect_q = 0;


        int current_start = 0;    // ток начала подачи
        int current_stop = 0;     // ток конца подачи
        int revers = 0;           // реверс
        int start_on_current = 0; // старт по току пучка

        // ручной режим
        int feed_speed = 0;       // скорость подачи
        int feed_distance = 0;    // расстояние подачи
        int short_press = 0;      // краткое нажатие
        int long_press = 0;       // длительное нажатие 

        SerialPort s_port = new SerialPort();

        /*
        bool correct(string s)
        {
            
            if (s != "0" || "1")
            {
                return false;
            }
            else { return true; }
        }

    */
        void GetAvaileblePorts()
        {
            string ports_name;
            String[] ports = SerialPort.GetPortNames();

            if (String.IsNullOrWhiteSpace(string.Concat(ports)))
            {
                ports_name = "нет доступных COM-портов";
                com_ports_names.Items.Add(ports_name);
            }
            else
            {
                com_ports_names.ItemsSource = ports;
            }

        }
        
        void ConnectComPorts()
        {
            if (s_port.IsOpen == false)
            {
                try
                {
                    s_port.PortName = "COM2";
                    s_port.BaudRate = 921600;
                    s_port.DtrEnable = true;
                    s_port.Open();
                }
                catch (Exception)
                {
                    error_connect_q++;
                }
                open_port.IsEnabled = true;
                close_com_port.IsEnabled = false;
            }
            else
            {
                open_port.IsEnabled = false;
                close_com_port.IsEnabled = true;
            }
        }
        void Error_con()
        {
            ConnectComPorts();
            if (error_connect_q >= 5 && s_port.IsOpen == false)
            {
                error_text.Visibility = Visibility.Visible;
                error_text.Content = "Ошибка подключения к COM-порту. Выберите порт вручную";
                com_port_options.Visibility = Visibility.Visible;

            }
        }

        public MainWindow()
        {
            
            InitializeComponent();
            screen_1.Text = current_start.ToString();
            screen_2.Text = current_stop.ToString();
            screen_hand_mode_1.Text = feed_speed.ToString();
            screen_hand_mode_podachi.Text = feed_distance.ToString();
            GetAvaileblePorts();
            
        }

        private void Windows_loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer timer_connect = new DispatcherTimer();
            timer_connect.Interval = TimeSpan.FromSeconds(1);
            timer_connect.Tick += T_Tick;
            timer_connect.Start();
        }

        private void T_Tick(object sender, EventArgs e)
        {
           
            Error_con();
           

        }
        
        private void butteb_up_1_Click(object sender, RoutedEventArgs e)
        {
            if (current_start >= 999) { current_start = 998; }
            current_start++;
            screen_1.Text = current_start.ToString();
        }

        private void button_down_1_Click(object sender, RoutedEventArgs e)
        {
            if (current_start <= 0) { current_start = 1; }
            current_start--;
            screen_1.Text = current_start.ToString();
        }

        private void butteb_up_2_Click(object sender, RoutedEventArgs e)
        {
            if (current_stop >= 999) { current_stop = 998; }
            current_stop++;
            screen_2.Text = current_stop.ToString();
        }

        private void button_down_2_Click(object sender, RoutedEventArgs e)
        {
            if (current_stop <=0) { current_stop = 1; }
            current_stop--;
            screen_2.Text = current_stop.ToString();
            
        }

        

       

        private void screen_1_TextChanged(object sender, TextChangedEventArgs e)
        { 
            string s1 = screen_1.Text;
            screen_1.MaxLength = 3;
             if (s1.Count() < 1) { s1 = "0"; }
             current_start = Int32.Parse(s1);
         
        }
        

        private void screen_2_TextChanged(object sender, TextChangedEventArgs e)
        {
            string s2 = screen_2.Text;
            screen_2.MaxLength = 3;
            if (s2.Count() < 1) { s2 = "0"; }
            current_stop = Int32.Parse(s2);
        }
        

       
        

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
                
             ExitWindow exitWindow = new ExitWindow();
             exitWindow.Show();
            
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            InfoWindow  infoWindow = new InfoWindow();
            infoWindow.Show();
        }

        private void ciklogramma_Click(object sender, RoutedEventArgs e)
        {
            mode = 0100;
            hand_mode_grig.Visibility = Visibility.Hidden; border_hand_mode.Visibility = Visibility.Hidden; on_hand_mode.Visibility = Visibility.Hidden;
            on_cik.Visibility = Visibility.Visible; ciklogramma_grid.Visibility = Visibility.Visible; border_cik.Visibility = Visibility.Visible;
            


        }

        private void hand_mode_Click(object sender, RoutedEventArgs e)
        {
            mode = 0001;
            on_cik.Visibility = Visibility.Hidden; ciklogramma_grid.Visibility = Visibility.Hidden; border_cik.Visibility = Visibility.Hidden;
            hand_mode_grig.Visibility = Visibility.Visible; border_hand_mode.Visibility = Visibility.Visible; on_hand_mode.Visibility = Visibility.Visible;



        }

        private void start_button_1_Click(object sender, RoutedEventArgs e)
        {
            start_on_current++;
            if (start_on_current == 2) { start_on_current = 0; }
            if (start_on_current == 1) { on_start_2.Visibility = Visibility.Visible; screen_1.IsEnabled = true; screen_2.IsEnabled = true;}
            else if (start_on_current == 0) { on_start_2.Visibility = Visibility.Hidden; screen_1.IsEnabled = false; screen_2.IsEnabled = false;}

        }

        private void start_button_Click(object sender, RoutedEventArgs e)
        {
            start = 1;
            off_img.Visibility = Visibility.Hidden;
            on_img.Visibility = Visibility.Visible;
            
        }
        private void stop_button_Click(object sender, RoutedEventArgs e)
        {
            start = 0;
            on_img.Visibility = Visibility.Hidden;
            off_img.Visibility = Visibility.Visible;
           
        }
        private void butteb_up_speed_hand_mode_Click(object sender, RoutedEventArgs e)
        {
            if (feed_speed >= 999) { feed_speed = 998; }
            feed_speed++;
            screen_hand_mode_1.Text = feed_speed.ToString();

        }

        private void button_down_speed_hand_mode_Click(object sender, RoutedEventArgs e)
        {
            if (feed_speed <= 0) { feed_speed = 1; }
            feed_speed--;
            screen_hand_mode_1.Text = feed_speed.ToString();
        }


        private void screen_hand_mode_1_TextChanged(object sender, TextChangedEventArgs e)
        {
            string s4 = screen_hand_mode_1.Text;
            screen_hand_mode_1.MaxLength = 3;
            if (s4.Count() < 1) { s4 = "0"; }

            feed_speed = Int32.Parse(s4);

        }

        private void butteb_up_podachi_Click(object sender, RoutedEventArgs e)
        {
            if (feed_distance >= 9999) { feed_distance = 9998; }
            feed_distance++;
            screen_hand_mode_podachi.Text = feed_distance.ToString();

        }

        private void button_down_podachi_Click(object sender, RoutedEventArgs e)
        {
            if (feed_distance <= 0) { feed_distance = 1; }
            feed_distance--;
            screen_hand_mode_podachi.Text = feed_distance.ToString();
        }

        private void screen_hand_mode_podachi_TextChanged(object sender, TextChangedEventArgs e)
        {

            string s5 = screen_hand_mode_podachi.Text;
            screen_hand_mode_podachi.MaxLength = 4;
            if (s5.Count()<1) { s5 = "0"; }
            if (feed_distance == 0) { on_podachi.Visibility = Visibility.Hidden; screen_hand_mode_podachi.IsEnabled = false; }
            feed_distance = Int32.Parse(s5);
            if (feed_distance > 0) { on_podachi.Visibility = Visibility.Visible; screen_hand_mode_podachi.IsEnabled = true; }
            

        }
        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            
            LogiWindow logiWindow = new LogiWindow();
            logiWindow.Show();


        }

        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            com_port_options.Visibility = Visibility.Visible;

        }

        private void open_port_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (com_ports_names.Text == "")
                {
                    error_text.Visibility = Visibility.Visible;
                    error_text.Content = "Пожалуйста, выберете COM-port";

                }
                else
                {
                    s_port.PortName = com_ports_names.Text;
                    error_text.Visibility = Visibility.Hidden;
                    try
                    {
                        
                        s_port.BaudRate = 921600;
                        s_port.DtrEnable = true;
                        s_port.Open();
                    }
                    catch (Exception)
                    {
                        error_text.Content = "Отказано в доступе!";
                    }
                    if (s_port.IsOpen == true)
                    {
                        com_ports_names.IsEnabled = false;
                        progres_bar.Value = 100;
                        open_port.IsEnabled = false;
                        close_com_port.IsEnabled = true;
                        

                    }
                    else
                    {
                        error_text.Visibility = Visibility.Visible;
                        error_text.Content = "Отказано в доступе!";
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                error_text.Visibility = Visibility.Visible;
                error_text.Content = "Отказано в доступе!";
            }
            
        }
        

        private void close_com_port_Click(object sender, RoutedEventArgs e)
        { 
            s_port.Close();
            progres_bar.Value = 0;
            com_ports_names.IsEnabled = true;
            open_port.IsEnabled = true;
            close_com_port.IsEnabled = false;
        }

       

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            Array.Clear(send_mess, 0, 9);
            Array.Clear(enter_mess, 0, 9);
            txt = "";
            txt1 = "";

            int feed_speed_l = feed_speed;
            int feed_speed_h = feed_speed >> 8;
            int feed_distance_l = feed_distance;
            int feed_distance_h = feed_distance >> 8;
            

            
            send_mess[0] = (byte)id_mess;                   // id сообщения
            send_mess[1] = (byte)mode;                      // режим работы
            send_mess[2] = (byte)start;                     // команда
            send_mess[3] = (byte)auto_revers;               // автоматический реверс
            send_mess[4] = (byte)feed_speed_h;              // скорость подачи (старший байт)
            send_mess[5] = (byte)feed_speed_l;              // скорость подачи (младший байт)
            send_mess[6] = (byte)feed_distance_h;           // расстояние подачи (старший байт)
            send_mess[7] = (byte)feed_distance_l;           // расстояние подачи (младший байт)
            contr_sum = 0;
            for (var i = 0; i <= send_mess.Length -1 ; i++)
            {
                contr_sum = contr_sum ^ send_mess[i];
            }

            send_mess[8] = (byte)contr_sum;                 // контрольная сумма    
            try { s_port.Write(send_mess, 0, 9); }
            catch (Exception) {
                error_text.Visibility = Visibility.Visible;
                error_text.Content = "выберете COM-port";
            }
           

            try
            {
                System.Threading.Thread.Sleep(3);
                s_port.Read(enter_mess, 0, 3);
                
            }
            catch (Exception)
            {
                error_text.Visibility = Visibility.Visible;
                error_text.Content = "слишком долго";
            }
            
            for (var k = 0; k <= send_mess.Length - 1; k++)
            {
                qqq = Convert.ToInt32(send_mess[k]);
                txt1 += qqq.ToString() + " ";
            }
            send_text.Text = txt1;
            
            for (var j = 0; j <= enter_mess.Length - 1; j++)
            {

                qqq = Convert.ToInt32(enter_mess[j]);
                txt += qqq.ToString() + " ";
            }

            read_text.Text = txt;
            
        }
         


        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Array.Clear(send_mess, 0, 9);
            Array.Clear(enter_mess, 0, 9);
            


            try
            {
                
                s_port.DiscardInBuffer();
                s_port.DiscardOutBuffer();
                //read_text.Text = "";
                txt = "";
                txt1 = "";
                send_text.Clear();
                read_text.Clear();
            }
            catch(InvalidOperationException)
            {
                error_text.Visibility = Visibility.Visible;
                error_text.Content = "слишком долго";
            }

        }

        private void start_button_1_Copy_Click(object sender, RoutedEventArgs e)
        {
            auto_revers++;
            if (auto_revers == 2) { auto_revers = 0; }
            if (auto_revers == 1) { on_autu_rev.Visibility = Visibility.Visible; }
            else if (auto_revers == 0) { on_autu_rev.Visibility = Visibility.Hidden; }
        }

        private void close_com_option_Click(object sender, RoutedEventArgs e)
        {
            com_port_options.Visibility = Visibility.Hidden;
        }

        private void com_ports_names_DropDownOpened(object sender, EventArgs e)
        {
            GetAvaileblePorts();
        }
    }
}
