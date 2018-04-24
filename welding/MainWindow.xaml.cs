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
using System.Threading;
using System.Globalization;
namespace welding

  
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        SerialPort s_port = new SerialPort();
       
        DispatcherTimer timer_connect = new DispatcherTimer();
        DispatcherTimer timer_flow = new DispatcherTimer();
        DispatcherTimer indicator_timer = new DispatcherTimer();
        
       


        int start;           // определяет включина или выключена программа (0 - стоп, 1- сторт, 2 реверс)
        int mode;            // режим работы (0000- проверка связи, 0001 - режим "Ручной", 0010 - режим "По току сварки", 0011 - режим "По току проволоки", 0100 - режим "Циклограмма") 
        int auto_revers;     // вкл., выкл. автоматический реверс 
        int id_mess = 1;         // идентификатор сообщения
        int contr_sum;           // контрольная сумма

        byte[] enter_mess = new byte[14];
        byte[] send_mess = new byte[15];
        string txt = "";
        string txt1 = "";
        int qqq;
        int error_connect_q = 0;
        int check_conn = 0;

        // индикаторы
        int current_welding_ind = 0;     // ток сварки
        int wire_speed_ind = 0;          // скорость подачи проволки
        int current__wire_ind = 0;       // ток через проволку
        int bias_voltage_ind = 0;        // напряжение смещения
        int distance_one_ind = 0;        // длина подачи за один цикл
        int distance_all_ind = 0;        // вся длина подачи


        int current_start;    // ток начала подачи
        int current_stop;     // ток конца подачи
        
        int revers_distance;  // растояние реверса
        int start_on_current; // старт по току пучка

        int bias_voltage;     // напряжение сещения

        // ручной режим
        int feed_speed;       // скорость подачи
        float feed_speed_d = Properties.Settings.Default.feed_speed_d; // промежуточная переменная для преобразование в значение с точкой
        int feed_distance = Properties.Settings.Default.feed_distance;    // расстояние подачи
        

        

        

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
                        s_port.PortName = Properties.Settings.Default.COM_port;
                        s_port.BaudRate = 921600;
                        s_port.DtrEnable = true;
                        s_port.Open();
                    }
                    catch (Exception)
                    {
                        error_connect_q++;
                        if (error_connect_q >= 5)
                        {

                            timer_connect.Stop();
                        }
                    }
                    open_port.IsEnabled = true;
                    close_com_port.IsEnabled = false;
                }
           else
                {
                check_conn++;
                if (check_conn == 3)
                {
                    Check_connect();
                }
                if (check_conn > 4)
                {
                    check_conn = 4;
                }
                open_port.IsEnabled = false;
                    close_com_port.IsEnabled = true;

                }
            
        }
        void Ok_connect()
        {
            if (s_port.IsOpen == true)
            {
                
                com_ports_names.IsEnabled = false;
                progres_bar.Value = 100;
                com_ports_names.Text = Properties.Settings.Default.COM_port;
            }
            else if (s_port.IsOpen == false)
            {
                progres_bar.Value = 0;
                com_ports_names.IsEnabled = true;
                open_port.IsEnabled = true;
                close_com_port.IsEnabled = false;

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
        void Check_connect()
        {
            s_port.DiscardInBuffer();
            s_port.DiscardOutBuffer();
            Array.Clear(send_mess, 0, 15);
            Array.Clear(enter_mess, 0, 14);
            send_mess[0] = 1;
            send_mess[1] = 0;
            send_mess[2] = 0;
            send_mess[3] = 0;
            send_mess[4] = 0;
            send_mess[5] = 0;
            send_mess[6] = 0;
            send_mess[7] = 0;
            send_mess[8] = 0;
            send_mess[9] = 0;
            send_mess[10] = 0;
            send_mess[11] = 0;
            send_mess[12] = 0;
            send_mess[13] = 0;
            contr_sum = 0;
            for (var i = 0; i <= send_mess.Length - 1; i++)
            {
                contr_sum = contr_sum ^ send_mess[i];
            }

            send_mess[14] = (byte)contr_sum;                 // контрольная сумма   
            s_port.Write(send_mess, 0, 15);
            /*
            try
            {
                System.Threading.Thread.Sleep(3);
                s_port.Read(enter_mess, 0, 3);

            }
            catch (InvalidOperationException)
            {
                error_text.Visibility = Visibility.Visible;
                error_text.Content = "порт отключен";
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
            */

        }
        void Send_function()
        {
            if (s_port.IsOpen == true)
            {
                s_port.DiscardInBuffer();
                s_port.DiscardOutBuffer();
                Array.Clear(send_mess, 0, 15);
                Array.Clear(enter_mess, 0, 14);
                txt = "";
                txt1 = "";
                float feed_speed_d_1 = 0;
                feed_speed_d_1 = feed_speed_d * 10;
                feed_speed = (int)feed_speed_d_1;
                int feed_speed_l = feed_speed;
                int feed_speed_h = feed_speed >> 8;
                int feed_distance_l = feed_distance;
                int feed_distance_h = feed_distance >> 8;
                int bias_voltage_h = bias_voltage >> 8;
                int bias_voltage_l = bias_voltage;





                send_mess[0] =  (byte)id_mess;                  // id сообщения
                send_mess[1] =  (byte)mode;                     // режим работы (0000- проверка связи, 0001 - режим "Ручной", 0010 - режим "По току сварки", 0011 - режим "По току проволоки", 0100 - режим "Циклограмма") 
                send_mess[2] =  (byte)start;                    // команда включина или выключена программа (0 - стоп, 1- сторт, 2 реверс)
                send_mess[3] =  (byte)auto_revers;              // автоматический реверс
                send_mess[4] =  (byte)feed_speed_h;             // скорость подачи (старший байт)
                send_mess[5] =  (byte)feed_speed_l;             // скорость подачи (младший байт)
                send_mess[6] =  (byte)feed_distance_h;          // расстояние подачи (старший байт)
                send_mess[7] =  (byte)feed_distance_l;          // расстояние подачи (младший байт)
                send_mess[8] =  (byte)revers_distance;          // расстояние реверса 
                send_mess[9] =  (byte)start_on_current;         // вкл., выкл. старт по току пучка
                send_mess[10] = (byte)current_start;            // ток начала подачи
                send_mess[11] = (byte)current_stop;             // ток конца подачи
                send_mess[12] = (byte)bias_voltage_h;           // напряжение смещения (старший байт)
                send_mess[13] = (byte)bias_voltage_l;           // напряжение смещения (младший байт)
                contr_sum = 0;
                for (var i = 0; i <= send_mess.Length - 1; i++)
                {
                    contr_sum = contr_sum ^ send_mess[i];
                }

                send_mess[14] = (byte)contr_sum;                 // контрольная сумма    
                try { s_port.Write(send_mess, 0, 15); }

                catch (Exception)
                {
                    error_text.Visibility = Visibility.Visible;
                    error_text.Content = "выберете COM-port";
                }


                try
                {
                    System.Threading.Thread.Sleep(3);
                    s_port.Read(enter_mess, 0, 14);

                }
                catch (InvalidOperationException)
                {
                    error_text.Visibility = Visibility.Visible;
                    error_text.Content = "порт отключен";
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

        }
         void Read_fanction()
        {
            if (s_port.IsOpen == true)
            {
                /*
                 enter_mess[0]  ток сварки (старший байт) 
                 enter_mess[1]  ток сварки (младший байт)
                 enter_mess[2]  длина подачи за цикл (старший байт)
                 enter_mess[3]  длина подачи за цикл (младший байт)
                 enter_mess[4]  скорость подачи (старший байт)
                 enter_mess[5]  скорость продачи (младший байт)
                 enter_mess[6]  ток пучка (старший байт)
                 enter_mess[7]  ток пучка (младший байт)
                 enter_mess[8]  ток через проволки (старший байт)
                 enter_mess[9]  ток через проволки (младший байт)
                 enter_mess[10] напряжение смещения (старший байт) 
                 enter_mess[11] напряжение смещения (младший байт)
                 enter_mess[12] работа/стоп
                 enter_mess[13] контрольная сумма

                int current_welding_ind = 0;     // ток сварки
                int wire_speed_ind      = 0;     // скорость подачи проволки
                int current__wire_ind   = 0;     // ток через проволку
                int bias_voltage_ind    = 0;     // напряжение смещения
                int distance_one_ind    = 0;     // длина подачи за один цикл
                int distance_all_ind    = 0;     // вся длина подачи
                 */

                try
                {
                    System.Threading.Thread.Sleep(3);
                    s_port.Read(enter_mess, 0, 14);
                    int control_sum_enter_mess = 0;

                    for (var i = 0; i <= enter_mess.Length - 1; i++)
                    {
                        control_sum_enter_mess = control_sum_enter_mess ^ send_mess[i];
                    }

                    if (control_sum_enter_mess == (int)enter_mess[13])
                    {
                        current_welding_ind = ((int)enter_mess[0] << 8) | enter_mess[1];
                        distance_one_ind = ((int)enter_mess[2] << 8) | enter_mess[3];
                        wire_speed_ind = ((int)enter_mess[4] << 8) | enter_mess[5];
                        current__wire_ind = ((int)enter_mess[8] << 8) | enter_mess[9];
                        bias_voltage_ind = ((int)enter_mess[10] << 8) | enter_mess[11];
                        start = enter_mess[12];
                    }
                }
                catch(Exception)
                {

                }
            }
        }
       

        public MainWindow()
        {
            
            InitializeComponent();
            
            screen_1.Text = Properties.Settings.Default.current_start.ToString();
            screen_2.Text = Properties.Settings.Default.current_stop.ToString();
            screen_volt.Text = Properties.Settings.Default.bias_voltage.ToString();
            screen_hand_mode_1.Text = feed_speed_d.ToString("F1");
            screen_hand_mode_podachi.Text = feed_distance.ToString();
            screen_rev.Text = Properties.Settings.Default.revers_distance.ToString();
            welding_cureent_screen.Text = current_welding_ind.ToString();
            speed_of_filing.Text = wire_speed_ind.ToString();
            current_though_wire.Text = current__wire_ind.ToString();
            bias_voltage_crreen.Text = bias_voltage_ind.ToString();
            screen_one_cikl.Text = distance_one_ind.ToString();
            screen_all_time.Text = distance_all_ind.ToString();
            GetAvaileblePorts();
            timer_connect.Interval = TimeSpan.FromSeconds(1);
            timer_connect.Tick += T_Tick;
            timer_connect.Start();
            Thread myThread = new Thread(Reed_line_function);
            myThread.Start();
            // проверка связи



        }
        void Reed_line_function()
        {


            while (true)
            {
                Thread.Sleep(5);
                if (s_port.IsOpen == true)
                {

                    /*
                     enter_mess[0]  ток сварки (старший байт) 
                     enter_mess[1]  ток сварки (младший байт)
                     enter_mess[2]  длина подачи за цикл (старший байт)
                     enter_mess[3]  длина подачи за цикл (младший байт)
                     enter_mess[4]  скорость подачи (старший байт)
                     enter_mess[5]  скорость продачи (младший байт)
                     enter_mess[6]  ток пучка (старший байт)
                     enter_mess[7]  ток пучка (младший байт)
                     enter_mess[8]  ток через проволки (старший байт)
                     enter_mess[9]  ток через проволки (младший байт)
                     enter_mess[10] напряжение смещения (старший байт) 
                     enter_mess[11] напряжение смещения (младший байт)
                     enter_mess[12] работа/стоп
                     enter_mess[13] контрольная сумма

                    int current_welding_ind = 0;     // ток сварки
                    int wire_speed_ind      = 0;     // скорость подачи проволки
                    int current__wire_ind   = 0;     // ток через проволку
                    int bias_voltage_ind    = 0;     // напряжение смещения
                    int distance_one_ind    = 0;     // длина подачи за один цикл
                    int distance_all_ind    = 0;     // вся длина подачи
                     */

                    try
                    {
                        
                        s_port.Read(enter_mess, 0, 14);
                        int control_sum_enter_mess = 0;

                        for (var i = 0; i <= enter_mess.Length - 1; i++)
                        {
                            control_sum_enter_mess = control_sum_enter_mess ^ send_mess[i];
                        }

                        if (control_sum_enter_mess == (int)enter_mess[13])
                        {
                            current_welding_ind = ((int)enter_mess[0] << 8) | enter_mess[1];
                            distance_one_ind = ((int)enter_mess[2] << 8) | enter_mess[3];
                            wire_speed_ind = ((int)enter_mess[4] << 8) | enter_mess[5];
                            current__wire_ind = ((int)enter_mess[8] << 8) | enter_mess[9];
                            bias_voltage_ind = ((int)enter_mess[10] << 8) | enter_mess[11];
                            start = enter_mess[12];
                        }
                    }
                    catch (Exception)
                    {
                        
                    }
                }
                /*else
                {
                    current_welding_ind++;
                    wire_speed_ind++;
                    current__wire_ind++;
                    bias_voltage_ind++;
                    distance_one_ind++;
                    distance_all_ind++;
                }
                */
            }
        }

        private void Main_window_load(object sender, RoutedEventArgs e)
        {
            indicator_timer.Interval = TimeSpan.FromMilliseconds(100);
            indicator_timer.Tick += indicator_tick;
            indicator_timer.Start();
            
            start = 0;

            // пуск по току сварки
            start_on_current = Properties.Settings.Default.start_on_current;
            if (start_on_current > 1) { start_on_current = 0; }
            if (start_on_current == 1) { on_start_2.Visibility = Visibility.Visible; screen_1.IsEnabled = true; screen_2.IsEnabled = true; }
            else if (start_on_current == 0) { on_start_2.Visibility = Visibility.Hidden; screen_1.IsEnabled = false; screen_2.IsEnabled = false; }

            
            current_start = Properties.Settings.Default.current_start;
            current_stop = Properties.Settings.Default.current_stop;
            bias_voltage = Properties.Settings.Default.bias_voltage;
            mode = Properties.Settings.Default.mode;
            auto_revers = Properties.Settings.Default.auto_revers;
            revers_distance = Properties.Settings.Default.revers_distance;

            // инициализация видимости режимов
            switch (mode)
            {
                case 1:
                    on_cik.Visibility = Visibility.Hidden; ciklogramma_grid.Visibility = Visibility.Hidden; border_cik.Visibility = Visibility.Hidden;
                    hand_mode_grig.Visibility = Visibility.Visible; border_hand_mode.Visibility = Visibility.Visible; on_hand_mode.Visibility = Visibility.Visible;
                    break;
                case 2:
                    hand_mode_grig.Visibility = Visibility.Hidden; border_hand_mode.Visibility = Visibility.Hidden; on_hand_mode.Visibility = Visibility.Hidden;
                    on_cik.Visibility = Visibility.Visible; ciklogramma_grid.Visibility = Visibility.Visible; border_cik.Visibility = Visibility.Visible;
                    break;
            }
            switch (auto_revers)
            {
                case 0:
                    on_autu_rev.Visibility = Visibility.Hidden;
                    break;
                case 1:
                    on_autu_rev.Visibility = Visibility.Visible;
                    break;
                
            }
            // ручной режим
            if (feed_distance == 0) { on_podachi.Visibility = Visibility.Hidden; screen_hand_mode_podachi.IsEnabled = false; }
            if (feed_distance > 0) { on_podachi.Visibility = Visibility.Visible; screen_hand_mode_podachi.IsEnabled = true; }


        }

        private void indicator_tick(object sender, EventArgs e)
        {
            welding_cureent_screen.Text = current_welding_ind.ToString();
            speed_of_filing.Text = wire_speed_ind.ToString();
            current_though_wire.Text = current__wire_ind.ToString();
            bias_voltage_crreen.Text = bias_voltage_ind.ToString();
            screen_one_cikl.Text = distance_one_ind.ToString();
            screen_all_time.Text = distance_all_ind.ToString();

        }

        private void T_Tick(object sender, EventArgs e)
        {
            
            Error_con();
        }
        
        private void butteb_up_1_Click(object sender, RoutedEventArgs e)
        {
            current_start++;
            if (current_start >= 999) { current_start = 999; }
            screen_1.Text = current_start.ToString();
            if (start_on_current == 1) { Send_function(); }
        }

        private void button_down_1_Click(object sender, RoutedEventArgs e)
        {
            current_start--;
            if (current_start <= 0) { current_start = 0; }
            screen_1.Text = current_start.ToString();
            if (start_on_current == 1) { Send_function(); }
        }

        private void butteb_up_2_Click(object sender, RoutedEventArgs e)
        {
            current_stop++;
            if (current_stop >= 999) { current_stop = 999; }
            screen_2.Text = current_stop.ToString();
            if (start_on_current == 1) { Send_function(); }
        }

        private void button_down_2_Click(object sender, RoutedEventArgs e)
        {
            current_stop--;
            if (current_stop <= 0) { current_stop = 0; }
            screen_2.Text = current_stop.ToString();
            if (start_on_current == 1) { Send_function(); }

        }

        

       

        private void screen_1_TextChanged(object sender, TextChangedEventArgs e)
        {
           
           
            string s1 = screen_1.Text;
            screen_1.MaxLength = 3;
             if (s1.Count() < 1) { s1 = "0"; }
             current_start = Int32.Parse(s1);
            Properties.Settings.Default.current_start = current_start;




        }
        

        private void screen_2_TextChanged(object sender, TextChangedEventArgs e)
        {
            string s2 = screen_2.Text;
            screen_2.MaxLength = 3;
            if (s2.Count() < 1) { s2 = "0"; }
            current_stop = Int32.Parse(s2);
            Properties.Settings.Default.current_stop = current_stop;
        }
        

       
        

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.feed_speed_d = feed_speed_d;
            Properties.Settings.Default.mode = mode;
            Properties.Settings.Default.Save();
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
            mode = 2;
            if (mode == 2)
            {
                hand_mode_grig.Visibility = Visibility.Hidden; border_hand_mode.Visibility = Visibility.Hidden; on_hand_mode.Visibility = Visibility.Hidden;
                on_cik.Visibility = Visibility.Visible; ciklogramma_grid.Visibility = Visibility.Visible; border_cik.Visibility = Visibility.Visible;
            }


        }

        private void hand_mode_Click(object sender, RoutedEventArgs e)
        {
            mode = 1;
            if (mode == 1)
            {
                on_cik.Visibility = Visibility.Hidden; ciklogramma_grid.Visibility = Visibility.Hidden; border_cik.Visibility = Visibility.Hidden;
                hand_mode_grig.Visibility = Visibility.Visible; border_hand_mode.Visibility = Visibility.Visible; on_hand_mode.Visibility = Visibility.Visible;
            }



        }

        private void start_button_1_Click(object sender, RoutedEventArgs e)
        {
            start_on_current++;
            if (start_on_current > 1) { start_on_current = 0; }
            if (start_on_current == 1) { on_start_2.Visibility = Visibility.Visible; screen_1.IsEnabled = true; screen_2.IsEnabled = true;}
            else if (start_on_current == 0) { on_start_2.Visibility = Visibility.Hidden; screen_1.IsEnabled = false; screen_2.IsEnabled = false; }
            Properties.Settings.Default.start_on_current = start_on_current;
            Send_function();

            

        }

        private void start_button_Click(object sender, RoutedEventArgs e)
        {

            start = 1;
            revers_butt.IsEnabled = false;
            off_img.Visibility = Visibility.Hidden;
            on_img.Visibility = Visibility.Visible;
            Send_function();
        }
        private void stop_button_Click(object sender, RoutedEventArgs e)
        {
            start = 0;
            revers_butt.IsEnabled = true;
            on_img.Visibility = Visibility.Hidden;
            off_img.Visibility = Visibility.Visible;
            Send_function();
        }
        
        private void butteb_up_speed_hand_mode_Click(object sender, RoutedEventArgs e)
        {
            
            feed_speed_d = feed_speed_d + (float)0.1;
            if (feed_speed_d >= (float)16) { feed_speed_d = (float)16; }
            screen_hand_mode_1.Text = screen_hand_mode_1.Text.Replace(".", ",");
            screen_hand_mode_1.Text = feed_speed_d.ToString("F1");
            Properties.Settings.Default.feed_speed_d = feed_speed_d;
            Send_function();
            

        }

        private void button_down_speed_hand_mode_Click(object sender, RoutedEventArgs e)
        {
            
            feed_speed_d = feed_speed_d - (float)0.1; ;
            if (feed_speed_d <= (float)0) { feed_speed_d = (float)0; }
            screen_hand_mode_1.Text = screen_hand_mode_1.Text.Replace(".", ",");
            screen_hand_mode_1.Text = feed_speed_d.ToString("F1");
            Properties.Settings.Default.feed_speed_d = feed_speed_d;
            Send_function();
            
        }


        private void screen_hand_mode_1_TextChanged(object sender, TextChangedEventArgs e)
        {
            string s4 = screen_hand_mode_1.Text;
            s4 = s4.Replace(".", ",");
            screen_hand_mode_1.MaxLength = 4;
            if (s4.Count() < 1) { s4 = "0"; }
            try
            {
                feed_speed_d = float.Parse(s4);
            }
            catch(Exception)
            { }
            
          
            if (feed_speed_d > (float)16) { feed_speed_d = (float)15.9; }
            Properties.Settings.Default.feed_speed_d = feed_speed_d;
            if (start == 1 && feed_speed_d!= 0) { Send_function(); }
            
        }

        private void butteb_up_podachi_Click(object sender, RoutedEventArgs e)
        {
            feed_distance++;
            if (feed_distance >= 9999) { feed_distance = 9999; }
            screen_hand_mode_podachi.Text = feed_distance.ToString();
            Properties.Settings.Default.feed_distance = feed_distance;
            Send_function();
        }

        private void button_down_podachi_Click(object sender, RoutedEventArgs e)
        {
            feed_distance--;
            if (feed_distance <= 0) { feed_distance = 0; }
            screen_hand_mode_podachi.Text = feed_distance.ToString();
            Properties.Settings.Default.feed_distance = feed_distance;
            Send_function();
        }

        private void screen_hand_mode_podachi_TextChanged(object sender, TextChangedEventArgs e)
        {

            string s5 = screen_hand_mode_podachi.Text;
            screen_hand_mode_podachi.MaxLength = 4;
            if (s5.Count()<1) { s5 = "0"; }
            if (feed_distance > 9999) { feed_distance = 9998; }
            if (feed_distance == 0) { on_podachi.Visibility = Visibility.Hidden; screen_hand_mode_podachi.IsEnabled = false; }
            feed_distance = Int32.Parse(s5);
            
            if (feed_distance > 0) { on_podachi.Visibility = Visibility.Visible; screen_hand_mode_podachi.IsEnabled = true; }
            Properties.Settings.Default.feed_distance = feed_distance;
            if (start == 1 && feed_distance != 0) { Send_function(); }


        }
        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            
            LogiWindow logiWindow = new LogiWindow();
            logiWindow.Show();


        }

        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            
            Ok_connect();
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
                        Properties.Settings.Default.COM_port = com_ports_names.Text;
                        Properties.Settings.Default.Save();
                       



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
            timer_connect.Stop();
            progres_bar.Value = 0;
            com_ports_names.IsEnabled = true;
            open_port.IsEnabled = true;
            close_com_port.IsEnabled = false;
        }

       

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Send_function();


        }
         


        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

            Array.Clear(send_mess, 0, 15);
            Array.Clear(enter_mess, 0, 14);
            


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
                error_text.Content = "порт отключен";
            }

        }

        private void start_button_1_Copy_Click(object sender, RoutedEventArgs e)
        {
            auto_revers++;
            if (auto_revers == 2) { auto_revers = 0; }
            if (auto_revers == 1) { on_autu_rev.Visibility = Visibility.Visible; }
            else if (auto_revers == 0) { on_autu_rev.Visibility = Visibility.Hidden; }
            Properties.Settings.Default.auto_revers = auto_revers;
        }

        private void close_com_option_Click(object sender, RoutedEventArgs e)
        {
            com_port_options.Visibility = Visibility.Hidden;
        }

        private void com_ports_names_DropDownOpened(object sender, EventArgs e)
        {
            GetAvaileblePorts();
        }

        private void revers_butt_Click(object sender, RoutedEventArgs e)
        {
            start = 2;
            Send_function();            
        }

        private void screen_hand_mode_1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            
            if ((!Char.IsDigit(e.Text, 0)) && (e.Text != ",") && (e.Text != "."))
            {
                { e.Handled = true; }
            }
            /*
            else if ((e.Text == ",") && ((screen_hand_mode_1.Text.IndexOf(",") != -1) || (screen_hand_mode_1.Text == "")))
            { e.Handled = true; }
            else if ((e.Text == ".") && ((screen_hand_mode_1.Text.IndexOf(".") != -1) || (screen_hand_mode_1.Text == "")))
            { e.Handled = true; }
            */
        }

        private void screen_1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0)) e.Handled = true;
        }

        private void screen_2_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0)) e.Handled = true;
        }

        private void screen_volt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
           
           
            if (!Char.IsDigit(e.Text, 0) && (e.Text != "-")) e.Handled = true;
            
        }

        private void screen_rev_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0)) e.Handled = true;
        }

        private void screen_hand_mode_podachi_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0)) e.Handled = true;
        }

        private void screen_rev_TextChanged(object sender, TextChangedEventArgs e)
        {
            string s6 = screen_rev.Text;
            screen_rev.MaxLength = 2;
            if (s6.Count() < 1) { s6 = "0"; }
            if (feed_distance > 99) { feed_distance = 98; }
            revers_distance = Int32.Parse(s6);
            Properties.Settings.Default.revers_distance = revers_distance;


        }

        private void butteb_up_rev_Click(object sender, RoutedEventArgs e)
        {
            
           
            revers_distance++;
            if (revers_distance > 99) { revers_distance = 99; }
            screen_rev.Text = revers_distance.ToString();
             Send_function();
        }

        private void button_down_rev_Click(object sender, RoutedEventArgs e)
        {
            
            
            revers_distance--;
            if (revers_distance <= 0) { revers_distance = 0; }
            screen_rev.Text = revers_distance.ToString();


            Send_function();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            InstruktionWindow instruktionWindow = new InstruktionWindow();
            instruktionWindow.Show();

        }

        private void screen_volt_TextChanged(object sender, TextChangedEventArgs e)
        {
            string s7 = screen_volt.Text;
            
            
            screen_volt.MaxLength = 4;
            if (s7.Count() < 1) { s7 = "0"; }
            try
            {
                bias_voltage = Int32.Parse(s7);
            }
            catch(Exception)
            { }
            if (bias_voltage > 999) { bias_voltage = 999; }

            
           
            Properties.Settings.Default.bias_voltage = bias_voltage;
            if (start == 1 && bias_voltage != 0) { Send_function(); }
        }

        private void butteb_up_2_Copy_Click(object sender, RoutedEventArgs e)
        {
            
            bias_voltage++;
            if (bias_voltage > 999) { bias_voltage = 999; }
            screen_volt.Text = bias_voltage.ToString();
            Send_function();
        }

        private void button_down_2_Copy_Click(object sender, RoutedEventArgs e)
        {
             
            bias_voltage--;
            if (bias_voltage < -999) { bias_voltage = -999; }
            screen_volt.Text = bias_voltage.ToString();
            Send_function();
        }

        
    }
}
