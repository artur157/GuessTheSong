using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicStartWithAMoment
{
    public partial class Card : UserControl
    {
        FormCollection form;

        public Card(string artist, FormCollection form)
        {
            InitializeComponent();

            label1.Text = artist;
            this.form = form;
            pictureBox1.Image = Image.FromFile(@"data\" + artist + ".jpg");
        }

        private void Card_Click(object sender, EventArgs e)
        {
            form.artist = label1.Text;
            form.Close();
        }

        private void Card_MouseEnter(object sender, EventArgs e)
        {
            this.BackColor = Color.Blue;
        }

        private void Card_MouseLeave(object sender, EventArgs e)
        {
            this.BackColor = Color.MediumBlue;
        }
    }
}
