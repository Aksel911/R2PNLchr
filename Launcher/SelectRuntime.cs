namespace Launcher
{
    using Launcher.Properties;
    using PNLauncher;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    public class SelectRuntime : Form
    {
        private MainForm mMainForm;
        private IContainer components;
        private Button btn_runtime_main;
        private Button btn_runtime_pts;
        private TextBox txtbox_main_path;
        private TextBox txtbox_pts_path;
        private PictureBox pic_pts_set_dir;
        private PictureBox reborn_select_path;
        private TextBox reborn_path;
        private Button select_reborn;

        public SelectRuntime(MainForm pForm)
        {
            this.InitializeComponent();
            this.mMainForm = pForm;
            this.UpdateGuiPath();
            if (MainForm.mRunTime.mRunTimeType == MainForm.ERunTime.Main)
            {
                this.btn_runtime_main.Enabled = false;
                this.pic_pts_set_dir.Visible = true;
            }
            else if (MainForm.mRunTime.mRunTimeType == MainForm.ERunTime.PTS)
            {
                this.btn_runtime_pts.Enabled = false;
                this.pic_pts_set_dir.Visible = false;
            }
            else if (MainForm.mRunTime.mRunTimeType == MainForm.ERunTime.Reborn)
            {
                this.select_reborn.Enabled = false;
                this.reborn_select_path.Visible = false;
            }
        }

        private void btn_runtime_main_Click(object sender, EventArgs e)
        {
            MainForm.mRunTime = MainForm.mRunTimeList[MainForm.ERunTime.Main];
            base.Close();
        }

        private void btn_runtime_pts_Click(object sender, EventArgs e)
        {
            MainForm.mRunTime = MainForm.mRunTimeList[MainForm.ERunTime.PTS];
            base.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(SelectRuntime));
            this.btn_runtime_main = new Button();
            this.btn_runtime_pts = new Button();
            this.txtbox_main_path = new TextBox();
            this.txtbox_pts_path = new TextBox();
            this.pic_pts_set_dir = new PictureBox();
            this.reborn_select_path = new PictureBox();
            this.reborn_path = new TextBox();
            this.select_reborn = new Button();
            ((ISupportInitialize) this.pic_pts_set_dir).BeginInit();
            ((ISupportInitialize) this.reborn_select_path).BeginInit();
            base.SuspendLayout();
            this.btn_runtime_main.Location = new Point(12, 12);
            this.btn_runtime_main.Name = "btn_runtime_main";
            this.btn_runtime_main.Size = new Size(0x74, 0x17);
            this.btn_runtime_main.TabIndex = 0;
            this.btn_runtime_main.Text = "Main";
            this.btn_runtime_main.UseVisualStyleBackColor = true;
            this.btn_runtime_main.Click += new EventHandler(this.btn_runtime_main_Click);
            this.btn_runtime_pts.Location = new Point(0x86, 12);
            this.btn_runtime_pts.Name = "btn_runtime_pts";
            this.btn_runtime_pts.Size = new Size(0x74, 0x17);
            this.btn_runtime_pts.TabIndex = 1;
            this.btn_runtime_pts.Text = "PTS";
            this.btn_runtime_pts.UseVisualStyleBackColor = true;
            this.btn_runtime_pts.Click += new EventHandler(this.btn_runtime_pts_Click);
            this.txtbox_main_path.Location = new Point(12, 0x29);
            this.txtbox_main_path.Name = "txtbox_main_path";
            this.txtbox_main_path.ReadOnly = true;
            this.txtbox_main_path.Size = new Size(0x53, 20);
            this.txtbox_main_path.TabIndex = 2;
            this.txtbox_pts_path.Location = new Point(0x86, 0x2a);
            this.txtbox_pts_path.Name = "txtbox_pts_path";
            this.txtbox_pts_path.ReadOnly = true;
            this.txtbox_pts_path.Size = new Size(0x53, 20);
            this.txtbox_pts_path.TabIndex = 3;
            this.pic_pts_set_dir.Cursor = Cursors.Hand;
            this.pic_pts_set_dir.Image = Resources.folder;
            this.pic_pts_set_dir.InitialImage = (Image) manager.GetObject("pic_pts_set_dir.InitialImage");
            this.pic_pts_set_dir.Location = new Point(0xdf, 0x29);
            this.pic_pts_set_dir.Name = "pic_pts_set_dir";
            this.pic_pts_set_dir.Size = new Size(0x1b, 0x15);
            this.pic_pts_set_dir.TabIndex = 4;
            this.pic_pts_set_dir.TabStop = false;
            this.pic_pts_set_dir.Click += new EventHandler(this.pic_pts_set_dir_Click);
            this.reborn_select_path.Cursor = Cursors.Hand;
            this.reborn_select_path.Image = Resources.folder;
            this.reborn_select_path.InitialImage = (Image) manager.GetObject("reborn_select_path.InitialImage");
            this.reborn_select_path.Location = new Point(0x159, 0x29);
            this.reborn_select_path.Name = "reborn_select_path";
            this.reborn_select_path.Size = new Size(0x1b, 0x15);
            this.reborn_select_path.TabIndex = 7;
            this.reborn_select_path.TabStop = false;
            this.reborn_select_path.Click += new EventHandler(this.reborn_select_path_Click);
            this.reborn_path.Location = new Point(0x100, 0x2a);
            this.reborn_path.Name = "reborn_path";
            this.reborn_path.ReadOnly = true;
            this.reborn_path.Size = new Size(0x53, 20);
            this.reborn_path.TabIndex = 6;
            this.select_reborn.Location = new Point(0x100, 12);
            this.select_reborn.Name = "select_reborn";
            this.select_reborn.Size = new Size(0x74, 0x17);
            this.select_reborn.TabIndex = 5;
            this.select_reborn.Text = "Reborn";
            this.select_reborn.UseVisualStyleBackColor = true;
            this.select_reborn.Click += new EventHandler(this.select_reborn_Click);
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x17b, 0x48);
            base.Controls.Add(this.reborn_select_path);
            base.Controls.Add(this.reborn_path);
            base.Controls.Add(this.select_reborn);
            base.Controls.Add(this.pic_pts_set_dir);
            base.Controls.Add(this.txtbox_pts_path);
            base.Controls.Add(this.txtbox_main_path);
            base.Controls.Add(this.btn_runtime_pts);
            base.Controls.Add(this.btn_runtime_main);
            base.Icon = (Icon) manager.GetObject("$this.Icon");
            base.Name = "SelectRuntime";
            this.Text = "SelectRuntime";
            ((ISupportInitialize) this.pic_pts_set_dir).EndInit();
            ((ISupportInitialize) this.reborn_select_path).EndInit();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void pic_pts_set_dir_Click(object sender, EventArgs e)
        {
            MainForm.mRunTimeList[MainForm.ERunTime.PTS].SelectDirectory();
            this.UpdateGuiPath();
        }

        private void reborn_select_path_Click(object sender, EventArgs e)
        {
            MainForm.mRunTimeList[MainForm.ERunTime.Reborn].SelectDirectory();
            this.UpdateGuiPath();
        }

        private void select_reborn_Click(object sender, EventArgs e)
        {
            MainForm.mRunTime = MainForm.mRunTimeList[MainForm.ERunTime.Reborn];
            base.Close();
        }

        private void UpdateGuiPath()
        {
            this.txtbox_main_path.Text = MainForm.mRunTimeList[MainForm.ERunTime.Main].GetDirectory();
            this.txtbox_pts_path.Text = (MainForm.mRunTimeList[MainForm.ERunTime.PTS].GetDirectory() == null) ? "(null)" : MainForm.mRunTimeList[MainForm.ERunTime.PTS].GetDirectory();
            this.reborn_path.Text = (MainForm.mRunTimeList[MainForm.ERunTime.Reborn].GetDirectory() == null) ? "(null)" : MainForm.mRunTimeList[MainForm.ERunTime.Reborn].GetDirectory();
            this.txtbox_main_path.SelectionStart = this.txtbox_main_path.TextLength;
            this.txtbox_pts_path.SelectionStart = this.txtbox_pts_path.TextLength;
            this.reborn_path.SelectionStart = this.reborn_path.TextLength;
        }
    }
}

