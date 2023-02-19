namespace Launcher
{
    using PNLauncher.Core;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Net;
    using System.Windows.Forms;

    public class SelectMirror : Form
    {
        private IContainer components;
        private TextBox text_mirror_text;
        private Button btn_select_mirror;

        public SelectMirror()
        {
            this.InitializeComponent();
        }

        private void btn_select_mirror_Click(object sender, EventArgs e)
        {
            IPAddress address;
            if (!IPAddress.TryParse(this.text_mirror_text.Text, out address))
            {
                MessageBox.Show("Wrong Mirror IP");
            }
            else
            {
                GameCoordinator.PushNextIP(address);
                base.Close();
            }
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
            ComponentResourceManager manager = new ComponentResourceManager(typeof(SelectMirror));
            this.text_mirror_text = new TextBox();
            this.btn_select_mirror = new Button();
            base.SuspendLayout();
            this.text_mirror_text.Location = new Point(12, 12);
            this.text_mirror_text.Name = "text_mirror_text";
            this.text_mirror_text.Size = new Size(120, 20);
            this.text_mirror_text.TabIndex = 0;
            this.btn_select_mirror.Location = new Point(0x8d, 12);
            this.btn_select_mirror.Name = "btn_select_mirror";
            this.btn_select_mirror.Size = new Size(60, 20);
            this.btn_select_mirror.TabIndex = 1;
            this.btn_select_mirror.Text = "select";
            this.btn_select_mirror.UseVisualStyleBackColor = true;
            this.btn_select_mirror.Click += new EventHandler(this.btn_select_mirror_Click);
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(210, 0x2c);
            base.Controls.Add(this.btn_select_mirror);
            base.Controls.Add(this.text_mirror_text);
            base.Icon = (Icon) manager.GetObject("$this.Icon");
            this.MaximumSize = new Size(0xe2, 0x53);
            this.MinimumSize = new Size(0xe2, 0x53);
            base.Name = "SelectMirror";
            this.Text = "P&N Launcher";
            base.ResumeLayout(false);
            base.PerformLayout();
        }
    }
}

