using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Vytas_Lab2
{
    public partial class Form1 : Form
    {
        int figuri = 0;
        Image imgOriginal;
        Point oldLocation;
        bool drawing = false;
        int historyCounter = 0;
        GraphicsPath currentPath;
        public Color historyColor = Color.FromArgb(0, 0, 0);
        Bitmap bitmap = new Bitmap(690, 375);
        Pen currentPen = new Pen(Color.Black) { Width = 1 };
        List<Image> history = new List<Image>();
        SaveFileDialog saveDlg = new SaveFileDialog();
        OpenFileDialog openDlg = new OpenFileDialog();

        int locallX = 0;
        int locallY = 0;
        int locallXO = 0;
        int locallYO = 0;

        bool bucket = false;
        public Form1()
        {
            InitializeComponent();
            history.Add(new Bitmap(690, 375));

            saveDlg.Filter = "All|*|Jpeg Image|*.jpg|Bitmap Image|*.bmp|GIF Image|*.gif|PNG Image|*.png";
            saveDlg.FilterIndex = 1;

            openDlg.Filter = "All|*|Jpeg Image|*.jpg|Bitmap Image|*.bmp|GIF Image|*.gif|PNG Image|*.png";
            openDlg.FilterIndex = 1;

            trackBar1.Scroll += TrackBar1_Scroll;
            pictureBox1.MouseUp += PictureBox1_MouseUp;
            pictureBox1.MouseMove += PictureBox1_MouseMove;
            pictureBox1.MouseDown += PictureBox1_MouseDown;

            trackBar2.Minimum = -2;
            trackBar2.Maximum = 10;
            trackBar2.Enabled = false;

            pictureBox1.MouseWheel += new MouseEventHandler(pMouseWheel);

            this.noneToolStripMenuItem.Checked = true;
        }

        private void pMouseWheel(object sender, MouseEventArgs e)
        {
            if(pictureBox1.Image != null)
            {
                if (e.Delta > 0)
                {
                    if (trackBar2.Value < trackBar2.Maximum)
                    {
                        trackBar2.Value++;
                    }
                }
                else
                {
                    if (trackBar2.Value > trackBar2.Minimum)
                    {
                        trackBar2.Value--;
                    }
                }
                pictureBox1.Image = Zoom(imgOriginal, trackBar2.Value);
            }
        }

        private void TrackBar1_Scroll(object sender, EventArgs e)
        {
            currentPen.Width = trackBar1.Value;
        }

        private void TrackBar2_Scroll(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image = Zoom(imgOriginal, trackBar2.Value);
            }
        }

        Image Zoom(Image img, int size)
        {
            Bitmap bmp = new Bitmap(img, img.Width + (img.Width * size / 10), img.Height + (img.Height * size / 10));
            Graphics g = Graphics.FromImage(bmp);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            return bmp;
        }
        void FloodFill(Bitmap bitmap, int x, int y, Color color)
        {
            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int[] bits = new int[data.Stride / 4 * data.Height];
            Marshal.Copy(data.Scan0, bits, 0, bits.Length);

            LinkedList<Point> check = new LinkedList<Point>();
            int floodTo = color.ToArgb();
            int floodFrom = bits[x + y * data.Stride / 4];
            bits[x + y * data.Stride / 4] = floodTo;

            if (floodFrom != floodTo)
            {
                check.AddLast(new Point(x, y));
                while (check.Count > 0)
                {
                    Point cur = check.First.Value;
                    check.RemoveFirst();

                    foreach (Point off in new Point[] {
                new Point(0, -1), new Point(0, 1),
                new Point(-1, 0), new Point(1, 0)})
                    {
                        Point next = new Point(cur.X + off.X, cur.Y + off.Y);
                        if (next.X >= 0 && next.Y >= 0 &&
                            next.X < data.Width &&
                            next.Y < data.Height)
                        {
                            if (bits[next.X + next.Y * data.Stride / 4] == floodFrom)
                            {
                                check.AddLast(next);
                                bits[next.X + next.Y * data.Stride / 4] = floodTo;
                            }
                        }
                    }
                }
            }

            Marshal.Copy(bits, 0, data.Scan0, bits.Length);
            bitmap.UnlockBits(data);

            pictureBox1.Image = bitmap;
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (drawing && !bucket)
            {
                if (figuri == 0)
                {
                    Graphics g = Graphics.FromImage(pictureBox1.Image);
                    currentPath.AddLine(oldLocation, e.Location);
                    g.DrawPath(currentPen, currentPath);
                    oldLocation = e.Location;
                    g.Dispose();
                    pictureBox1.Invalidate();
                }
                else
                {
                    locallX = oldLocation.X;
                    locallY = oldLocation.Y;
                    locallXO = e.Location.X - oldLocation.X;
                    locallYO = e.Location.Y - oldLocation.Y;
                }
            }
            label1.Text = "x: " + e.X.ToString() + " y: " + e.Y.ToString();
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (bucket)
            {
                FloodFill(new Bitmap(pictureBox1.Image), oldLocation.X, oldLocation.Y, historyColor);
            }
            else if (figuri == 1)
            {
                if(Control.ModifierKeys == Keys.Shift)
                {
                    Graphics g = Graphics.FromImage(pictureBox1.Image);
                    Rectangle pathRect = new Rectangle(locallX, locallY, Math.Max(locallXO, locallYO), Math.Max(locallXO, locallYO));
                    currentPath.AddRectangle(pathRect);
                    g.DrawPath(currentPen, currentPath);
                    oldLocation = e.Location;
                    g.Dispose();
                    pictureBox1.Invalidate();
                }
                else
                {
                    Graphics g = Graphics.FromImage(pictureBox1.Image);
                    Rectangle pathRect = new Rectangle(locallX, locallY, locallXO, locallYO);
                    if (locallXO < 0 && locallYO < 0)
                    {
                        pathRect = new Rectangle(locallX + locallXO, locallY + locallYO, locallXO * -1, locallYO * -1);
                    }
                    else if (locallXO < 0)
                    {
                        pathRect = new Rectangle(locallX + locallXO, locallY, locallXO * -1, locallYO);
                    }
                    else if (locallYO < 0)
                    {
                        pathRect = new Rectangle(locallX, locallY + locallYO, locallXO, locallYO * -1);
                    }
                    currentPath.AddRectangle(pathRect);
                    g.DrawPath(currentPen, currentPath);
                    oldLocation = e.Location;
                    g.Dispose();
                    pictureBox1.Invalidate();
                }
            }
            else if (figuri == 2)
            {
                Graphics g = Graphics.FromImage(pictureBox1.Image);
                Rectangle pathRect = new Rectangle(locallX, locallY, locallXO, locallYO);
                if (locallXO < 0 && locallYO < 0)
                {
                    pathRect = new Rectangle(locallX + locallXO, locallY + locallYO, locallXO * -1, locallYO * -1);
                }
                else if (locallXO < 0)
                {
                    pathRect = new Rectangle(locallX + locallXO, locallY, locallXO * -1, locallYO);
                }
                else if (locallYO < 0)
                {
                    pathRect = new Rectangle(locallX, locallY + locallYO, locallXO, locallYO * -1);
                }
                g.DrawEllipse(currentPen, pathRect);
                oldLocation = e.Location;
                g.Dispose();
                pictureBox1.Invalidate();
            }
            else if (figuri == 3)
            {
                Graphics g = Graphics.FromImage(pictureBox1.Image);
                g.DrawLine(currentPen, new Point(locallX, locallY + locallYO), new Point((locallXO / 2) + locallX, locallY));
                g.DrawLine(currentPen, new Point((locallXO / 2) + locallX, locallY), new Point(locallXO + locallX, locallYO + locallY));
                g.DrawLine(currentPen, new Point(locallXO + locallX, locallYO + locallY), new Point(locallX, locallYO + locallY));
                oldLocation = e.Location;
                g.Dispose();
                pictureBox1.Invalidate();
            }
            else if (figuri == 4)
            {
                Graphics g = Graphics.FromImage(pictureBox1.Image);
                g.DrawLine(currentPen, new Point(locallX, locallY), new Point(locallX + locallXO, locallY + locallYO));
                oldLocation = e.Location;
                g.Dispose();
                pictureBox1.Invalidate();
            }

            history.RemoveRange(historyCounter + 1, history.Count - historyCounter - 1);
            history.Add(new Bitmap(pictureBox1.Image));
            if (historyCounter + 1 < 30) historyCounter++;
            if (history.Count - 1 == 30) history.RemoveAt(0);
            drawing = false;
            try
            {
                currentPath.Dispose();
            }
            catch { };
            imgOriginal = pictureBox1.Image;
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Создайте или выберите новый файл");
                return;
            }
            else if(e.Button == MouseButtons.Left)
            {
                currentPen.Color = historyColor;
                Draw(e);
            }
            else if (e.Button == MouseButtons.Right)
            {
                currentPen.Color = Color.White;
                Draw(e);
            }
        }

        private void Draw(MouseEventArgs e)
        {
            drawing = true;
            oldLocation = e.Location;
            currentPath = new GraphicsPath();
        }

        private void SaveImg()
        {
            saveDlg.ShowDialog();
            if(saveDlg.FileName.Trim() != "")
            {
                FileStream fileStream = (FileStream)saveDlg.OpenFile();
                switch (saveDlg.FilterIndex)
                {
                    case 1:
                        pictureBox1.Image.Save(fileStream, ImageFormat.Jpeg);
                        break;
                    case 2:
                        pictureBox1.Image.Save(fileStream, ImageFormat.Jpeg);
                        break;
                    case 3:
                        pictureBox1.Image.Save(fileStream, ImageFormat.Bmp);
                        break;
                    case 4:
                        pictureBox1.Image.Save(fileStream, ImageFormat.Gif);
                        break;
                    case 5:
                        pictureBox1.Image.Save(fileStream, ImageFormat.Png);
                        break;
                }
                fileStream.Close();
            }
        }

        private void CreatImg()
        {
            if (pictureBox1.Image != null)
            {
                var result = MessageBox.Show("Сохранить текущее изображение перед созданием нового рисунка?", "Предупреждение", MessageBoxButtons.YesNoCancel);

                switch (result)
                {
                    case DialogResult.No:
                        history.Clear();
                        historyCounter = 0;
                        pictureBox1.Image = new Bitmap(690, 375);
                        imgOriginal = pictureBox1.Image;
                        history.Add(new Bitmap(pictureBox1.Image));
                        break;
                    case DialogResult.Yes: SaveImg(); break;
                    case DialogResult.Cancel: return;
                }
            }
            else
            {
                pictureBox1.Image = bitmap;
                trackBar2.Enabled = true;
            }
            Graphics g = Graphics.FromImage(pictureBox1.Image);
            g.Clear(Color.White);
            g.DrawImage(pictureBox1.Image, 0, 0, 690, 375);
        }

        private void OpenImg()
        {
            if(openDlg.ShowDialog() != DialogResult.Cancel)
            {
                pictureBox1.Load(openDlg.FileName);
                pictureBox1.AutoSize = true;
                imgOriginal = pictureBox1.Image;
            }
        }

        private void ExitImg()
        {
            if (pictureBox1.Image != null)
            {
                var result = MessageBox.Show("Сохранить текущее изображение перед созданием нового рисунка?", "Предупреждение", MessageBoxButtons.YesNoCancel);

                switch (result)
                {
                    case DialogResult.No: Application.Exit(); break;
                    case DialogResult.Yes: SaveImg(); Application.Exit(); break;
                    case DialogResult.Cancel: return;
                }
            }
            else
            {
                Application.Exit();
            }
        }

        private void ToolStripButton1_Click(object sender, EventArgs e)
        {
            CreatImg();
        }

        private void ToolStripButton3_Click(object sender, EventArgs e)
        {
            SaveImg();
        }

        private void ToolStripButton2_Click(object sender, EventArgs e)
        {
            OpenImg();
        }

        private void ToolStripButton4_Click(object sender, EventArgs e)
        {
            ExitImg();
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreatImg();
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveImg();
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenImg();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExitImg();
        }

        private void UndoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (history.Count != 0 && historyCounter != 0)
            {
                pictureBox1.Image = new Bitmap(history[--historyCounter]);
            }
        }

        private void RenoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (historyCounter < history.Count - 1)
            {
                pictureBox1.Image = new Bitmap(history[++historyCounter]);
            }
        }

        private void ColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 AddRec = new Form2(historyColor);
            AddRec.Owner = this;
            AddRec.ShowDialog();
        }

        private void ToolStripButton5_Click(object sender, EventArgs e)
        {
            bucket = false;
            Form2 AddRec = new Form2(historyColor);
            AddRec.Owner = this;
            AddRec.ShowDialog();
        }

        private void ToolStripButton6_Click(object sender, EventArgs e)
        {
            bucket = true;
            Form2 AddRec = new Form2(historyColor);
            AddRec.Owner = this;
            AddRec.ShowDialog();
        }

        private void SolidToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentPen.DashStyle = DashStyle.Solid;
            this.solidToolStripMenuItem.Checked = true;
            this.dotToolStripMenuItem.Checked = false;
            this.dashDotDotToolStripMenuItem.Checked = false;
        }

        private void DotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentPen.DashStyle = DashStyle.Dot;
            this.solidToolStripMenuItem.Checked = false;
            this.dotToolStripMenuItem.Checked = true;
            this.dashDotDotToolStripMenuItem.Checked = false;
        }

        private void DashDotDotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentPen.DashStyle = DashStyle.DashDotDot;
            this.solidToolStripMenuItem.Checked = false;
            this.dotToolStripMenuItem.Checked = false;
            this.dashDotDotToolStripMenuItem.Checked = true;
        }

        private void NoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.noneToolStripMenuItem.Checked = true;
            this.boxToolStripMenuItem1.Checked = false;
            this.ellipseToolStripMenuItem.Checked = false;
            this.triangleToolStripMenuItem.Checked = false;
            this.lineToolStripMenuItem.Checked = false;
            figuri = 0;
        }

        private void BoxToolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            this.noneToolStripMenuItem.Checked = false;
            this.boxToolStripMenuItem1.Checked = true;
            this.ellipseToolStripMenuItem.Checked = false;
            this.triangleToolStripMenuItem.Checked = false;
            this.lineToolStripMenuItem.Checked = false;
            figuri = 1;
        }

        private void EllipseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.noneToolStripMenuItem.Checked = false;
            this.boxToolStripMenuItem1.Checked = false;
            this.ellipseToolStripMenuItem.Checked = true;
            this.triangleToolStripMenuItem.Checked = false;
            this.lineToolStripMenuItem.Checked = false;
            figuri = 2;
        }

        private void TriangleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.noneToolStripMenuItem.Checked = false;
            this.boxToolStripMenuItem1.Checked = false;
            this.ellipseToolStripMenuItem.Checked = false;
            this.triangleToolStripMenuItem.Checked = true;
            this.lineToolStripMenuItem.Checked = false;
            figuri = 3;
        }

        private void LineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.noneToolStripMenuItem.Checked = false;
            this.boxToolStripMenuItem1.Checked = false;
            this.ellipseToolStripMenuItem.Checked = false;
            this.triangleToolStripMenuItem.Checked = false;
            this.lineToolStripMenuItem.Checked = true;
            figuri = 4;
        }
    }
}
