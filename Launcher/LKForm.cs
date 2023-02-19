namespace Launcher
{
    using Launcher.Network;
    using PNLauncher;
    using PNLauncher.Core;
    using PNLauncher.Languages;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    public class LKForm : Form
    {
        public MainForm.ProfileInfo Info;
        public MainForm _mf;
        public bool Active;
        private IContainer components;
        public Label gui_balance;
        public GroupBox lk_header;
        public GroupBox gui_passchange_header;
        public Button btn_change_pass;
        public Label gui_newpassword;
        public TextBox gui_old_pass;
        public Label gui_lastpassword;
        public TextBox gui_new_pass;
        public DataGridView entry_grid;
        public Label gui_lastip;
        public Label gui_gamestatus;
        public Label gui_last_activity;
        private DataGridViewTextBoxColumn grid_server;
        private DataGridViewTextBoxColumn grid_date;
        private DataGridViewTextBoxColumn grid_ip;
        public Label gui_retrynewpass;
        public TextBox gui_retry_new_pass;
        private PictureBox show_pass_Pic;

        public LKForm(MainForm mf)
        {
            this._mf = mf;
            this.InitializeComponent();
        }

        private void btn_change_pass_Click(object sender, EventArgs e)
        {
            string text = this.gui_old_pass.Text;
            string newPassword = this.gui_new_pass.Text;
            string str3 = this.gui_retry_new_pass.Text;
            if ((text.Length < 6) || ((newPassword.Length < 6) || (str3.Length < 6)))
            {
                this._mf.ShowError(LangController.GetTranslate("msg_password_too_low"));
            }
            else if (newPassword != str3)
            {
                this._mf.ShowError(LangController.GetTranslate("msg_password_differ"));
            }
            else if (newPassword == text)
            {
                this._mf.ShowError(LangController.GetTranslate("msg_password_no_differ"));
            }
            else
            {
                GameCoordinator.Send(new ChangePassword(text, newPassword));
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
            ComponentResourceManager manager = new ComponentResourceManager(typeof(LKForm));
            this.gui_balance = new Label();
            this.lk_header = new GroupBox();
            this.gui_last_activity = new Label();
            this.gui_passchange_header = new GroupBox();
            this.show_pass_Pic = new PictureBox();
            this.btn_change_pass = new Button();
            this.gui_retrynewpass = new Label();
            this.gui_retry_new_pass = new TextBox();
            this.gui_newpassword = new Label();
            this.gui_old_pass = new TextBox();
            this.gui_lastpassword = new Label();
            this.gui_new_pass = new TextBox();
            this.entry_grid = new DataGridView();
            this.grid_server = new DataGridViewTextBoxColumn();
            this.grid_date = new DataGridViewTextBoxColumn();
            this.grid_ip = new DataGridViewTextBoxColumn();
            this.gui_lastip = new Label();
            this.gui_gamestatus = new Label();
            this.lk_header.SuspendLayout();
            this.gui_passchange_header.SuspendLayout();
            ((ISupportInitialize) this.show_pass_Pic).BeginInit();
            ((ISupportInitialize) this.entry_grid).BeginInit();
            base.SuspendLayout();
            this.gui_balance.AutoSize = true;
            this.gui_balance.Location = new Point(0x10, 0x1d);
            this.gui_balance.Name = "gui_balance";
            this.gui_balance.Size = new Size(150, 13);
            this.gui_balance.TabIndex = 0;
            this.gui_balance.Text = "Ваш баланс: %balance% руб.";
            this.lk_header.Controls.Add(this.gui_last_activity);
            this.lk_header.Controls.Add(this.gui_passchange_header);
            this.lk_header.Controls.Add(this.entry_grid);
            this.lk_header.Controls.Add(this.gui_lastip);
            this.lk_header.Controls.Add(this.gui_gamestatus);
            this.lk_header.Controls.Add(this.gui_balance);
            this.lk_header.Location = new Point(12, 12);
            this.lk_header.Name = "lk_header";
            this.lk_header.Size = new Size(710, 250);
            this.lk_header.TabIndex = 1;
            this.lk_header.TabStop = false;
            this.lk_header.Text = "lk_header";
            this.gui_last_activity.AutoSize = true;
            this.gui_last_activity.Location = new Point(0x15b, 12);
            this.gui_last_activity.Name = "gui_last_activity";
            this.gui_last_activity.Size = new Size(0x52, 13);
            this.gui_last_activity.TabIndex = 11;
            this.gui_last_activity.Text = "gui_last_activity";
            this.gui_passchange_header.Controls.Add(this.show_pass_Pic);
            this.gui_passchange_header.Controls.Add(this.btn_change_pass);
            this.gui_passchange_header.Controls.Add(this.gui_retrynewpass);
            this.gui_passchange_header.Controls.Add(this.gui_retry_new_pass);
            this.gui_passchange_header.Controls.Add(this.gui_newpassword);
            this.gui_passchange_header.Controls.Add(this.gui_old_pass);
            this.gui_passchange_header.Controls.Add(this.gui_lastpassword);
            this.gui_passchange_header.Controls.Add(this.gui_new_pass);
            this.gui_passchange_header.Location = new Point(0x13, 0x6d);
            this.gui_passchange_header.Name = "gui_passchange_header";
            this.gui_passchange_header.Size = new Size(0x11e, 0x7b);
            this.gui_passchange_header.TabIndex = 10;
            this.gui_passchange_header.TabStop = false;
            this.gui_passchange_header.Text = "Смена пароля";
            this.show_pass_Pic.Cursor = Cursors.Hand;
            this.show_pass_Pic.Image = (Image) manager.GetObject("show_pass_Pic.Image");
            this.show_pass_Pic.Location = new Point(0xfd, 13);
            this.show_pass_Pic.Name = "show_pass_Pic";
            this.show_pass_Pic.Size = new Size(0x16, 0x16);
            this.show_pass_Pic.TabIndex = 12;
            this.show_pass_Pic.TabStop = false;
            this.show_pass_Pic.MouseEnter += new EventHandler(this.show_pass_Pic_MouseEnter);
            this.show_pass_Pic.MouseLeave += new EventHandler(this.show_pass_Pic_MouseLeave);
            this.btn_change_pass.Location = new Point(0x93, 0x5c);
            this.btn_change_pass.Name = "btn_change_pass";
            this.btn_change_pass.Size = new Size(0x86, 0x18);
            this.btn_change_pass.TabIndex = 5;
            this.btn_change_pass.Text = "Сменить пароль";
            this.btn_change_pass.UseVisualStyleBackColor = true;
            this.btn_change_pass.Click += new EventHandler(this.btn_change_pass_Click);
            this.gui_retrynewpass.AutoSize = true;
            this.gui_retrynewpass.Location = new Point(6, 70);
            this.gui_retrynewpass.Name = "gui_retrynewpass";
            this.gui_retrynewpass.Size = new Size(0x6a, 13);
            this.gui_retrynewpass.TabIndex = 11;
            this.gui_retrynewpass.Text = "Повторите пароль: ";
            this.gui_retry_new_pass.Location = new Point(0x73, 0x43);
            this.gui_retry_new_pass.MaxLength = 20;
            this.gui_retry_new_pass.Name = "gui_retry_new_pass";
            this.gui_retry_new_pass.Size = new Size(0x84, 20);
            this.gui_retry_new_pass.TabIndex = 10;
            this.gui_retry_new_pass.UseSystemPasswordChar = true;
            this.gui_newpassword.AutoSize = true;
            this.gui_newpassword.Location = new Point(6, 0x2c);
            this.gui_newpassword.Name = "gui_newpassword";
            this.gui_newpassword.Size = new Size(0x56, 13);
            this.gui_newpassword.TabIndex = 9;
            this.gui_newpassword.Text = "Новый пароль: ";
            this.gui_old_pass.Location = new Point(0x63, 15);
            this.gui_old_pass.MaxLength = 20;
            this.gui_old_pass.Name = "gui_old_pass";
            this.gui_old_pass.Size = new Size(0x94, 20);
            this.gui_old_pass.TabIndex = 6;
            this.gui_old_pass.UseSystemPasswordChar = true;
            this.gui_lastpassword.AutoSize = true;
            this.gui_lastpassword.Location = new Point(6, 0x12);
            this.gui_lastpassword.Name = "gui_lastpassword";
            this.gui_lastpassword.Size = new Size(90, 13);
            this.gui_lastpassword.TabIndex = 8;
            this.gui_lastpassword.Text = "Старый пароль: ";
            this.gui_new_pass.Location = new Point(0x5d, 0x29);
            this.gui_new_pass.MaxLength = 20;
            this.gui_new_pass.Name = "gui_new_pass";
            this.gui_new_pass.Size = new Size(0x9a, 20);
            this.gui_new_pass.TabIndex = 7;
            this.gui_new_pass.UseSystemPasswordChar = true;
            this.entry_grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            DataGridViewColumn[] dataGridViewColumns = new DataGridViewColumn[] { this.grid_server, this.grid_date, this.grid_ip };
            this.entry_grid.Columns.AddRange(dataGridViewColumns);
            this.entry_grid.Location = new Point(0x158, 0x1d);
            this.entry_grid.Name = "entry_grid";
            this.entry_grid.RowHeadersWidth = 4;
            this.entry_grid.Size = new Size(0x155, 0xcb);
            this.entry_grid.TabIndex = 4;
            this.grid_server.HeaderText = "Server";
            this.grid_server.Name = "grid_server";
            this.grid_date.HeaderText = "Date";
            this.grid_date.Name = "grid_date";
            this.grid_date.Width = 0x87;
            this.grid_ip.HeaderText = "Ip";
            this.grid_ip.Name = "grid_ip";
            this.gui_lastip.AutoSize = true;
            this.gui_lastip.Location = new Point(0x10, 0x52);
            this.gui_lastip.Name = "gui_lastip";
            this.gui_lastip.Size = new Size(0x7a, 13);
            this.gui_lastip.TabIndex = 3;
            this.gui_lastip.Text = "Последний айпи: %IP%";
            this.gui_gamestatus.AutoSize = true;
            this.gui_gamestatus.Location = new Point(0x10, 0x38);
            this.gui_gamestatus.Name = "gui_gamestatus";
            this.gui_gamestatus.Size = new Size(0xd6, 13);
            this.gui_gamestatus.TabIndex = 2;
            this.gui_gamestatus.Text = "Ваш аккаунт в игре на сервере %server%";
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x2de, 0x112);
            base.Controls.Add(this.lk_header);
            base.Icon = (Icon) manager.GetObject("$this.Icon");
            this.MaximumSize = new Size(750, 0x139);
            this.MinimumSize = new Size(0x29a, 0x139);
            base.Name = "LKForm";
            this.Text = "P&N Launcher";
            base.FormClosed += new FormClosedEventHandler(this.LKForm_FormClosed);
            this.lk_header.ResumeLayout(false);
            this.lk_header.PerformLayout();
            this.gui_passchange_header.ResumeLayout(false);
            this.gui_passchange_header.PerformLayout();
            ((ISupportInitialize) this.show_pass_Pic).EndInit();
            ((ISupportInitialize) this.entry_grid).EndInit();
            base.ResumeLayout(false);
        }

        private void LKForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Active = false;
        }

        private void show_pass_Pic_MouseEnter(object sender, EventArgs e)
        {
            this.gui_old_pass.UseSystemPasswordChar = false;
            this.gui_new_pass.UseSystemPasswordChar = false;
            this.gui_retry_new_pass.UseSystemPasswordChar = false;
        }

        private void show_pass_Pic_MouseLeave(object sender, EventArgs e)
        {
            this.gui_old_pass.UseSystemPasswordChar = true;
            this.gui_new_pass.UseSystemPasswordChar = true;
            this.gui_retry_new_pass.UseSystemPasswordChar = true;
        }
    }
}

