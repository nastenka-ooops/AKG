using System.Numerics;
using AKG.Drawing;
using AKG.Realization.Elements;
using AKG.Realization;
using static System.Math;
using System.IO;

namespace AKG
{
    public partial class MainForm : Form
    {
        private float ScaleChange = 0.01f;
        private const float ShiftChange = 1f;

        private Model _model;
        private Painter _painter;

        public MainForm()
        {
            InitializeComponent();
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {
        }

        private void PictureBox_MouseWheel(object sender, MouseEventArgs e)
        {
            // Определение направления прокрутки
            if (e.Delta > 0)
            {
                _model.Scale += ScaleChange; // Обработка прокрутки вверх
                textBoxScale.Text = _model.Scale.ToString();
                Repaint();
            }
            else if (e.Delta < 0)
            {
                _model.Scale -= ScaleChange;
                if (_model.Scale <= 0)
                {
                    _model.Scale = 0.001f;
                }
                textBoxScale.Text = _model.Scale.ToString();
                Repaint();
                // Обработка прокрутки вниз
            }
        }

        private void Repaint()
        {
            if (_model == null || _painter == null) return;

            _painter.PaintModelLaba2(_model);

            pictureBox.Refresh();
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            var openDialog = new OpenFileDialog() { Filter = "OBJ geometry format(*.obj)|*.obj" };
            if (openDialog.ShowDialog() != DialogResult.OK) return;
            var path = openDialog.FileName;
            //var path = "C:\\BSUIR\\AKGv2\\models\\low_poly_cat.obj";
            ObjParser objParser = new ObjParser();
            _model = objParser.Parse(path);
            _model.UpdateModelInfo(new Vector3(0, 0, 1), new Vector3(0, 0, -1), new Vector3(0, 1, 0));
            ResizeImage();
            Repaint();
        }

        private void ResizeImage()
        {
            var bitmap = new Bitmap(pictureBox.Width, pictureBox.Height);
            pictureBox.Image = bitmap;
            _painter = new Painter(bitmap);
            int r = int.Parse(textBoxR.Text);
            int g = int.Parse(textBoxG.Text);
            int b = int.Parse(textBoxB.Text);
            _painter.R = r;
            _painter.G = g;
            _painter.B = b;
        }

        private void pictureBox_SizeChanged(object sender, EventArgs e)
        {
            ResizeImage();
            Repaint();
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    _model.ShiftY -= ShiftChange;
                    Repaint();
                    break;
                case Keys.S:
                    _model.ShiftY += ShiftChange;
                    Repaint();
                    break;
                case Keys.A:
                    _model.ShiftX -= ShiftChange;
                    Repaint();
                    break;
                case Keys.D:
                    _model.ShiftX += ShiftChange;
                    Repaint();
                    break;
                case Keys.J:
                    _model.RotationOfXInRadians += (float)(10 * PI / 180);
                    Repaint();
                    break;
                case Keys.L:
                    _model.RotationOfXInRadians -= (float)(10 * PI / 180);
                    Repaint();
                    break;
                case Keys.I:
                    _model.RotationOfYInRadians += (float)(10 * PI / 180);
                    Repaint();
                    break;
                case Keys.K:
                    _model.RotationOfYInRadians -= (float)(10 * PI / 180);
                    Repaint();
                    break;
                case Keys.R:
                    _model.Scale += 0.001f;
                    Repaint();
                    break;
                case Keys.F:
                    _model.Scale -= 0.001f;
                    if (_model.Scale <= 0)
                    {
                        _model.Scale = 0.001f;
                    }

                    Repaint();

                    break;
            }

        }

        private bool mousePressed = false;
        int mouseX = 0;
        int mouseY = 0;
        float oldXRotate;
        float oldYRotate;

        private void pictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            mousePressed = true;
            mouseX = e.X;
            mouseY = e.Y;
            oldXRotate = _model.RotationOfXInRadians;
            oldYRotate = _model.RotationOfYInRadians;
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (mousePressed)
                if (_painter != null && _model != null)
                {
                    _model.RotationOfXInRadians =
                        oldXRotate + (float)((float)(e.Y - mouseY) / 200 * PI / (double)Math.PI);
                    _model.RotationOfYInRadians =
                        oldYRotate + (float)((float)(e.X - mouseX) / 200 * PI / (double)Math.PI);
                    Repaint();
                }
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            mousePressed = false;
            _model.RotationOfXInRadians = oldXRotate + (float)((float)(e.Y - mouseY) / 200 * PI / (double)Math.PI);
            _model.RotationOfYInRadians = oldYRotate + (float)((float)(e.X - mouseX) / 200 * PI / (double)Math.PI);
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            mousePressed = true;
            mouseX = e.X;
            mouseY = e.Y;
        }

        private void btnScaleChange_Click(object sender, EventArgs e)
        {
            ScaleChange = float.Parse(textBoxScaleChange.Text);
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            int r = int.Parse(textBoxR.Text);
            int g = int.Parse(textBoxG.Text);
            int b = int.Parse(textBoxB.Text);
            _painter.R = r;
            _painter.G = g;
            _painter.B = b;
            Repaint();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            var path = "..\\..\\..\\Models\\craneo.obj";
            ObjParser objParser = new ObjParser();
            _model = objParser.Parse(path);
            _model.UpdateModelInfo(new Vector3(0, 0, 1), new Vector3(0, 0, -1), new Vector3(0, 1, 0));
            ResizeImage();
            Repaint();
        }
    }
}