using System.Numerics;
using AKG.Drawing;
using AKG.Realization.Elements;
using AKG.Realization;
using static System.Math;
namespace AKG
{
    public partial class MainForm : Form
    {
        private const float ScaleChange = 0.05f;
        private const float ShiftChange = 10f;

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
                _model.Scale += ScaleChange;                // Обработка прокрутки вверх

                Repaint();
            }
            else if (e.Delta < 0)
            {
                _model.Scale -= ScaleChange;
                if (_model.Scale <= 0) { _model.Scale = 0.001f; }
                Repaint();
                // Обработка прокрутки вниз
            }

            // Вывод координат курсора
            Console.WriteLine($"Координаты: X={e.X}, Y={e.Y}");
        }
        private void Repaint()
        {
            if (_model == null || _painter == null) return;


            _painter.PaintModel(_model);

            pictureBox.Refresh();
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            var openDialog = new OpenFileDialog() { Filter = "OBJ geometry format(*.obj)|*.obj" };
             if (openDialog.ShowDialog() != DialogResult.OK) return;
            var path = openDialog.FileName;
            //var path = "E:\\6sem\\AkgGit\\AKG\\models\\low_poly_cat.obj";
            ObjParser objParser = new ObjParser();
            _model = objParser.Parse(path);
            _model.UpdateModelInfo(new Vector3(0, 0, 1), new Vector3(0, 0, -1), new Vector3(0, 1, 0));
             _model.ShiftX = 0.3f;
              _model.ShiftY = 0.3f;
            ResizeImage();
            Repaint();
        }

        private void ResizeImage()
        {
            var bitmap = new Bitmap(pictureBox.Width, pictureBox.Height);
            pictureBox.Image = bitmap;
            _painter = new Painter(bitmap);
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
                    // _model.eye.Y += 1;
                    break;
                case Keys.S:
                    _model.ShiftY += ShiftChange;
                    //_model.eye.Y += 1;

                    break;
                case Keys.A:
                    _model.ShiftX -= ShiftChange;
                    // _model.eye.X -= 1;

                    break;
                case Keys.D:
                    //_model.eye.X += 1;

                    _model.ShiftX += ShiftChange;
                    break;
                case Keys.J:
                    _model.RotationOfXInRadians += (float)(10 * PI / 180);
                    break;
                case Keys.L:
                    _model.RotationOfXInRadians -= (float)(10 * PI / 180);
                    break;
                case Keys.I:
                    _model.RotationOfYInRadians += (float)(10 * PI / 180);
                    break;
                case Keys.K:
                    _model.RotationOfYInRadians -= (float)(10 * PI / 180);
                    break;

            }
            Repaint();
        }
        private bool mousePressed = false;
        int mouseX = 0;
        int mouseY = 0;
        float oldXRotate;
        float oldYRotate;
        private void pictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            mousePressed = true;
            mouseX = e.X; mouseY = e.Y;
            oldXRotate = _model.RotationOfXInRadians;
            oldYRotate = _model.RotationOfYInRadians;
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (mousePressed)
                if (_painter != null && _model != null)
                {
                    _model.RotationOfXInRadians = oldXRotate + (float)((float)(e.X - mouseX) / 200 * PI / (double)Math.PI);
                    _model.RotationOfYInRadians = oldYRotate + (float)((float)(e.Y - mouseY) / 200 * PI / (double)Math.PI);
                    Repaint();
                }
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            mousePressed = false;
            _model.RotationOfXInRadians = oldXRotate + (float)((float)(e.X - mouseX) / 200 * PI / (double)Math.PI);
            _model.RotationOfYInRadians = oldYRotate + (float)((float)(e.Y - mouseY) / 200 * PI / (double)Math.PI);
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            mousePressed = true;
            mouseX = e.X; mouseY = e.Y;
        }
    }
}
