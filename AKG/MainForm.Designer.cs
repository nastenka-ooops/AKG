namespace AKG
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pictureBox = new PictureBox();
            panel1 = new Panel();
            buttonNastya = new Button();
            button1 = new Button();
            textBoxB = new TextBox();
            textBoxG = new TextBox();
            textBoxR = new TextBox();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            btnScaleChange = new Button();
            textBoxScaleChange = new TextBox();
            label2 = new Label();
            label1 = new Label();
            textBoxScale = new TextBox();
            buttonOpen = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // pictureBox
            // 
            pictureBox.BackColor = Color.MistyRose;
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.Location = new Point(0, 0);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new Size(584, 581);
            pictureBox.TabIndex = 0;
            pictureBox.TabStop = false;
            pictureBox.SizeChanged += pictureBox_SizeChanged;
            pictureBox.Click += pictureBox_Click;
            pictureBox.MouseClick += pictureBox_MouseClick;
            pictureBox.MouseDown += pictureBox_MouseDown;
            pictureBox.MouseMove += pictureBox_MouseMove;
            pictureBox.MouseUp += pictureBox_MouseUp;
            pictureBox.MouseWheel += PictureBox_MouseWheel;
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel1.Controls.Add(buttonNastya);
            panel1.Controls.Add(button1);
            panel1.Controls.Add(textBoxB);
            panel1.Controls.Add(textBoxG);
            panel1.Controls.Add(textBoxR);
            panel1.Controls.Add(label5);
            panel1.Controls.Add(label4);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(btnScaleChange);
            panel1.Controls.Add(textBoxScaleChange);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(textBoxScale);
            panel1.Controls.Add(buttonOpen);
            panel1.Location = new Point(0, 457);
            panel1.Name = "panel1";
            panel1.Size = new Size(584, 124);
            panel1.TabIndex = 1;
            // 
            // buttonNastya
            // 
            buttonNastya.BackColor = Color.LightCoral;
            buttonNastya.Location = new Point(22, 83);
            buttonNastya.Name = "buttonNastya";
            buttonNastya.Size = new Size(75, 23);
            buttonNastya.TabIndex = 13;
            buttonNastya.Text = "Настюша";
            buttonNastya.UseVisualStyleBackColor = false;
            buttonNastya.Click += button2_Click;
            // 
            // button1
            // 
            button1.Location = new Point(480, 28);
            button1.Name = "button1";
            button1.Size = new Size(81, 73);
            button1.TabIndex = 12;
            button1.Text = "Change";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBoxB
            // 
            textBoxB.Location = new Point(430, 84);
            textBoxB.Name = "textBoxB";
            textBoxB.Size = new Size(44, 23);
            textBoxB.TabIndex = 11;
            textBoxB.Text = "255";
            // 
            // textBoxG
            // 
            textBoxG.Location = new Point(430, 54);
            textBoxG.Name = "textBoxG";
            textBoxG.Size = new Size(44, 23);
            textBoxG.TabIndex = 10;
            textBoxG.Text = "255";
            // 
            // textBoxR
            // 
            textBoxR.Location = new Point(430, 27);
            textBoxR.Name = "textBoxR";
            textBoxR.Size = new Size(44, 23);
            textBoxR.TabIndex = 9;
            textBoxR.Text = "255";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(410, 84);
            label5.Name = "label5";
            label5.Size = new Size(14, 15);
            label5.TabIndex = 8;
            label5.Text = "B";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(410, 57);
            label4.Name = "label4";
            label4.Size = new Size(15, 15);
            label4.TabIndex = 7;
            label4.Text = "G";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(410, 30);
            label3.Name = "label3";
            label3.Size = new Size(14, 15);
            label3.TabIndex = 6;
            label3.Text = "R";
            label3.Click += label3_Click;
            // 
            // btnScaleChange
            // 
            btnScaleChange.Location = new Point(194, 84);
            btnScaleChange.Name = "btnScaleChange";
            btnScaleChange.Size = new Size(111, 29);
            btnScaleChange.TabIndex = 5;
            btnScaleChange.Text = "Change";
            btnScaleChange.UseVisualStyleBackColor = true;
            btnScaleChange.Click += btnScaleChange_Click;
            // 
            // textBoxScaleChange
            // 
            textBoxScaleChange.Location = new Point(194, 54);
            textBoxScaleChange.Name = "textBoxScaleChange";
            textBoxScaleChange.Size = new Size(111, 23);
            textBoxScaleChange.TabIndex = 4;
            textBoxScaleChange.Text = "0,01";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(113, 57);
            label2.Name = "label2";
            label2.Size = new Size(75, 15);
            label2.TabIndex = 3;
            label2.Text = "scale change";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(155, 26);
            label1.Name = "label1";
            label1.Size = new Size(33, 15);
            label1.TabIndex = 2;
            label1.Text = "scale";
            // 
            // textBoxScale
            // 
            textBoxScale.Location = new Point(194, 23);
            textBoxScale.Name = "textBoxScale";
            textBoxScale.Size = new Size(111, 23);
            textBoxScale.TabIndex = 1;
            textBoxScale.Text = "0,001";
            // 
            // buttonOpen
            // 
            buttonOpen.Location = new Point(22, 22);
            buttonOpen.Name = "buttonOpen";
            buttonOpen.Size = new Size(75, 23);
            buttonOpen.TabIndex = 0;
            buttonOpen.Text = "Открыть обьект";
            buttonOpen.UseVisualStyleBackColor = true;
            buttonOpen.Click += buttonOpen_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(584, 581);
            Controls.Add(panel1);
            Controls.Add(pictureBox);
            KeyPreview = true;
            Name = "MainForm";
            Text = "AKG_Gat_Bru";
            KeyDown += MainForm_KeyDown;
            ((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pictureBox;
        private Panel panel1;
        private Button buttonOpen;
        private TextBox textBoxScale;
        private TextBox textBoxScaleChange;
        private Label label2;
        private Label label1;
        private Button btnScaleChange;
        private Button button1;
        private TextBox textBoxB;
        private TextBox textBoxG;
        private TextBox textBoxR;
        private Label label5;
        private Label label4;
        private Label label3;
        private Button buttonNastya;
    }
}
