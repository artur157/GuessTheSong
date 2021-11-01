using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicStartWithAMoment
{
    public partial class Form1 : Form
    {
        string artist;
        string catalog;
        List<string> titles;
        int point;         // индекс проигрываемой песни в банке песен
        int trueindex;     // индекс кнопки, соотв. проигрываемой песне
        int time = 0;
        int raund = 0;
        List<int> bank;

        IWavePlayer waveOutDevice;
        WaveStream mainOutputStream;
        WaveChannel32 volumeStream;

        public Form1()
        {
            InitializeComponent();
            catalog = @"data\";
            titles = new List<string>();
            bank = new List<int>();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // сначала спросим кого хотят
            FormCollection form = new FormCollection();
            form.ShowDialog(this);
            artist = form.artist;

            if (artist == "")
            {
                Application.Exit();
                return;
            }

            if (artist == "Добавить исполнителя")
            {
                mi_new_Click(null, null);
                if (artist == "")
                    artist = "Машина времени";
            }

            label1.Text = artist;

            waveOutDevice = new WaveOut();

            //fillMenu();    // формируем меню
            fillTitles();    // вытаскиваем список композиций исполнителя
        }

        private void fillMenu()   // формируем меню
        {
            menuStrip1.Items.Clear();

            ToolStripMenuItem mainItem = new ToolStripMenuItem("Выбрать исполнителя");
            DirectoryInfo dir = new DirectoryInfo(catalog);
            foreach (var item in dir.GetDirectories())
            {
                ToolStripMenuItem mi = new ToolStripMenuItem(item.Name);
                mi.Click += mi_Click;
                mainItem.DropDownItems.Add(mi);
            }

            ToolStripMenuItem mi_new = new ToolStripMenuItem("Другой исполнитель");
            mi_new.Click += mi_new_Click;
            mainItem.DropDownItems.Add(mi_new);

            menuStrip1.Items.Add(mainItem);
        }

        private void fillTitles()  // вытаскиваем список композиций исполнителя
        {
            titles.Clear();

            DirectoryInfo dir = new DirectoryInfo(catalog + artist);
            foreach (var item in dir.GetFiles())
            {
                if (item.Name.Substring(item.Name.Length - 3, 3) == "wav" ||
                    item.Name.Substring(item.Name.Length - 3, 3) == "mp3")
                    titles.Add(item.Name);
            }
        }

        void mi_Click(object sender, EventArgs e)
        {
            artist = sender.ToString();
            label1.Text = artist;
            fillTitles();
        }

        void mi_new_Click(object sender, EventArgs e)
        {
            FormNewArtist form = new FormNewArtist();
            form.ShowDialog(this);

            выбратьИсполнителяToolStripMenuItem_Click(sender, e);
            //artist = form.artist != "" ? form.artist : "Машина времени";
            //label1.Text = artist;
            //fillMenu();
        }

        private bool wereEquals(params int[] inds)     // были ли повторения чисел в массиве (песен)?
        {
            int len = inds.Length;
            int last = inds[len - 1];

            foreach (int index in bank)
            {
                if (index == last)
                    return true;
            }

            for (int i = 0; i < len - 1; ++i)
            {
                if (inds[i] == last)
                    return true;
            }

            return false;
        }

        private string getTitle(string s)     // выводим title в нужном формате
        {
            string str = s.Substring(0, s.Length - 4);
            if (str.IndexOf(')') >= 0 && str.IndexOf(')') < 4)
                str = str.Substring(str.IndexOf(')') + 2);
            if (str.Length > 0 && Char.IsDigit(str[0]) && str.IndexOf('.') >= 0 && str.IndexOf('.') < 3)
                str = str.Substring(str.IndexOf('.') + 2);
            return str;
        }

        private void button_Click(int p)      // кнопка - выбрали один из вариантов
        {
            timer1.Stop();
            if (trueindex == p)
            {
                label3.Text = "" + (Int32.Parse(label3.Text) + time);
                label6.Text = "" + (Int32.Parse(label6.Text) + 1);
                MessageBox.Show("Правильно!");
            }
            else
            {
                MessageBox.Show("Неправильно... Это была - " + getTitle(titles[point]));
            }

            button1.Enabled = button2.Enabled = button3.Enabled = button4.Enabled = button5.Enabled = false;
            button6.Enabled = true;
            waveOutDevice.Stop();

            if (raund == 10)
            {
                button6.Enabled = false;
                MessageBox.Show("Поздравляем! Вы набрали " + label3.Text + " очков и угадали " + label6.Text + " песен.");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button_Click(0);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button_Click(1);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button_Click(2);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            button_Click(3);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            button_Click(4);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            waveOutDevice.Stop();
            waveOutDevice = new WaveOut();

            Random rnd = new Random();

            point = rnd.Next(titles.Count());
            int i = 0;
            while (i < bank.Count)
            {
                if (point == bank[i])
                {
                    i = 0;
                    point = rnd.Next(titles.Count());
                }
                else
                {
                    ++i;
                }
            }
            bank.Add(point);

            if (titles[point].Substring(titles[point].Length - 3, 3) == "wav")
            {
                WaveStream wmaReader = new WaveFileReader(catalog + artist + "\\" + titles[point]);
                volumeStream = new WaveChannel32(wmaReader);
            }
            else
            {
                MemoryStream mp3Stream = new MemoryStream(File.ReadAllBytes(catalog + artist + "\\" + titles[point]));
                Mp3FileReader mp3FileReader = new Mp3FileReader(mp3Stream);
                volumeStream = new WaveChannel32(mp3FileReader, 0.1f, 1f);
            }

            mainOutputStream = volumeStream;

            int moment = rnd.Next((int)mainOutputStream.TotalTime.TotalSeconds - 30);

            mainOutputStream.Skip(moment);
            waveOutDevice.Init(mainOutputStream);
            waveOutDevice.Play();

            // теперь формируем названия кнопок
            // рандомно вытаскиваем 5 разных названий, не совпадающих с нашим
            int p1, p2, p3, p4, p5;
            p1 = p2 = p3 = p4 = p5 = -1;
            do
            {
                p1 = rnd.Next(titles.Count());
            } while (wereEquals(p1));
            do
            {
                p2 = rnd.Next(titles.Count());
            } while (wereEquals(p1, p2));
            do
            {
                p3 = rnd.Next(titles.Count());
            } while (wereEquals(p1, p2, p3));
            do
            {
                p4 = rnd.Next(titles.Count());
            } while (wereEquals(p1, p2, p3, p4));
            do
            {
                p5 = rnd.Next(titles.Count());
            } while (wereEquals(p1, p2, p3, p4, p5));

            button1.Text = getTitle(titles[p1]);
            button2.Text = getTitle(titles[p2]);
            button3.Text = getTitle(titles[p3]);
            button4.Text = getTitle(titles[p4]);
            button5.Text = getTitle(titles[p5]);

            trueindex = rnd.Next(5);
            switch (trueindex)
            {
                case 0: button1.Text = getTitle(titles[point]); break;
                case 1: button2.Text = getTitle(titles[point]); break;
                case 2: button3.Text = getTitle(titles[point]); break;
                case 3: button4.Text = getTitle(titles[point]); break;
                case 4: button5.Text = getTitle(titles[point]); break;
            }

            button1.Enabled = button2.Enabled = button3.Enabled = button4.Enabled = button5.Enabled = true;
            button6.Enabled = false;
            time = 21;
            timer1.Start();
            label4.Text = "20";

            ++raund;
            label7.Text = "Раунд " + raund + " из 10";
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (time > 0) { 
                time--;
                label4.Text = "" + time;
            }
        }

        private void выбратьИсполнителяToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormCollection form = new FormCollection();
            form.ShowDialog(this);
            artist = form.artist;

            if (artist == "Добавить исполнителя")
                mi_new_Click(null, null);

            if (artist == "")
                artist = "Машина времени";

            label1.Text = artist;
            label3.Text = label6.Text = "0";
            raund = 0;
            label7.Text = "Раунд 1 из 10";
            button1.Enabled = button2.Enabled = button3.Enabled = button4.Enabled = button5.Enabled = false;
            button6.Enabled = true;
            timer1.Stop();
            label4.Text = "";
            bank.Clear();

            fillTitles();    // вытаскиваем список композиций исполнителя
        }

    }
}
