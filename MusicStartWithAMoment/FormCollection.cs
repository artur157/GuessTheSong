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
    public partial class FormCollection : Form
    {
        public string artist = "";

        public FormCollection()
        {
            InitializeComponent();

            string catalog = @"data\";

            DirectoryInfo dir = new DirectoryInfo(catalog);
            foreach (var item in dir.GetDirectories())
            {
                if (item.Name[0] != '$')
                {
                    Card card = new Card(item.Name, this);
                    flowLayoutPanel1.Controls.Add(card);
                }
            }

            Card card_new = new Card("Добавить исполнителя", this);
            flowLayoutPanel1.Controls.Add(card_new);
        }
    }
}
