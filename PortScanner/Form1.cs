using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.IO;

namespace PortScanner
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Dictionary<int, string> dict = new Dictionary<int, string>();//Словарь портов
        class destination //Класс содержащий параметры сканирования
        {
            public string ip = "localhost";
            public int port = 80;
            public int wait = 20;
            public destination(string _ip, int _port, int _wait)
            {
                ip = _ip;
                port = _port;
                wait = _wait;
            }
        }
        static int Scan(object _in)//Функция сканирования
        {
            destination where = (destination)_in;
            Socket socketToServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IAsyncResult resultOfConnect = socketToServer.BeginConnect(where.ip, where.port, null, socketToServer);
            if (resultOfConnect.AsyncWaitHandle.WaitOne(where.wait) == true)
            {
                socketToServer.Close();
                return where.port;
            }
            else
            {
                return 0;
            }
        }
        private void button1_Click(object destinationer, EventArgs e)//Запуск сканирования
        {
            Ping pingSender = new Ping();
            try
            {
                PingReply res = pingSender.Send(textBox1.Text);
                if(res.Status==IPStatus.Success)
                {
                    dataGridView1.Rows.Clear();//Очистка таблицы
                    backgroundWorker1.RunWorkerAsync();//Запуск сканирования
                    button1.Enabled = false;
                    button2.Enabled = true;
                    textBox1.ReadOnly = true;
                    this.Text = "Port scanner (идёт сканирование)";
                    numericUpDown1.Enabled = false;
                    numericUpDown2.Enabled = false;
                    numericUpDown3.Enabled = false;
                    checkBox1.Enabled = false;
                }
            }
            catch (System.Net.NetworkInformation.PingException)
            {
                MessageBox.Show("Недостижим адрес обращения", "Ошибка");
            }
        }
        private void button2_Click(object destinationer, EventArgs e)//Прерывание сканирования
        {
            backgroundWorker1.CancelAsync();
            button1.Enabled = true;
            button2.Enabled = false;
            textBox1.ReadOnly = false;
            numericUpDown1.Enabled = true;
            numericUpDown2.Enabled = true;
            if (!checkBox1.Checked)
            {
                numericUpDown3.Enabled = true;
            }
            this.Text = "Port scanner";
            checkBox1.Enabled = true;
        }
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)//Поток обработки сканирования
        {
            int start = decimal.ToInt32(numericUpDown1.Value);
            int end = decimal.ToInt32(numericUpDown2.Value);
            int wait = decimal.ToInt32(numericUpDown3.Value);
            if (checkBox1.Checked)//Автоматическая настройка ожидания
            {
                Ping pingSender = new Ping();
                PingReply res = pingSender.Send(textBox1.Text);
                wait = (int)res.RoundtripTime + 10;
                Console.WriteLine(wait.ToString());
            }
            for (int i = start; i <= end; i++)//Цикл сканирования
            {
                if (this.backgroundWorker1.CancellationPending != true)
                {
                    Console.WriteLine(i.ToString());
                    int result = Scan(new destination(textBox1.Text, i, wait));
                    if (result != 0)
                    {
                        this.backgroundWorker1.ReportProgress(i + 100000);
                    }
                    else
                    {
                        this.backgroundWorker1.ReportProgress(i);
                    }
                }
            }
        }
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)//Окончание сканирования
        {
            button1.Enabled = true;
            button2.Enabled = false;
            textBox1.ReadOnly = false;
            this.Text = "Port scanner";
            numericUpDown1.Enabled = true;
            numericUpDown2.Enabled = true;
            if (!checkBox1.Checked)
            {
                numericUpDown3.Enabled = true;
            }
            checkBox1.Enabled = true;
        }
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)//Добавление разультата в таблицу
        {
            int progress = e.ProgressPercentage;
            if (e.ProgressPercentage > 65536)//Порт открыт
            {
                progress = progress - 100000;
                string name;
                try
                {
                    name = dict[progress];//Поиск в словаре
                }
                catch
                {
                    name = "Без названия";
                }
                int k = dataGridView1.Rows.Add(progress.ToString() + " - " + name);
                dataGridView1[0, k].Style.ForeColor = Color.Red;
            }
            int start = decimal.ToInt32(numericUpDown1.Value);
            int end = decimal.ToInt32(numericUpDown2.Value);
            progressBar1.Value = (progress - start) * 100 / (end - start);//Правка полоски выполнения
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)//Выбор ожидания
        {
            if (checkBox1.Checked)
            {
                numericUpDown3.Enabled = false;
            }
            else
            {
                numericUpDown3.Enabled = true;
            }
        }
        private void Form1_Load(object sender, EventArgs e)//Загрузка словаря портов
        {
            if (File.Exists(Application.StartupPath + "\\ports.txt"))
            {
                StreamReader loadSettings = new StreamReader(Application.StartupPath + "\\ports.txt", false);
                string buffer = loadSettings.ReadLine();
                int end = int.Parse(buffer);
                for (int i = 1; i <= end; i++)//Загрузка из файла
                {
                    buffer = loadSettings.ReadLine();
                    string[] ink = new string[2];
                    ink = buffer.Split('	');
                    dict.Add(int.Parse(ink[0]), ink[1]);
                }
            }
            else
            {
                MessageBox.Show("Отсутствует список портов", "Ошибка");
            }
        }
        private void textBox1_KeyDown(object sender, KeyEventArgs e)//Запуск по Enter
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick();
            } 
        }
    }
}
