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
        private bool mode = true;
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
            switch (comboBoxLabChoice.SelectedIndex)
            {
                case 0:
                    _painter.PaintModelLaba1(_model);
                    break;
                case 1:
                    _painter.PaintModelLaba2(_model);
                    break;
                case 2:
                    _painter.PaintModelLaba3(_model);
                    break;
                case 3:
                    _painter.PaintModelLaba4(_model);
                    break;
                case 4:
                    _painter.PaintModelLaba5(_model);
                    break;
                default:
                    _painter.PaintModelLaba5(_model);
                    break;
            }
            

            pictureBox.Refresh();
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            var openDialog = new OpenFileDialog()
            {
                Filter = "OBJ geometry format(*.obj)|*.obj",
                InitialDirectory = "..\\..\\..\\Models"
            };
            if (openDialog.ShowDialog() != DialogResult.OK) return;
            var path = openDialog.FileName;
            //var path = "C:\\BSUIR\\AKGv2\\models\\low_poly_cat.obj";
            ObjParser objParser = new ObjParser();
            _model = objParser.Parse(path);
            _model.UpdateModelInfo(new Vector3(0, 0, 1), new Vector3(0, 0, -1), new Vector3(0, 1, 0));
            _model.RotationOfXInRadians = 0f;
            _model.RotationOfYInRadians = 0f;
            _model.Scale = 0.01f;
            _model.setDefaultMaterial();
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
                    if (mode)
                        _model.ShiftY -= ShiftChange;
                    else
                        _model.lightDir.X = Math.Clamp(_model.lightDir.X + 0.05f, -1, 1);
                    Repaint();
                    break;
                case Keys.S:
                    if(mode)
                    _model.ShiftY += ShiftChange;
                    else
                        _model.lightDir.X = Math.Clamp(_model.lightDir.X - 0.05f, -1, 1);
                    Repaint();
                    break;
                case Keys.A:
                    if (mode)
                    _model.ShiftX -= ShiftChange;
                    else
                        _model.lightDir.Y = Math.Clamp(_model.lightDir.Y + 0.05f, -1, 1);
                    Repaint();
                    break;
                case Keys.D:
                    if(mode)
                    _model.ShiftX += ShiftChange;
                    else
                        _model.lightDir.Y = Math.Clamp(_model.lightDir.Y - 0.05f, -1, 1);
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
                case Keys.M:
                    mode = !mode;
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
            _model.Scale = float.Parse(textBoxScale.Text);
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
            textBoxR.Text = "255";
            textBoxG.Text = "105";
            textBoxB.Text = "180";

            var path = "..\\..\\..\\Models\\cat.obj";
            ObjParser objParser = new ObjParser();
            _model = objParser.Parse(path);
            _model.UpdateModelInfo(new Vector3(0, 0, 1), new Vector3(0, 0, -1), new Vector3(0, 1, 0));
            _model.Scale = 0.09f;
            ScaleChange = 0.001f;
            _model.setDefaultMaterial();
            _model.RotationOfXInRadians = 0;
            _model.RotationOfYInRadians = -1.25f;
            textBoxScale.Text = "0,09";
            textBoxScaleChange.Text = "0,001";
            ResizeImage();
            Repaint();
            Repaint();
        }

        private void buttonSkull_Click(object sender, EventArgs e)
        {
            textBoxR.Text = "125";
            textBoxG.Text = "249";
            textBoxB.Text = "255";

            var path = "..\\..\\..\\Models\\craneo.obj";
            ObjParser objParser = new ObjParser();
            _model = objParser.Parse(path);
            _model.UpdateModelInfo(new Vector3(0, 0, 1), new Vector3(0, 0, -1), new Vector3(0, 1, 0));
            _model.Scale = 0.26f;
            _model.DiffuseMap = new Bitmap("../../../Models/diffuse-maps/craneo.jpg");
            _model.NormalMap = new Bitmap("../../../Models/normal-maps/craneo.jpg");
            _model.SpecularMap = new Bitmap("../../../Models/specular-maps/craneo.jpg");
            ScaleChange = 0.01f;
            _model.setDefaultMaterial();
            _model.RotationOfXInRadians = 0f;
            _model.RotationOfYInRadians = 0f;
            textBoxScale.Text = "0,09";
            textBoxScaleChange.Text = "0,001";
            ResizeImage();
            Repaint();
        }

        private void buttonLightColor_Click(object sender, EventArgs e)
        {
            int rr = Math.Clamp(int.Parse(txtBoxColorR.Text), 0, 255);
            int gg = Math.Clamp(int.Parse(txtBoxColorG.Text), 0, 255);
            int bb = Math.Clamp(int.Parse(txtBoxColorB.Text), 0, 255);
            float r = (float)rr / 255;
            float g = (float)gg / 255;
            float b = (float)bb / 255;
            _model.lightColor = new Vector3(b, g, r);
            Repaint();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }
    }
}