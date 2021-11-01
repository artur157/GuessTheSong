using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Diagnostics;
using System.IO;

namespace MusicStartWithAMoment
{
    public partial class FormNewArtist : Form
    {
        public string artist = "";

        public FormNewArtist()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("Введите исполнителя", "Введите исполнителя");
                return;
            }

            label1.Text = "Ожидайте...";

            int x = 2;
            for (int i = 0; i < 20; i++)
            {
                x = x * x;
            }

            // через cmd запускаем python со вспомогательным скриптом
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo("CMD.exe", @"/C python music_downloader.py " + textBox1.Text);
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.Start();

            // для получения обратной связи
            StringBuilder q = new StringBuilder();
            while (!p.HasExited)
            {
                q.Append(p.StandardOutput.ReadToEnd());
            }
            string r = q.ToString().Trim();

            if (r.Length > 0)
            {
                string msg = r[r.Length - 1] == '0' ? "Исполнитель успешно добавлен" : "Исполнитель не может быть добавлен";
                MessageBox.Show(msg, msg);

                if (r[r.Length - 1] == '0')
                {
                    artist = textBox1.Text;
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("Проблема с python", "Проблема с python");
            }
        }
    }
}
