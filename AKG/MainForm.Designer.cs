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
            comboBoxLabChoice = new ComboBox();
            label10 = new Label();
            label9 = new Label();
            buttonLightColor = new Button();
            txtBoxColorB = new TextBox();
            txtBoxColorG = new TextBox();
            txtBoxColorR = new TextBox();
            label6 = new Label();
            label7 = new Label();
            label8 = new Label();
            buttonSkull = new Button();
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
            pictureBox.Size = new Size(660, 581);
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
            panel1.Controls.Add(comboBoxLabChoice);
            panel1.Controls.Add(label10);
            panel1.Controls.Add(label9);
            panel1.Controls.Add(buttonLightColor);
            panel1.Controls.Add(txtBoxColorB);
            panel1.Controls.Add(txtBoxColorG);
            panel1.Controls.Add(txtBoxColorR);
            panel1.Controls.Add(label6);
            panel1.Controls.Add(label7);
            panel1.Controls.Add(label8);
            panel1.Controls.Add(buttonSkull);
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
            panel1.Size = new Size(660, 124);
            panel1.TabIndex = 1;
            panel1.Paint += panel1_Paint;
            // 
            // comboBoxLabChoice
            // 
            comboBoxLabChoice.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxLabChoice.FormattingEnabled = true;
            comboBoxLabChoice.Items.AddRange(new object[] { "lab1", "lab2", "lab3", "lab4", "lab5" });
            comboBoxLabChoice.Location = new Point(113, 25);
            comboBoxLabChoice.Name = "comboBoxLabChoice";
            comboBoxLabChoice.Size = new Size(98, 23);
            comboBoxLabChoice.TabIndex = 24;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(568, 8);
            label10.Name = "label10";
            label10.Size = new Size(66, 15);
            label10.TabIndex = 23;
            label10.Text = "Light Color";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(439, 9);
            label9.Name = "label9";
            label9.Size = new Size(74, 15);
            label9.TabIndex = 22;
            label9.Text = "Object Color";
            label9.Click += label9_Click;
            // 
            // buttonLightColor
            // 
            buttonLightColor.Location = new Point(592, 26);
            buttonLightColor.Name = "buttonLightColor";
            buttonLightColor.Size = new Size(56, 73);
            buttonLightColor.TabIndex = 21;
            buttonLightColor.Text = "Change";
            buttonLightColor.UseVisualStyleBackColor = true;
            buttonLightColor.Click += buttonLightColor_Click;
            // 
            // txtBoxColorB
            // 
            txtBoxColorB.Location = new Point(557, 82);
            txtBoxColorB.Name = "txtBoxColorB";
            txtBoxColorB.Size = new Size(29, 23);
            txtBoxColorB.TabIndex = 20;
            txtBoxColorB.Text = "255";
            // 
            // txtBoxColorG
            // 
            txtBoxColorG.Location = new Point(557, 52);
            txtBoxColorG.Name = "txtBoxColorG";
            txtBoxColorG.Size = new Size(29, 23);
            txtBoxColorG.TabIndex = 19;
            txtBoxColorG.Text = "255";
            // 
            // txtBoxColorR
            // 
            txtBoxColorR.Location = new Point(557, 25);
            txtBoxColorR.Name = "txtBoxColorR";
            txtBoxColorR.Size = new Size(29, 23);
            txtBoxColorR.TabIndex = 18;
            txtBoxColorR.Text = "255";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(537, 82);
            label6.Name = "label6";
            label6.Size = new Size(14, 15);
            label6.TabIndex = 17;
            label6.Text = "B";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(537, 55);
            label7.Name = "label7";
            label7.Size = new Size(15, 15);
            label7.TabIndex = 16;
            label7.Text = "G";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(537, 28);
            label8.Name = "label8";
            label8.Size = new Size(14, 15);
            label8.TabIndex = 15;
            label8.Text = "R";
            // 
            // buttonSkull
            // 
            buttonSkull.BackColor = Color.Cyan;
            buttonSkull.Location = new Point(22, 53);
            buttonSkull.Name = "buttonSkull";
            buttonSkull.Size = new Size(85, 23);
            buttonSkull.TabIndex = 14;
            buttonSkull.Text = "Черепушка";
            buttonSkull.UseVisualStyleBackColor = false;
            buttonSkull.Click += buttonSkull_Click;
            // 
            // buttonNastya
            // 
            buttonNastya.BackColor = Color.LightCoral;
            buttonNastya.Location = new Point(22, 83);
            buttonNastya.Name = "buttonNastya";
            buttonNastya.Size = new Size(85, 23);
            buttonNastya.TabIndex = 13;
            buttonNastya.Text = "Настюша";
            buttonNastya.UseVisualStyleBackColor = false;
            buttonNastya.Click += button2_Click;
            // 
            // button1
            // 
            button1.Location = new Point(466, 28);
            button1.Name = "button1";
            button1.Size = new Size(56, 73);
            button1.TabIndex = 12;
            button1.Text = "Change";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBoxB
            // 
            textBoxB.Location = new Point(431, 84);
            textBoxB.Name = "textBoxB";
            textBoxB.Size = new Size(29, 23);
            textBoxB.TabIndex = 11;
            textBoxB.Text = "255";
            // 
            // textBoxG
            // 
            textBoxG.Location = new Point(431, 54);
            textBoxG.Name = "textBoxG";
            textBoxG.Size = new Size(29, 23);
            textBoxG.TabIndex = 10;
            textBoxG.Text = "255";
            // 
            // textBoxR
            // 
            textBoxR.Location = new Point(431, 27);
            textBoxR.Name = "textBoxR";
            textBoxR.Size = new Size(29, 23);
            textBoxR.TabIndex = 9;
            textBoxR.Text = "255";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(411, 84);
            label5.Name = "label5";
            label5.Size = new Size(14, 15);
            label5.TabIndex = 8;
            label5.Text = "B";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(411, 57);
            label4.Name = "label4";
            label4.Size = new Size(15, 15);
            label4.TabIndex = 7;
            label4.Text = "G";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(411, 30);
            label3.Name = "label3";
            label3.Size = new Size(14, 15);
            label3.TabIndex = 6;
            label3.Text = "R";
            label3.Click += label3_Click;
            // 
            // btnScaleChange
            // 
            btnScaleChange.Location = new Point(294, 86);
            btnScaleChange.Name = "btnScaleChange";
            btnScaleChange.Size = new Size(111, 29);
            btnScaleChange.TabIndex = 5;
            btnScaleChange.Text = "Change";
            btnScaleChange.UseVisualStyleBackColor = true;
            btnScaleChange.Click += btnScaleChange_Click;
            // 
            // textBoxScaleChange
            // 
            textBoxScaleChange.Location = new Point(294, 56);
            textBoxScaleChange.Name = "textBoxScaleChange";
            textBoxScaleChange.Size = new Size(111, 23);
            textBoxScaleChange.TabIndex = 4;
            textBoxScaleChange.Text = "0,01";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(213, 59);
            label2.Name = "label2";
            label2.Size = new Size(75, 15);
            label2.TabIndex = 3;
            label2.Text = "scale change";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(255, 28);
            label1.Name = "label1";
            label1.Size = new Size(33, 15);
            label1.TabIndex = 2;
            label1.Text = "scale";
            // 
            // textBoxScale
            // 
            textBoxScale.Location = new Point(294, 25);
            textBoxScale.Name = "textBoxScale";
            textBoxScale.Size = new Size(111, 23);
            textBoxScale.TabIndex = 1;
            textBoxScale.Text = "0,001";
            // 
            // buttonOpen
            // 
            buttonOpen.Location = new Point(22, 22);
            buttonOpen.Name = "buttonOpen";
            buttonOpen.Size = new Size(85, 23);
            buttonOpen.TabIndex = 0;
            buttonOpen.Text = "Открыть обьект";
            buttonOpen.UseVisualStyleBackColor = true;
            buttonOpen.Click += buttonOpen_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(660, 581);
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
        private Button buttonSkull;
        private Button buttonLightColor;
        private TextBox txtBoxColorB;
        private TextBox txtBoxColorG;
        private TextBox txtBoxColorR;
        private Label label6;
        private Label label7;
        private Label label8;
        private Label label9;
        private ComboBox comboBoxLabChoice;
        private Label label10;
    }
}
