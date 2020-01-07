using System;
using System.Drawing;
using System.Windows.Forms;

namespace Vytas_Lab2
{
    public partial class Form2 : Form
    {
        public Color rColor;
        ColorDialog colorDialog = new ColorDialog();
        public Form2(Color historyColor)
        {
            Form1 main = Owner as Form1;
            InitializeComponent();

            rColor = historyColor;

            hScrollBar1.Minimum = 0;
            hScrollBar2.Minimum = 0;
            hScrollBar3.Minimum = 0;
            hScrollBar1.Maximum = 255;
            hScrollBar2.Maximum = 255;
            hScrollBar3.Maximum = 255;
            hScrollBar1.Value = rColor.R;
            hScrollBar2.Value = rColor.G;
            hScrollBar3.Value = rColor.B;

            numericUpDown1.Minimum = 0;
            numericUpDown2.Minimum = 0;
            numericUpDown3.Minimum = 0;
            numericUpDown1.Maximum = 255;
            numericUpDown2.Maximum = 255;
            numericUpDown3.Maximum = 255;
            numericUpDown1.Increment = 1;
            numericUpDown2.Increment = 1;
            numericUpDown3.Increment = 1;
            numericUpDown1.Value = rColor.R;
            numericUpDown2.Value = rColor.G;
            numericUpDown3.Value = rColor.B;

            hScrollBar1.ValueChanged += ValueChanged;
            hScrollBar2.ValueChanged += ValueChanged;
            hScrollBar3.ValueChanged += ValueChanged;
            numericUpDown1.ValueChanged += ValueChanged2;
            numericUpDown2.ValueChanged += ValueChanged2;
            numericUpDown3.ValueChanged += ValueChanged2;

            pictureBox1.BackColor = rColor;
        }

        private void ValueChanged(object sender, EventArgs e)
        {
            rColor = Color.FromArgb(Convert.ToInt32(hScrollBar1.Value), Convert.ToInt32(hScrollBar2.Value), Convert.ToInt32(hScrollBar3.Value));
            numericUpDown1.Value = Convert.ToInt32(hScrollBar1.Value);
            numericUpDown2.Value = Convert.ToInt32(hScrollBar2.Value);
            numericUpDown3.Value = Convert.ToInt32(hScrollBar3.Value);
            pictureBox1.BackColor = rColor;
        }

        private void ValueChanged2(object sender, EventArgs e)
        {
            rColor = Color.FromArgb(Convert.ToInt32(numericUpDown1.Value), Convert.ToInt32(numericUpDown2.Value), Convert.ToInt32(numericUpDown3.Value));
            hScrollBar1.Value = Convert.ToInt32(numericUpDown1.Value);
            hScrollBar2.Value = Convert.ToInt32(numericUpDown2.Value);
            hScrollBar3.Value = Convert.ToInt32(numericUpDown3.Value);
            pictureBox1.BackColor = rColor;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Form1 main = Owner as Form1;
            main.historyColor = rColor;
            Close();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            if(colorDialog.ShowDialog() == DialogResult.OK)
            {
                rColor = colorDialog.Color;
                pictureBox1.BackColor = rColor;
                hScrollBar1.Value = Convert.ToInt32(colorDialog.Color.R);
                hScrollBar2.Value = Convert.ToInt32(colorDialog.Color.G);
                hScrollBar3.Value = Convert.ToInt32(colorDialog.Color.B);
                numericUpDown1.Value = Convert.ToInt32(colorDialog.Color.R);
                numericUpDown2.Value = Convert.ToInt32(colorDialog.Color.G);
                numericUpDown3.Value = Convert.ToInt32(colorDialog.Color.B);
            }
        }
    }
}
