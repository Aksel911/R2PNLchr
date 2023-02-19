namespace Launcher
{
    using PNLauncher;
    using PNLauncher.Core;
    using PNLauncher.Network;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    public class AuthForm : Form
    {
        private MainForm rf;
        public bool Active;
        private IContainer components;
        public Label gui_auth_login;
        public Label gui_auth_password;
        private TextBox text_auth_login;
        private TextBox text_auth_password;
        private Button auth_btn;

        public AuthForm(MainForm f)
        {
            this.rf = f;
            this.InitializeComponent();
        }

        private void authbtn_Click(object sender, EventArgs e)
        {
            GameCoordinator.Send(new AuthPacket(this.text_auth_login.Text, this.text_auth_password.Text));
        }

        private void AuthForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Active = false;
        }

        private void AuthForm_Load(object sender, EventArgs e)
        {
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
            ComponentResourceManager manager = new ComponentResourceManager(typeof(AuthForm));
            this.gui_auth_login = new Label();
            this.gui_auth_password = new Label();
            this.text_auth_login = new TextBox();
            this.text_auth_password = new TextBox();
            this.auth_btn = new Button();
            base.SuspendLayout();
            this.gui_auth_login.AutoSize = true;
            this.gui_auth_login.Location = new Point(20, 0x16);
            this.gui_auth_login.Name = "gui_auth_login";
            this.gui_auth_login.Size = new Size(0x2c, 13);
            this.gui_auth_login.TabIndex = 0;
            this.gui_auth_login.Text = "Логин: ";
            this.gui_auth_password.AutoSize = true;
            this.gui_auth_password.Location = new Point(12, 0x35);
            this.gui_auth_password.Name = "gui_auth_password";
            this.gui_auth_password.Size = new Size(0x30, 13);
            this.gui_auth_password.TabIndex = 1;
            this.gui_auth_password.Text = "Пароль:";
            this.text_auth_login.Location = new Point(0x45, 0x13);
            this.text_auth_login.MaxLength = 20;
            this.text_auth_login.Name = "text_auth_login";
            this.text_auth_login.Size = new Size(0x76, 20);
            this.text_auth_login.TabIndex = 2;
            this.text_auth_password.Location = new Point(0x45, 0x33);
            this.text_auth_password.MaxLength = 20;
            this.text_auth_password.Name = "text_auth_password";
            this.text_auth_password.PasswordChar = '•';
            this.text_auth_password.Size = new Size(0x76, 20);
            this.text_auth_password.TabIndex = 3;
            this.auth_btn.Location = new Point(0x17, 0x58);
            this.auth_btn.Name = "auth_btn";
            this.auth_btn.Size = new Size(0xa4, 0x17);
            this.auth_btn.TabIndex = 4;
            this.auth_btn.Text = "Авторизация";
            this.auth_btn.UseVisualStyleBackColor = true;
            this.auth_btn.Click += new EventHandler(this.authbtn_Click);
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0xcc, 0x7f);
            base.Controls.Add(this.auth_btn);
            base.Controls.Add(this.text_auth_password);
            base.Controls.Add(this.text_auth_login);
            base.Controls.Add(this.gui_auth_password);
            base.Controls.Add(this.gui_auth_login);
            base.Icon = (Icon) manager.GetObject("$this.Icon");
            this.MaximumSize = new Size(220, 0xa6);
            this.MinimumSize = new Size(220, 0xa6);
            base.Name = "AuthForm";
            this.Text = "P&N Launcher";
            base.FormClosed += new FormClosedEventHandler(this.AuthForm_FormClosed);
            base.Load += new EventHandler(this.AuthForm_Load);
            base.ResumeLayout(false);
            base.PerformLayout();
        }
    }
}

