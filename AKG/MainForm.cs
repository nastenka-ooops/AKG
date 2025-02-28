using AKG.Drawing;
using AKG.Realization.Elements;
using AKG.Realization;

namespace AKG
{
    public partial class MainForm : Form
    {

        private Model _model;
        private Painter _painter;
        public MainForm()
        {
            InitializeComponent();
            
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {

        }

        private void Repaint()
        {
            if (_model == null || _painter == null) return;

            //            ScaleValue.Text = $"Масштаб: {_model.Scale:F3}";
            //            PoligonSize.Text = $"Количество полигонов: {_model.Faces.Count}";

            _painter.PaintModel(_model);

            pictureBox.Refresh();
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            var openDialog = new OpenFileDialog() { Filter = "OBJ geometry format(*.obj)|*.obj" };
            if (openDialog.ShowDialog() != DialogResult.OK) return;
            var path = openDialog.FileName;

            ObjParser objParser = new ObjParser();
            _model = objParser.Parse(path);
            _model.UpdateModelInfo(new Vertex(0, 0, 10), new Vertex(0, 0, -1), new Vertex(0, 1, 0));
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
    }
}
