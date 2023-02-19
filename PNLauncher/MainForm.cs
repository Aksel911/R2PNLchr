namespace PNLauncher
{
    using Launcher;
    using Launcher.Core;
    using Newtonsoft.Json;
    using PNLauncher.Core;
    using PNLauncher.Help;
    using PNLauncher.Languages;
    using PNLauncher.Network;
    using PNLauncher.Structs;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using WFast.Networking;

    public class MainForm : Form
    {
        public static Dictionary<ERunTime, CRunTime> mRunTimeList = new Dictionary<ERunTime, CRunTime>();
        private AuthForm _AuthForm;
        private LKForm _LKForm;
        private static UpdateStatus _current_update_status = UpdateStatus.StartUpdate;
        private string _current_update_info = "";
        public static CRunTime mRunTime = null;
        public static bool IsPaused = false;
        private static Thread _update_thread = null;
        public static bool UpdateIsProcess = false;
        public static WebClient Client = new WebClient();
        public static long TotalBytes = 0L;
        public static long DownloadBytes = 0L;
        public static DateTime startUpdate;
        public static bool AllThreads = false;
        public static bool mIsIgnoreLauncherUpdate = false;
        public static string LoginIP = "";
        public static int LoginPort = 0;
        public static bool LoginStatus = false;
        public static bool ContentInited = false;
        public List<Server> ServerList = new List<Server>();
        public static bool needUpdateRestart = false;
        public Dictionary<string, string> ServersList = new Dictionary<string, string>();
        public byte[] ServersHash;
        public bool start_try_ended = true;
        private int currentIdx;
        private int filesToUpdate;
        private UpdateFile currentfile;
        private static bool _isAdminLauncher = false;
        private WebClient downClient = new WebClient();
        private static DateTime lastProgressNotify = new DateTime();
        private IContainer components;
        private GroupBox update_group;
        private Label gui_progress;
        private ProgressBar p_update_progress;
        private Button btn_start;
        private Label gui_currentver;
        private Label gui_lastver;
        private Label gui_setlang;
        private ComboBox combo_language;
        private GroupBox servers_group;
        private Label ls_status;
        private Label gui_ls;
        private Button btn_update_servers;
        private Button btn_play;
        private Button btn_settings;
        private Button btn_register;
        private Button btn_support;
        private Button btn_news;
        private Label gui_alterserver;
        private Label gui_auth_status;
        private LinkLabel btn_auth;
        private LinkLabel btn_logout;
        private CheckBox btn_all_threads;
        private Button btn_repair;
        private Label gui_gc_status;
        private LinkLabel flush_settings_btn;
        private ComboBox selected_server;
        private LinkLabel mirror_link;
        private LinkLabel link_chng_runtime;
        private LinkLabel lnk_cancel_update;

        public MainForm(bool is_admin, bool is_ignore_launcher_update)
        {
            this.InitializeComponent();
            mIsIgnoreLauncherUpdate = is_ignore_launcher_update;
            _isAdminLauncher = is_admin;
        }

        private void _update()
        {
            try
            {
                int count = mRunTime.mClientFiles.Count;
                TotalBytes = 0L;
                MD5Calculator.CheckedFiles = 0;
                MD5Calculator.currentIndex = 0;
                MD5Calculator.FilesToUpdate = mRunTime.mClientFiles.ToArray();
                MD5Calculator.syncObject = new object();
                MD5Calculator.Threads = AllThreads ? Environment.ProcessorCount : 1;
                MD5Calculator.WrongFiles = mRunTime.mUpdateFiles;
                MD5Calculator.ThreadsSuccess = 0;
                MD5Calculator.Start();
                while (true)
                {
                    if (MD5Calculator.Success())
                    {
                        TotalBytes = MD5Calculator.TotalBytes;
                        FileCache.SaveCache();
                        int num = mRunTime.mUpdateFiles.Count;
                        if (UpdateIsProcess)
                        {
                            this.SetUpdateInfo(UpdateStatus.CheckSuccess, num.ToString());
                        }
                        if (num == 0)
                        {
                            MessageBox.Show(LangController.GetTranslate(LangController.GetUpdateTagByStatus(UpdateStatus.LastVersion)));
                            this.end_update();
                        }
                        else
                        {
                            startUpdate = DateTime.Now;
                            this.downClient = new WebClient();
                            this.downClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(this.Update_File_Progress_Changed);
                            this.downClient.DownloadFileCompleted += new AsyncCompletedEventHandler(this.DownClient_DownloadFileCompleted);
                            DownloadBytes = 0L;
                            this.currentIdx = 0;
                            this.filesToUpdate = num;
                            this.dwnlFile(mRunTime.mUpdateFiles[0]);
                        }
                        break;
                    }
                    this.SetUpdateInfo(UpdateStatus.CheckFiles, $"[{MD5Calculator.CheckedFiles}/{MD5Calculator.lostIndex}]");
                    Thread.Sleep(0x7d);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("[UPDATE FATAL ERROR]: " + exception.ToString());
                Environment.Exit(-1);
            }
        }

        private void auth_btn_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!GameCoordinator.Connected)
            {
                this.ShowError(LangController.GetTranslate("msg_gamecoord_offline"));
            }
            else if (GameCoordinator.Authorized)
            {
                GameCoordinator.Send(new Packet(PacketIds.client_open_profile, 0));
            }
            else
            {
                if (this._AuthForm.Active)
                {
                    this._AuthForm.Close();
                }
                this._AuthForm.Active = true;
                this._AuthForm.ShowDialog();
            }
        }

        private void btn_all_threads_CheckedChanged(object sender, EventArgs e)
        {
            AllThreads = this.btn_all_threads.Checked;
            if (AllThreads)
            {
                this.ShowInfo(LangController.GetTranslate("all_threads_warning"));
            }
            this.UpdateGuiLanguage();
        }

        private void btn_logout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Logout();
        }

        private void btn_news_Click(object sender, EventArgs e)
        {
            Process.Start(Config.NewsURL);
        }

        private void btn_play_Click(object sender, EventArgs e)
        {
            if ((mRunTime.mRunTimeType == ERunTime.Reborn) && !System.IO.File.Exists(mRunTime.GetFile("r2.exe")))
            {
                MessageBox.Show("File r2.exe not found.");
            }
            else if (!System.IO.File.Exists(mRunTime.GetFile("R2.cfg")))
            {
                if (!System.IO.File.Exists(mRunTime.GetFile("r2option.exe")))
                {
                    MessageBox.Show("сfg error");
                }
                else
                {
                    StartOption();
                }
            }
            else if (System.IO.File.ReadAllText(mRunTime.GetFile("R2.cfg")).Contains("127.0.0.1") && (mRunTime.mRunTimeType != ERunTime.Reborn))
            {
                System.IO.File.Delete(mRunTime.GetFile("R2.cfg"));
                StartOption();
            }
            else if (!this.start_try_ended)
            {
                MessageBox.Show("Game already started");
            }
            else
            {
                string str = null;
                if (!this.ServersList.TryGetValue(this.selected_server.Text, out str))
                {
                    MessageBox.Show("Wrong server");
                }
                else
                {
                    this.saveLastUsedServer(this.selected_server.Text);
                    try
                    {
                        new Thread(new ParameterizedThreadStart(this.ExecuteGame)).Start(new _startParams(str));
                    }
                    catch (Exception exception1)
                    {
                        MessageBox.Show(exception1.ToString());
                    }
                }
            }
        }

        private void btn_register_Click(object sender, EventArgs e)
        {
            Process.Start(Config.RegisterURL);
        }

        private void btn_repair_Click(object sender, EventArgs e)
        {
            if (mRunTime.mClientFiles.Count == 0)
            {
                this.GetFilesList();
            }
            if (mRunTime.mClientFiles.Count == 0)
            {
                this.ShowError(LangController.GetTranslate("update_error"));
            }
            else
            {
                this.btn_start.Enabled = false;
                this.btn_repair.Enabled = false;
                IsPaused = false;
                UpdateIsProcess = true;
                this.UpdateGuiLanguage();
                FileCache.FlushCache(false);
                _update_thread = new Thread(new ThreadStart(this._update));
                _update_thread.IsBackground = true;
                _update_thread.Priority = ThreadPriority.AboveNormal;
                _update_thread.Start();
            }
        }

        private void btn_settings_Click(object sender, EventArgs e)
        {
            StartOption();
        }

        private void btn_start_Click(object sender, EventArgs e)
        {
            if (mRunTime.mClientFiles.Count == 0)
            {
                this.GetFilesList();
            }
            if (mRunTime.mClientFiles.Count == 0)
            {
                this.ShowError(LangController.GetTranslate("update_error"));
            }
            else
            {
                this.btn_start.Enabled = false;
                this.btn_repair.Enabled = false;
                IsPaused = false;
                UpdateIsProcess = true;
                this.UpdateGuiLanguage();
                _update_thread = new Thread(new ThreadStart(this._update));
                _update_thread.IsBackground = true;
                _update_thread.Priority = ThreadPriority.AboveNormal;
                _update_thread.Start();
            }
        }

        private void btn_support_Click(object sender, EventArgs e)
        {
            Process.Start(Config.SupportURL);
        }

        private void btn_update_servers_Click(object sender, EventArgs e)
        {
            this.update_info();
        }

        private Tuple<int, int> CalcPacketsStat(string[] pInLog)
        {
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < pInLog.Length; i++)
            {
                if (pInLog[i].Contains("Server -> Client"))
                {
                    num2++;
                }
                else if (pInLog[i].Contains("Client -> Server"))
                {
                    num++;
                }
            }
            return new Tuple<int, int>(num, num2);
        }

        private void combo_language_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void combo_language_SelectedValueChanged(object sender, EventArgs e)
        {
            string text = this.combo_language.Text;
            if (!LangController.LangExists(text))
            {
                this.ShowError(LangController.GetTranslate("msg_langnotfound"));
            }
            else
            {
                LangController.SetLanguage(text);
                this.InitGuiLanguage();
            }
        }

        private string ComputeMD5Checksum(string path)
        {
            using (FileStream stream = System.IO.File.OpenRead(path))
            {
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, (int) stream.Length);
                return BitConverter.ToString(new MD5CryptoServiceProvider().ComputeHash(buffer)).Replace("-", string.Empty).ToLower();
            }
        }

        private void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            int num2;
            string[] strArray2 = Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories);
            for (num2 = 0; num2 < strArray2.Length; num2++)
            {
                Directory.CreateDirectory(strArray2[num2].Replace(sourcePath, targetPath));
            }
            string[] strArray = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
            int num = 0;
            strArray2 = strArray;
            for (num2 = 0; num2 < strArray2.Length; num2++)
            {
                string sourceFileName = strArray2[num2];
                System.IO.File.Copy(sourceFileName, sourceFileName.Replace(sourcePath, targetPath), true);
                this.SetUpdateInfo(UpdateStatus.CopyFiles, $"{++num}/{strArray.Length}");
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

        private static void doLauncherUpdate()
        {
            if (System.IO.File.Exists("Launcher.update"))
            {
                System.IO.File.WriteAllText("update.bat", "\r\n                echo Updateing launcher... Wait 2 second\r\n\r\n                @echo off\r\n\r\n                timeout 1 > nul\r\n\r\n                del Launcher.exe\r\n                ren Launcher.update Launcher.exe\r\n                \r\n                start Launcher.exe\r\n                del update.bat\r\n                ");
                Process.Start("update.bat");
                Environment.Exit(0);
            }
        }

        private void DownClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            int num2 = this.currentIdx + 1;
            this.currentIdx = num2;
            int num = num2;
            DownloadBytes += this.currentfile.Size;
            if (num >= this.filesToUpdate)
            {
                this.end_update();
            }
            else
            {
                this.dwnlFile(mRunTime.mUpdateFiles[num]);
            }
        }

        private void dwnlFile(UpdateFile file)
        {
            this.currentfile = file;
            string name = file.Name;
            if (name.Contains("Launcher.exe"))
            {
                name = name.Replace("Launcher.exe", "Launcher.update");
                needUpdateRestart = true;
            }
            this.downClient.DownloadFileTaskAsync(mRunTime.GetDownloadFile(file.NetPath), name);
        }

        private void end_update()
        {
            if (base.InvokeRequired)
            {
                base.Invoke(delegate {
                    this.btn_start.Enabled = true;
                    this.btn_repair.Enabled = true;
                });
            }
            else
            {
                this.btn_start.Enabled = true;
                this.btn_repair.Enabled = true;
            }
            IsPaused = false;
            foreach (UpdateFile file in mRunTime.mUpdateFiles)
            {
                if (FileCache.CacheList.ContainsKey(file.Name))
                {
                    FileCache.CacheList.Remove(file.Name);
                }
                FileCache.CacheItem itm = new FileCache.CacheItem {
                    MD5 = file.MD5Hash,
                    Path = file.Name,
                    Size = file.Size,
                    writeTime = new FileInfo(file.Name).LastWriteTime.ToBinary()
                };
                FileCache.AddInCache(itm);
            }
            FileCache.SaveCache();
            this.SetUpdateInfo(UpdateStatus.UpdateSuccess, "");
            mRunTime.mUpdateFiles.Clear();
            mRunTime.mClientFiles.Clear();
            mRunTime.SuccessUpdate();
            this.currentfile = null;
            UpdateIsProcess = false;
            this.UpdateGuiLanguage();
            if (needUpdateRestart)
            {
                doLauncherUpdate();
            }
        }

        private void ExecuteGame(object _in)
        {
            _startParams @params = (_startParams) _in;
            try
            {
                Process process2;
                string str6;
                this.start_try_ended = false;
                if (mRunTime.mRunTimeType != ERunTime.Main)
                {
                    if (mRunTime.mRunTimeType != ERunTime.Reborn)
                    {
                        if (mRunTime.mRunTimeType == ERunTime.PTS)
                        {
                            string str7 = "\"hater|ne|proidet\" 1251 936 949 228 1337 1488" + (mRunTime.mHaveParam ? (" " + mRunTime.mStartParam) : string.Empty);
                            try
                            {
                                string path = Path.Combine(mRunTime.GetDirectory(), "set_to_dev");
                                if (@params.game_name == "admin_patch")
                                {
                                    System.IO.File.Create(path).Close();
                                }
                                else if (System.IO.File.Exists(path))
                                {
                                    System.IO.File.Delete(path);
                                }
                            }
                            catch
                            {
                            }
                            if (System.IO.File.Exists(mRunTimeList[ERunTime.Main].GetFile("launcher.auth")))
                            {
                                System.IO.File.Copy(mRunTimeList[ERunTime.Main].GetFile("launcher.auth"), mRunTime.GetFile("launcher.auth"), true);
                            }
                            if (System.IO.File.Exists(mRunTimeList[ERunTime.Main].GetFile("lang/lang.default")))
                            {
                                System.IO.File.Copy(mRunTimeList[ERunTime.Main].GetFile("lang/lang.default"), mRunTime.GetFile("lang/lang.default"), true);
                            }
                            ProcessStartInfo info4 = new ProcessStartInfo();
                            info4.FileName = mRunTime.GetFile("PNGame.exe");
                            info4.Arguments = str7;
                            info4.WorkingDirectory = mRunTime.GetDirectory();
                            ProcessStartInfo startInfo = info4;
                            Process process3 = Process.Start(startInfo);
                            this.start_try_ended = true;
                            process3.WaitForExit();
                            if ((process3.ExitCode != 0) && (process3.ExitCode != 1))
                            {
                                string str9 = string.Empty;
                                bool flag2 = false;
                                switch (process3.ExitCode)
                                {
                                    case -13:
                                        str9 = "Encoding error";
                                        break;

                                    case -12:
                                        str9 = "Locale error";
                                        break;

                                    case -11:
                                        str9 = "Authorize error";
                                        break;

                                    case -10:
                                        str9 = "Error Init data";
                                        break;

                                    default:
                                        flag2 = this.SendCrashReport(ERunTime.PTS, startInfo.FileName, startInfo.Arguments, process3.ExitCode, @params.game_name);
                                        break;
                                }
                                if (!flag2)
                                {
                                    MessageBox.Show($"GameError[{@params.game_name}] => [{process3.ExitCode}) [{str9}]]");
                                }
                            }
                        }
                        return;
                    }
                    else
                    {
                        string file = mRunTime.GetFile("r2.exe");
                        string str5 = $"PNGame.exe {LoginIP} {LoginPort} {LangController.GetCodeId} {@params.game_name}";
                        if ((@params.game_name == "admin_patch") || (@params.game_name == "admin_no_patch"))
                        {
                            file = mRunTime.GetFile("r2_admin.exe");
                            str5 = $"PNGame.exe 127.0.0.1 31250 {LangController.GetCodeId} Rsh" + ((@params.game_name == "admin_no_patch") ? " -no_patch" : "");
                        }
                        if (System.IO.File.Exists(mRunTimeList[ERunTime.Main].GetFile("launcher.auth")))
                        {
                            System.IO.File.Copy(mRunTimeList[ERunTime.Main].GetFile("launcher.auth"), mRunTime.GetFile("launcher.auth"), true);
                        }
                        if (System.IO.File.Exists(mRunTimeList[ERunTime.Main].GetFile("lang/lang.default")))
                        {
                            System.IO.File.Copy(mRunTimeList[ERunTime.Main].GetFile("lang/lang.default"), mRunTime.GetFile("lang/lang.default"), true);
                        }
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.FileName = file;
                        startInfo.Arguments = str5;
                        startInfo.WorkingDirectory = mRunTime.GetDirectory();
                        process2 = Process.Start(startInfo);
                        process2.WaitForExit();
                        if (process2.ExitCode == 0)
                        {
                            return;
                        }
                        else if (process2.ExitCode == 3)
                        {
                            return;
                        }
                        else
                        {
                            int exitCode = process2.ExitCode;
                            if (exitCode > 0x2c)
                            {
                                if (exitCode > 0x4c)
                                {
                                    if (exitCode > 0x90)
                                    {
                                        if (exitCode == 0x91)
                                        {
                                            str6 = "Auth file corrupted";
                                            goto TR_0016;
                                        }
                                        else if (exitCode == 0x9f)
                                        {
                                            str6 = "Game file not found";
                                            goto TR_0016;
                                        }
                                        else if (exitCode == 0xfe)
                                        {
                                            str6 = "Error initialize (2)";
                                            goto TR_0016;
                                        }
                                    }
                                    else
                                    {
                                        switch (exitCode)
                                        {
                                            case 0x76:
                                                str6 = "Wrong port";
                                                goto TR_0016;

                                            case 0x77:
                                                str6 = "Error accept";
                                                goto TR_0016;

                                            case 120:
                                                str6 = "Wrong socket";
                                                goto TR_0016;

                                            default:
                                                if (exitCode != 0x90)
                                                {
                                                    break;
                                                }
                                                str6 = "Auth file not found";
                                                goto TR_0016;
                                        }
                                    }
                                }
                                else if (exitCode > 0x40)
                                {
                                    if (exitCode == 0x41)
                                    {
                                        str6 = "CFG file not found";
                                        goto TR_0016;
                                    }
                                    else if (exitCode == 0x4c)
                                    {
                                        str6 = "Error accept (game)";
                                        goto TR_0016;
                                    }
                                }
                                else if (exitCode == 60)
                                {
                                    str6 = "Error start listen (client)";
                                    goto TR_0016;
                                }
                                else if (exitCode == 0x40)
                                {
                                    str6 = "Wrong login port";
                                    goto TR_0016;
                                }
                            }
                            else if (exitCode > 1)
                            {
                                if (exitCode > 11)
                                {
                                    if (exitCode == 0x19)
                                    {
                                        str6 = "Error create process";
                                        goto TR_0016;
                                    }
                                    else if (exitCode == 0x1c)
                                    {
                                        str6 = "Error make socket (client)";
                                        goto TR_0016;
                                    }
                                    else if (exitCode == 0x2c)
                                    {
                                        str6 = "Error bind socket (client)";
                                        goto TR_0016;
                                    }
                                }
                                else if (exitCode == 10)
                                {
                                    str6 = "Error initialize";
                                    goto TR_0016;
                                }
                                else if (exitCode == 11)
                                {
                                    str6 = "Error make socket (server)";
                                    goto TR_0016;
                                }
                            }
                            else if (exitCode > -44)
                            {
                                if (exitCode == -28)
                                {
                                    str6 = "Error make socket (server)";
                                    goto TR_0016;
                                }
                                else if (exitCode == 1)
                                {
                                    str6 = "Wrong version";
                                    goto TR_0016;
                                }
                            }
                            else if (exitCode == -60)
                            {
                                str6 = "Error start listen (server)";
                                goto TR_0016;
                            }
                            else if (exitCode == -44)
                            {
                                str6 = "Error bind socket (server)";
                                goto TR_0016;
                            }
                            str6 = "Unknown";
                        }
                    }
                }
                else
                {
                    string str = "\"hater|ne|proidet\" 1251 936 949 228 1337 1488" + (mRunTime.mHaveParam ? (" " + mRunTime.mStartParam) : string.Empty);
                    try
                    {
                        string path = Path.Combine(mRunTime.GetDirectory(), "set_to_dev");
                        if (@params.game_name == "admin_patch")
                        {
                            System.IO.File.Create(path).Close();
                        }
                        else if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                        }
                    }
                    catch
                    {
                    }
                    ProcessStartInfo info1 = new ProcessStartInfo();
                    info1.FileName = mRunTime.GetFile("PNGame.exe");
                    info1.Arguments = str;
                    info1.WorkingDirectory = mRunTime.GetDirectory();
                    ProcessStartInfo startInfo = info1;
                    Process process = Process.Start(startInfo);
                    this.start_try_ended = true;
                    process.WaitForExit();
                    if ((process.ExitCode != 0) && (process.ExitCode != 1))
                    {
                        string str3 = string.Empty;
                        bool flag = false;
                        switch (process.ExitCode)
                        {
                            case -13:
                                str3 = "Encoding error";
                                break;

                            case -12:
                                str3 = "Locale error";
                                break;

                            case -11:
                                str3 = "Authorize error";
                                break;

                            case -10:
                                str3 = "Error Init data";
                                break;

                            default:
                                flag = this.SendCrashReport(ERunTime.Main, startInfo.FileName, startInfo.Arguments, process.ExitCode, @params.game_name);
                                break;
                        }
                        if (!flag)
                        {
                            MessageBox.Show($"GameError[{@params.game_name}] => [{process.ExitCode}) [{str3}]]");
                        }
                    }
                    return;
                }
            TR_0016:
                MessageBox.Show($"GameError[{@params.game_name}] => [{process2.ExitCode}) [{str6}]]");
            }
            catch (Exception exception)
            {
                string path = $"error_{Environment.TickCount}.txt";
                System.IO.File.WriteAllText(path, @params.game_name + ":" + exception.ToString());
                MessageBox.Show("Fatal error happend. All error-data writed in file [" + path + "]");
            }
            finally
            {
                this.start_try_ended = true;
            }
        }

        private Server find(string name)
        {
            int count = this.ServerList.Count;
            for (int i = 0; i < count; i++)
            {
                Server server = this.ServerList[i];
                if ((server != null) && (server.Name == name))
                {
                    return server;
                }
            }
            return null;
        }

        private void flush_settings_btn_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (MessageBox.Show(LangController.GetTranslate("flush_settings_warning").Replace("%new_line%", Environment.NewLine), "P&N Launcher", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                List<string> list = new List<string>();
                List<string> list2 = new List<string>();
                try
                {
                    if (Directory.Exists(mRunTime.GetFile("env")))
                    {
                        list.AddRange(Directory.GetFiles(mRunTime.GetFile("env")));
                        list2.AddRange(Directory.GetFiles(mRunTime.GetFile("env")));
                    }
                    if (Directory.Exists(mRunTime.GetFile("gmark")))
                    {
                        list.AddRange(Directory.GetFiles(mRunTime.GetFile("gmark")));
                        list2.AddRange(Directory.GetDirectories(mRunTime.GetFile("gmark")));
                    }
                }
                catch
                {
                    this.ShowError(LangController.GetTranslate("flush_settings_error"));
                }
                int num = 0;
                int num2 = 0;
                while (true)
                {
                    if (num2 >= list.Count)
                    {
                        int num3 = 0;
                        while (true)
                        {
                            if (num3 >= list2.Count)
                            {
                                if (num != 0)
                                {
                                    this.ShowError(LangController.GetTranslate("flush_settings_success_with_errors").Replace("%errors%", num.ToString()));
                                    break;
                                }
                                this.ShowInfo(LangController.GetTranslate("flush_settings_success"));
                                return;
                            }
                            try
                            {
                                Directory.Delete(list2[num3], true);
                            }
                            catch
                            {
                                num++;
                            }
                            num3++;
                        }
                        break;
                    }
                    try
                    {
                        FileInfo info = new FileInfo(list[num2]);
                        if (((info.Name != "readme.txt") && (info.Name != "pn.dat")) && (info.Name != "pn.log"))
                        {
                            info.Delete();
                        }
                    }
                    catch
                    {
                        num++;
                    }
                    num2++;
                }
            }
        }

        private void GCClient_OnConnectSuccess(IPAddress remote_ip, ushort remote_port, Exception e, string reason)
        {
            GameCoordinator.GCRTT = -1;
            this.UpdateGuiLanguage();
            if (System.IO.File.Exists("launcher.auth") && Auth.ReadKey(out GameCoordinator.AuthKey))
            {
                GameCoordinator.Send(new AuthKey(GameCoordinator.AuthKey));
            }
            Packet p = new Packet(PacketIds.client_servers_ways_info, 0x10);
            p.WriteBytes(this.ServersHash);
            GameCoordinator.Send(p);
            this.saveNewIP(remote_ip);
        }

        private void GCClient_OnDisconnect(IPAddress remote_ip, ushort remote_port, Exception e, string reason)
        {
            if (!GameCoordinator.Connected)
            {
                this.GCClient_OnErrorConnect(null, 0, null, null);
                Thread.Sleep(0x1388);
            }
            else
            {
                GameCoordinator.GCRTT = -1;
                GameCoordinator.Retry = 1;
                GameCoordinator.Authorized = false;
                GameCoordinator.AuthKey = new byte[0];
                GameCoordinator.Connected = false;
                GameCoordinator.Login = "";
                try
                {
                    if (base.InvokeRequired)
                    {
                        base.Invoke(delegate {
                            if (this._AuthForm.Active)
                            {
                                this._AuthForm.Close();
                            }
                            if (this._LKForm.Active)
                            {
                                this._LKForm.Close();
                            }
                        });
                    }
                    else
                    {
                        if (this._AuthForm.Active)
                        {
                            this._AuthForm.Close();
                        }
                        if (this._LKForm.Active)
                        {
                            this._LKForm.Close();
                        }
                    }
                    this.UpdateGuiLanguage();
                }
                catch
                {
                }
            }
        }

        private void GCClient_OnErrorConnect(IPAddress remote_ip, ushort remote_port, Exception e, string reason)
        {
            GameCoordinator.GCRTT = -1;
            GameCoordinator.Retry++;
            GameCoordinator.Connected = false;
            this.UpdateGuiLanguage();
        }

        private void GCClient_OnNewPacket(byte[] buffer)
        {
            try
            {
                this.HandlePacket(new Packet(buffer));
            }
            catch (Exception exception)
            {
                System.IO.File.AppendAllText("pn_launcher_error.log", $"[{DateTime.Now}]: error HandlePacket({FileCache.byteArrayToString(buffer, BitConverter.ToInt32(buffer, 0))}) => {exception.ToString()}");
            }
        }

        public void GetFilesList()
        {
            if (mRunTime.mHaveDownloadURL)
            {
                mRunTime.mClientFiles.Clear();
                char[] separator = new char[] { '\n' };
                string[] strArray = Client.DownloadString(mRunTime.GetFilesSumURL()).Split(separator);
                int length = strArray.Length;
                for (int i = 0; i < length; i++)
                {
                    string str = strArray[i];
                    if (!string.IsNullOrEmpty(str))
                    {
                        char[] chArray2 = new char[] { '\t' };
                        string[] strArray2 = str.Split(chArray2);
                        string pFileName = strArray2[0];
                        if (mRunTime.mRunTimeType == ERunTime.PTS)
                        {
                            pFileName = pFileName.Replace("pts/", "").Replace("/", @"\");
                        }
                        if (mRunTime.mRunTimeType == ERunTime.Reborn)
                        {
                            pFileName = pFileName.Replace("reborn/", "").Replace("/", @"\");
                        }
                        UpdateFile item = new UpdateFile(mRunTime.GetFile(pFileName), strArray2[1], int.Parse(strArray2[2]), strArray2[0]);
                        mRunTime.mClientFiles.Add(item);
                    }
                }
                mRunTime.LoadLastVersion();
                if (LastVersion == CurrentVersion)
                {
                    this.SetUpdateInfo(UpdateStatus.LastVersion, LastVersion.ToString());
                }
                else
                {
                    this.ShowInfo(LangController.GetTranslate("update_client_old"));
                    this.SetUpdateInfo(UpdateStatus.StartUpdate, "");
                }
            }
        }

        public static string GetPrevDirectory() => 
            Path.GetDirectoryName(Environment.CurrentDirectory);

        public void Gui_SetCurrentVersion()
        {
            this.gui_currentver.Text = LangController.GetTranslate("gui_currentver") + ": " + ((CurrentVersion == -1) ? "-" : CurrentVersion.ToString());
        }

        public void Gui_SetLastVersion()
        {
            string[] textArray1 = new string[6];
            textArray1[0] = LangController.GetTranslate("gui_lastver");
            textArray1[1] = ": ";
            textArray1[2] = (LastVersion == -1) ? "-" : LastVersion.ToString();
            string[] local1 = textArray1;
            local1[3] = " (";
            local1[4] = mRunTime.mRunTimeType.ToString();
            local1[5] = ")";
            this.gui_lastver.Text = string.Concat(local1);
        }

        public void HandlePacket(Packet p)
        {
            PacketIds iD = p.ID;
            if (iD > PacketIds.server_auth_result)
            {
                if (iD > PacketIds.server_message)
                {
                    if (iD == PacketIds.server_disconnect)
                    {
                        GameCoordinator.Close();
                    }
                    else if (iD == PacketIds.server_show_message)
                    {
                        int count = p.ReadInt32();
                        string str4 = Encoding.UTF8.GetString(p.ReadBytes(count));
                        if (p.ReadByte() == 1)
                        {
                            this.ShowError(LangController.GetTranslate(str4));
                        }
                        else
                        {
                            this.ShowInfo(LangController.GetTranslate(str4));
                        }
                    }
                    else if (iD == PacketIds.server_rtt_packet)
                    {
                        int num3 = p.ReadInt32();
                        GameCoordinator.GCRTT = (int) Math.Floor((double) (Environment.TickCount - num3));
                        GameCoordinator.Connected = true;
                        GameCoordinator.Retry = 1;
                        this.UpdateGuiLanguage();
                    }
                }
                else if (iD == PacketIds.server_profile_info)
                {
                    byte[] bytes = p.ReadBytes(p.ReadInt32());
                    string str3 = Encoding.UTF8.GetString(bytes);
                    this._LKForm.Info = (bytes.Length != 0) ? JsonConvert.DeserializeObject<ProfileInfo>(str3) : null;
                    new Thread(() => base.Invoke(delegate {
                        if ((this._LKForm.Info != null) && (this._LKForm.Info.account_info != null))
                        {
                            this._LKForm.Active = true;
                            this.UpdateGuiLanguage();
                            this._LKForm.ShowDialog();
                        }
                    })).Start();
                }
                else if (iD != PacketIds.server_result_change_pass)
                {
                    if (iD == PacketIds.server_message)
                    {
                        int num7 = p.ReadInt32();
                        this.ShowInfo(LangController.GetTranslate($"msg_[{num7}]"));
                    }
                }
                else
                {
                    this.Logout();
                    if (p.ReadByte() != 1)
                    {
                        this.ShowError(LangController.GetTranslate("error_change_password"));
                    }
                    else
                    {
                        int count = p.ReadInt32();
                        GameCoordinator.Send(new AuthKey(p.ReadBytes(count)));
                        this.ShowInfo(LangController.GetTranslate("success_change_password"));
                    }
                    base.Invoke(delegate {
                        if (this._LKForm.Active)
                        {
                            this._LKForm.Close();
                        }
                    });
                }
            }
            else if (iD > PacketIds.server_urls_info)
            {
                if (iD == PacketIds.server_ls_info)
                {
                    LoginStatus = p.ReadByte() == 1;
                    LoginIP = $"{p.ReadByte()}.{p.ReadByte()}.{p.ReadByte()}.{p.ReadByte()}";
                    LoginPort = p.ReadInt16();
                    this.UpdateGuiLanguage();
                }
                else if (iD == PacketIds.server_servers_ways_info)
                {
                    int count = p.ReadInt32();
                    byte[] bytes = p.ReadBytes(count);
                    string contents = Encoding.UTF8.GetString(bytes);
                    System.IO.File.WriteAllText("servers.list", contents);
                    this.loadServers();
                }
                else if (iD == PacketIds.server_auth_result)
                {
                    if (p.ReadByte() != 1)
                    {
                        this.Logout();
                    }
                    else
                    {
                        byte count = p.ReadByte();
                        GameCoordinator.AuthKey = p.ReadBytes(p.ReadInt32());
                        Auth.SaveKey(GameCoordinator.AuthKey);
                        GameCoordinator.Authorized = true;
                        GameCoordinator.Login = Encoding.UTF8.GetString(p.ReadBytes(count));
                        base.Invoke(delegate {
                            if (this._AuthForm.Active)
                            {
                                this._AuthForm.Close();
                            }
                        });
                        this.UpdateGuiLanguage();
                    }
                }
            }
            else if (iD == PacketIds.client_runtime_param)
            {
                CRunTime time2;
                ERunTime key = (ERunTime) p.ReadInt32();
                int count = p.ReadInt32();
                string pParam = Encoding.UTF8.GetString(p.ReadBytes(count));
                if (mRunTimeList.TryGetValue(key, out time2))
                {
                    time2.SetParam(pParam);
                }
            }
            else if (iD != PacketIds.server_servers_info)
            {
                if (iD == PacketIds.server_urls_info)
                {
                    ulong num8 = p.ReadUInt64();
                    ulong num9 = p.ReadUInt64();
                    byte[] bf = p.ReadBytes((int) num9);
                    ulong num10 = p.ReadUInt64();
                    ulong num11 = p.ReadUInt64();
                    byte[] buffer5 = p.ReadBytes((int) num11);
                    ulong num12 = p.ReadUInt64();
                    byte[] buffer6 = p.ReadBytes((int) num12);
                    ulong num13 = p.ReadUInt64();
                    byte[] buffer7 = p.ReadBytes((int) num13);
                    Config.SupportURL = p.ReadBytes((int) num8).AsString();
                    Config.RegisterURL = bf.AsString();
                    Config.NewsURL = p.ReadBytes((int) num10).AsString();
                    mRunTimeList[ERunTime.Main].SetDownloadURL(buffer5.AsString());
                    mRunTimeList[ERunTime.PTS].SetDownloadURL(buffer6.AsString());
                    mRunTimeList[ERunTime.Reborn].SetDownloadURL(buffer7.AsString());
                    if (!ContentInited)
                    {
                        this.GetFilesList();
                        ContentInited = true;
                        this.UpdateGuiLanguage();
                    }
                }
            }
            else if (this.ServerList.Count > 0)
            {
                int num14 = p.Tell();
                int size = p.Size;
                while (num14 < size)
                {
                    byte count = p.ReadByte();
                    string name = Encoding.GetEncoding(0x4e3).GetString(p.ReadBytes(count));
                    bool flag = p.ReadByte() == 1;
                    this.find(name).Status = flag;
                    num14 = p.Tell();
                }
                this.UpdateGuiLanguage();
            }
            else
            {
                int num17 = 1;
                Point location = this.gui_ls.Location;
                Point point2 = this.ls_status.Location;
                int num18 = p.Tell();
                int size = p.Size;
                p.GetBuffer();
                while (true)
                {
                    if (num18 >= size)
                    {
                        this.UpdateGuiLanguage();
                        break;
                    }
                    byte count = p.ReadByte();
                    string str6 = Encoding.GetEncoding(0x4e3).GetString(p.ReadBytes(count));
                    bool flag2 = p.ReadByte() == 1;
                    Server newServer = new Server {
                        Name = str6,
                        Status = flag2,
                        gui_name = new Label()
                    };
                    newServer.gui_name.Location = new Point(location.X, location.Y + (15 * num17));
                    newServer.gui_name.Size = new Size(0x53, 13);
                    newServer.gui_name.Text = str6;
                    base.Invoke(() => this.servers_group.Controls.Add(newServer.gui_name));
                    newServer.gui_status = new Label();
                    newServer.gui_status.Location = new Point(point2.X, point2.Y + (15 * num17));
                    newServer.gui_status.Size = new Size(70, 13);
                    newServer.gui_status.Text = "";
                    base.Invoke(() => this.servers_group.Controls.Add(newServer.gui_status));
                    this.ServerList.Add(newServer);
                    num17++;
                    num18 = p.Tell();
                }
            }
        }

        public void InitGuiLanguage()
        {
            if (!string.IsNullOrEmpty(LangController.CurrentLanguage))
            {
                this.Gui_SetLastVersion();
                this.Gui_SetCurrentVersion();
                this.link_chng_runtime.Text = LangController.GetTranslate("link_chng_runtime");
                this.lnk_cancel_update.Text = LangController.GetTranslate("rollback_lbl");
                this.gui_ls.Text = LangController.GetTranslate("login_server");
                if (!UpdateIsProcess)
                {
                    this.gui_progress.Text = LangController.GetTranslate("gui_progress");
                }
                this.btn_start.Text = (CurrentVersion == LastVersion) ? LangController.GetTranslate("btn_check_update") : LangController.GetTranslate("btn_update");
                this.btn_repair.Text = LangController.GetTranslate("btn_repair");
                this.btn_update_servers.Text = LangController.GetTranslate("btn_update");
                this.gui_setlang.Text = LangController.GetTranslate("gui_setlang") + ": ";
                this.btn_play.Text = LangController.GetTranslate("btn_play");
                this.btn_register.Text = LangController.GetTranslate("btn_register");
                this.btn_settings.Text = LangController.GetTranslate("btn_settings");
                this.btn_settings.Text = LangController.GetTranslate("btn_settings");
                this.btn_settings.Text = LangController.GetTranslate("btn_settings");
                this.btn_settings.Text = LangController.GetTranslate("btn_settings");
                this.btn_settings.Text = LangController.GetTranslate("btn_settings");
                this.btn_support.Text = LangController.GetTranslate("btn_support");
                this.btn_news.Text = LangController.GetTranslate("btn_news");
                this.flush_settings_btn.Text = LangController.GetTranslate("flush_settings_btn");
                if (!GameCoordinator.GCClient.Connected)
                {
                    this.gui_gc_status.Text = LangController.GetTranslate("gc_status_error").Replace("%retry%", GameCoordinator.Retry.ToString());
                    this.gui_gc_status.ForeColor = Color.Red;
                }
                else if (GameCoordinator.GCRTT == -1)
                {
                    this.gui_gc_status.Text = LangController.GetTranslate("gc_status_installing");
                    this.gui_gc_status.ForeColor = Color.FromName("ControlText");
                }
                else
                {
                    this.gui_gc_status.Text = LangController.GetTranslate("gc_status_success").Replace("%rtt%", GameCoordinator.GCRTT.ToString());
                    this.gui_gc_status.ForeColor = Color.Green;
                }
                this.btn_all_threads.Text = LangController.GetTranslate("btn_all_threads");
                this.btn_all_threads.Enabled = !UpdateIsProcess;
                this._AuthForm.gui_auth_login.Text = LangController.GetTranslate("lbl_login");
                this._AuthForm.gui_auth_password.Text = LangController.GetTranslate("lbl_password");
                if (!GameCoordinator.Authorized)
                {
                    this.btn_logout.Visible = false;
                    this.btn_auth.Text = LangController.GetTranslate("btn_auth");
                    this.gui_auth_status.ForeColor = Color.Red;
                    this.gui_auth_status.Text = LangController.GetTranslate("lbl_auth_status_not");
                }
                else
                {
                    this.btn_logout.Visible = true;
                    this.btn_logout.Text = LangController.GetTranslate("btn_logout");
                    this.btn_auth.Text = LangController.GetTranslate("btn_profile");
                    this.gui_auth_status.ForeColor = Color.FromName("ControlText");
                    this.gui_auth_status.Text = LangController.GetTranslate("lbl_auth_status_success").Replace("%login%", GameCoordinator.Login);
                }
                this.update_group.Text = LangController.GetTranslate("group_update");
                this.servers_group.Text = LangController.GetTranslate("g_servers");
                if (!GameCoordinator.Connected)
                {
                    this.ls_status.ForeColor = Color.Orange;
                    this.ls_status.Text = LangController.GetTranslate("serv_status_undefined");
                    this.mirror_link.Text = LangController.GetTranslate("lbl_mirror");
                    this.mirror_link.Visible = true;
                }
                else
                {
                    this.mirror_link.Visible = false;
                    if (LoginStatus)
                    {
                        this.ls_status.ForeColor = Color.Green;
                        this.ls_status.Text = LangController.GetTranslate("serv_status_online");
                    }
                    else
                    {
                        this.ls_status.ForeColor = Color.Red;
                        this.ls_status.Text = LangController.GetTranslate("serv_status_offline");
                    }
                }
                int count = this.ServerList.Count;
                for (int i = 0; i < count; i++)
                {
                    Server server = this.ServerList[i];
                    if (server != null)
                    {
                        if (!LoginStatus || !GameCoordinator.Connected)
                        {
                            server.gui_status.ForeColor = Color.Orange;
                            server.gui_status.Text = LangController.GetTranslate("serv_status_undefined");
                        }
                        else if (server.Status)
                        {
                            server.gui_status.ForeColor = Color.Green;
                            server.gui_status.Text = LangController.GetTranslate("serv_status_online");
                        }
                        else
                        {
                            server.gui_status.ForeColor = Color.Red;
                            server.gui_status.Text = LangController.GetTranslate("serv_status_offline");
                        }
                    }
                }
                if (((LastVersion != CurrentVersion) && GameCoordinator.Connected) || (!GameCoordinator.Authorized && GameCoordinator.Connected))
                {
                    this.btn_play.Enabled = false;
                }
                else
                {
                    this.btn_play.Enabled = true;
                }
                if (this._LKForm.Active)
                {
                    base.Invoke(delegate {
                        if ((this._LKForm.Info == null) || (this._LKForm.Info.account_info == null))
                        {
                            MessageBox.Show("error open profile");
                        }
                        else
                        {
                            this._LKForm.gui_lastpassword.Text = LangController.GetTranslate("lbl_last_password");
                            this._LKForm.gui_newpassword.Text = LangController.GetTranslate("lbl_new_password");
                            this._LKForm.gui_retrynewpass.Text = LangController.GetTranslate("lbl_new_password_retry");
                            this._LKForm.gui_balance.Text = LangController.GetTranslate("lbl_balance").Replace("%balance%", this._LKForm.Info.account_info.Balance.ToString());
                            if (this._LKForm.Info.account_info.IsOnline)
                            {
                                this._LKForm.gui_gamestatus.Text = LangController.GetTranslate("lbl_account_in_game").Replace("%server%", this._LKForm.Info.account_info.CurrentServer);
                                this._LKForm.gui_gamestatus.ForeColor = Color.Green;
                            }
                            else
                            {
                                this._LKForm.gui_gamestatus.Text = LangController.GetTranslate("lbl_account_not_in_game");
                                this._LKForm.gui_gamestatus.ForeColor = Color.FromName("ControlText");
                            }
                            this._LKForm.gui_lastip.Text = LangController.GetTranslate("lbl_last_ip").Replace("%IP%", this._LKForm.Info.account_info.LastIp);
                            this._LKForm.lk_header.Text = GameCoordinator.Login;
                            this._LKForm.gui_passchange_header.Text = LangController.GetTranslate("lbl_change_pass");
                            this._LKForm.btn_change_pass.Text = LangController.GetTranslate("btn_change_pass");
                            this._LKForm.gui_last_activity.Text = LangController.GetTranslate("lbl_last_activity");
                            this._LKForm.entry_grid.Rows.Clear();
                            if (this._LKForm.Info.activity != null)
                            {
                                for (int j = 0; j < this._LKForm.Info.activity.Length; j++)
                                {
                                    object[] values = new object[] { this._LKForm.Info.activity[j].ServerName, this._LKForm.Info.activity[j].EnterTime, this._LKForm.Info.activity[j].EnterIp };
                                    this._LKForm.entry_grid.Rows.Add(values);
                                }
                            }
                        }
                    });
                }
                this.selected_server.Items.Clear();
                foreach (KeyValuePair<string, string> pair in this.ServersList)
                {
                    this.selected_server.Items.Add(pair.Key);
                }
                if (!UpdateIsProcess)
                {
                    this.gui_progress.Text = LangController.GetTranslate(LangController.GetUpdateTagByStatus(_current_update_status)) + ((this._current_update_info.Length != 0) ? (": " + this._current_update_info) : "");
                }
            }
        }

        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(MainForm));
            this.update_group = new GroupBox();
            this.link_chng_runtime = new LinkLabel();
            this.btn_repair = new Button();
            this.btn_all_threads = new CheckBox();
            this.gui_progress = new Label();
            this.p_update_progress = new ProgressBar();
            this.btn_start = new Button();
            this.gui_currentver = new Label();
            this.gui_lastver = new Label();
            this.gui_setlang = new Label();
            this.combo_language = new ComboBox();
            this.servers_group = new GroupBox();
            this.btn_update_servers = new Button();
            this.ls_status = new Label();
            this.gui_ls = new Label();
            this.btn_play = new Button();
            this.btn_settings = new Button();
            this.btn_register = new Button();
            this.btn_support = new Button();
            this.btn_news = new Button();
            this.gui_alterserver = new Label();
            this.gui_auth_status = new Label();
            this.btn_auth = new LinkLabel();
            this.btn_logout = new LinkLabel();
            this.gui_gc_status = new Label();
            this.flush_settings_btn = new LinkLabel();
            this.selected_server = new ComboBox();
            this.mirror_link = new LinkLabel();
            this.lnk_cancel_update = new LinkLabel();
            this.update_group.SuspendLayout();
            this.servers_group.SuspendLayout();
            base.SuspendLayout();
            this.update_group.Controls.Add(this.lnk_cancel_update);
            this.update_group.Controls.Add(this.link_chng_runtime);
            this.update_group.Controls.Add(this.btn_repair);
            this.update_group.Controls.Add(this.btn_all_threads);
            this.update_group.Controls.Add(this.gui_progress);
            this.update_group.Controls.Add(this.p_update_progress);
            this.update_group.Controls.Add(this.btn_start);
            this.update_group.Controls.Add(this.gui_currentver);
            this.update_group.Controls.Add(this.gui_lastver);
            this.update_group.Location = new Point(12, 0x1b);
            this.update_group.Name = "update_group";
            this.update_group.Size = new Size(350, 0xbf);
            this.update_group.TabIndex = 0;
            this.update_group.TabStop = false;
            this.update_group.Text = "Обновление";
            this.link_chng_runtime.AutoSize = true;
            this.link_chng_runtime.Location = new Point(0xfb, 30);
            this.link_chng_runtime.Name = "link_chng_runtime";
            this.link_chng_runtime.Size = new Size(0x53, 13);
            this.link_chng_runtime.TabIndex = 9;
            this.link_chng_runtime.TabStop = true;
            this.link_chng_runtime.Text = "Сменить среду";
            this.link_chng_runtime.LinkClicked += new LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            this.btn_repair.Location = new Point(180, 0x8e);
            this.btn_repair.Name = "btn_repair";
            this.btn_repair.Size = new Size(0x9a, 0x19);
            this.btn_repair.TabIndex = 8;
            this.btn_repair.Text = "Починить игру";
            this.btn_repair.UseVisualStyleBackColor = true;
            this.btn_repair.Click += new EventHandler(this.btn_repair_Click);
            this.btn_all_threads.AutoSize = true;
            this.btn_all_threads.Location = new Point(0x21, 170);
            this.btn_all_threads.Name = "btn_all_threads";
            this.btn_all_threads.Size = new Size(0x115, 0x11);
            this.btn_all_threads.TabIndex = 7;
            this.btn_all_threads.Text = "Использовать все потоки (при проверке файлов)";
            this.btn_all_threads.UseVisualStyleBackColor = true;
            this.btn_all_threads.CheckedChanged += new EventHandler(this.btn_all_threads_CheckedChanged);
            this.gui_progress.AutoSize = true;
            this.gui_progress.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0xcc);
            this.gui_progress.Location = new Point(6, 0x4b);
            this.gui_progress.Name = "gui_progress";
            this.gui_progress.Size = new Size(0x38, 13);
            this.gui_progress.TabIndex = 6;
            this.gui_progress.Text = "Прогресс";
            this.p_update_progress.Location = new Point(9, 0x71);
            this.p_update_progress.Name = "p_update_progress";
            this.p_update_progress.Size = new Size(0x14f, 0x17);
            this.p_update_progress.TabIndex = 5;
            this.btn_start.Location = new Point(20, 0x8e);
            this.btn_start.Name = "btn_start";
            this.btn_start.Size = new Size(0x9a, 0x19);
            this.btn_start.TabIndex = 2;
            this.btn_start.Text = "Начать обновление";
            this.btn_start.UseVisualStyleBackColor = true;
            this.btn_start.Click += new EventHandler(this.btn_start_Click);
            this.gui_currentver.AutoSize = true;
            this.gui_currentver.Location = new Point(6, 0x34);
            this.gui_currentver.Name = "gui_currentver";
            this.gui_currentver.Size = new Size(0x5e, 13);
            this.gui_currentver.TabIndex = 1;
            this.gui_currentver.Text = "Текущая версия:";
            this.gui_lastver.AutoSize = true;
            this.gui_lastver.Location = new Point(6, 30);
            this.gui_lastver.Name = "gui_lastver";
            this.gui_lastver.Size = new Size(0x6c, 13);
            this.gui_lastver.TabIndex = 0;
            this.gui_lastver.Text = "Последняя версия: ";
            this.gui_setlang.AutoSize = true;
            this.gui_setlang.Location = new Point(0x167, 7);
            this.gui_setlang.Name = "gui_setlang";
            this.gui_setlang.Size = new Size(0x26, 13);
            this.gui_setlang.TabIndex = 1;
            this.gui_setlang.Text = "Язык:";
            this.combo_language.FormattingEnabled = true;
            this.combo_language.Location = new Point(420, 4);
            this.combo_language.Name = "combo_language";
            this.combo_language.Size = new Size(0x7d, 0x15);
            this.combo_language.TabIndex = 2;
            this.combo_language.SelectedIndexChanged += new EventHandler(this.combo_language_SelectedIndexChanged);
            this.combo_language.SelectedValueChanged += new EventHandler(this.combo_language_SelectedValueChanged);
            this.servers_group.Controls.Add(this.btn_update_servers);
            this.servers_group.Controls.Add(this.ls_status);
            this.servers_group.Controls.Add(this.gui_ls);
            this.servers_group.Location = new Point(0x170, 0x1b);
            this.servers_group.Name = "servers_group";
            this.servers_group.RightToLeft = RightToLeft.No;
            this.servers_group.Size = new Size(0xb5, 0xa2);
            this.servers_group.TabIndex = 3;
            this.servers_group.TabStop = false;
            this.servers_group.Text = "Сервера";
            this.btn_update_servers.Location = new Point(100, 0x85);
            this.btn_update_servers.Name = "btn_update_servers";
            this.btn_update_servers.Size = new Size(0x4b, 0x17);
            this.btn_update_servers.TabIndex = 4;
            this.btn_update_servers.Text = "Обновить";
            this.btn_update_servers.UseVisualStyleBackColor = true;
            this.btn_update_servers.Click += new EventHandler(this.btn_update_servers_Click);
            this.ls_status.AutoSize = true;
            this.ls_status.ForeColor = Color.Red;
            this.ls_status.Location = new Point(110, 20);
            this.ls_status.Name = "ls_status";
            this.ls_status.Size = new Size(0x43, 13);
            this.ls_status.TabIndex = 1;
            this.ls_status.Text = "Недоступен";
            this.gui_ls.AutoSize = true;
            this.gui_ls.Location = new Point(7, 20);
            this.gui_ls.Name = "gui_ls";
            this.gui_ls.Size = new Size(0x53, 13);
            this.gui_ls.TabIndex = 0;
            this.gui_ls.Text = "Логин-сервер: ";
            this.btn_play.Location = new Point(0x1bf, 0xde);
            this.btn_play.Name = "btn_play";
            this.btn_play.Size = new Size(0x66, 0x23);
            this.btn_play.TabIndex = 4;
            this.btn_play.Text = "Играть";
            this.btn_play.UseVisualStyleBackColor = true;
            this.btn_play.Click += new EventHandler(this.btn_play_Click);
            this.btn_settings.Location = new Point(0x153, 0xde);
            this.btn_settings.Name = "btn_settings";
            this.btn_settings.Size = new Size(0x66, 0x23);
            this.btn_settings.TabIndex = 5;
            this.btn_settings.Text = "Настройки";
            this.btn_settings.UseVisualStyleBackColor = true;
            this.btn_settings.Click += new EventHandler(this.btn_settings_Click);
            this.btn_register.Location = new Point(15, 0xde);
            this.btn_register.Name = "btn_register";
            this.btn_register.Size = new Size(0x66, 0x23);
            this.btn_register.TabIndex = 6;
            this.btn_register.Text = "Регистрация";
            this.btn_register.UseVisualStyleBackColor = true;
            this.btn_register.Click += new EventHandler(this.btn_register_Click);
            this.btn_support.Location = new Point(0xe7, 0xde);
            this.btn_support.Name = "btn_support";
            this.btn_support.Size = new Size(0x66, 0x23);
            this.btn_support.TabIndex = 7;
            this.btn_support.Text = "Поддержка";
            this.btn_support.UseVisualStyleBackColor = true;
            this.btn_support.Click += new EventHandler(this.btn_support_Click);
            this.btn_news.Location = new Point(0x7b, 0xde);
            this.btn_news.Name = "btn_news";
            this.btn_news.Size = new Size(0x66, 0x23);
            this.btn_news.TabIndex = 8;
            this.btn_news.Text = "Новости";
            this.btn_news.UseVisualStyleBackColor = true;
            this.btn_news.Click += new EventHandler(this.btn_news_Click);
            this.gui_alterserver.AutoSize = true;
            this.gui_alterserver.Font = new Font("Microsoft Sans Serif", 6.75f, FontStyle.Regular, GraphicsUnit.Point, 0xcc);
            this.gui_alterserver.Location = new Point(0x13, 0x106);
            this.gui_alterserver.Name = "gui_alterserver";
            this.gui_alterserver.Size = new Size(0, 12);
            this.gui_alterserver.TabIndex = 9;
            this.gui_auth_status.AutoSize = true;
            this.gui_auth_status.Location = new Point(0x10, 9);
            this.gui_auth_status.Name = "gui_auth_status";
            this.gui_auth_status.Size = new Size(0, 13);
            this.gui_auth_status.TabIndex = 10;
            this.btn_auth.AutoSize = true;
            this.btn_auth.Location = new Point(0xd3, 7);
            this.btn_auth.Name = "btn_auth";
            this.btn_auth.Size = new Size(0x49, 13);
            this.btn_auth.TabIndex = 11;
            this.btn_auth.TabStop = true;
            this.btn_auth.Text = "Авторизация";
            this.btn_auth.LinkClicked += new LinkLabelLinkClickedEventHandler(this.auth_btn_LinkClicked);
            this.btn_logout.AutoSize = true;
            this.btn_logout.Location = new Point(0x126, 7);
            this.btn_logout.Name = "btn_logout";
            this.btn_logout.Size = new Size(0x27, 13);
            this.btn_logout.TabIndex = 12;
            this.btn_logout.TabStop = true;
            this.btn_logout.Text = "Выйти";
            this.btn_logout.Visible = false;
            this.btn_logout.LinkClicked += new LinkLabelLinkClickedEventHandler(this.btn_logout_LinkClicked);
            this.gui_gc_status.AutoSize = true;
            this.gui_gc_status.Location = new Point(3, 0x103);
            this.gui_gc_status.Name = "gui_gc_status";
            this.gui_gc_status.Size = new Size(0x111, 13);
            this.gui_gc_status.TabIndex = 13;
            this.gui_gc_status.Text = "Установка соединения с игровым координатором...";
            this.flush_settings_btn.AutoSize = true;
            this.flush_settings_btn.Location = new Point(0x1bb, 0x102);
            this.flush_settings_btn.Name = "flush_settings_btn";
            this.flush_settings_btn.Size = new Size(0x6f, 13);
            this.flush_settings_btn.TabIndex = 14;
            this.flush_settings_btn.TabStop = true;
            this.flush_settings_btn.Text = "Сбросить настройки";
            this.flush_settings_btn.LinkClicked += new LinkLabelLinkClickedEventHandler(this.flush_settings_btn_LinkClicked);
            this.selected_server.FormattingEnabled = true;
            object[] items = new object[] { "P&N Triumph", "P&N RP", "P&N Reborn" };
            this.selected_server.Items.AddRange(items);
            this.selected_server.Location = new Point(0x170, 0xc3);
            this.selected_server.Name = "selected_server";
            this.selected_server.Size = new Size(0xb5, 0x15);
            this.selected_server.TabIndex = 15;
            this.selected_server.SelectedIndexChanged += new EventHandler(this.selected_server_SelectedIndexChanged);
            this.mirror_link.AutoSize = true;
            this.mirror_link.Location = new Point(0x18b, 0x102);
            this.mirror_link.Name = "mirror_link";
            this.mirror_link.Size = new Size(50, 13);
            this.mirror_link.TabIndex = 0x10;
            this.mirror_link.TabStop = true;
            this.mirror_link.Text = "Зеркало";
            this.mirror_link.LinkClicked += new LinkLabelLinkClickedEventHandler(this.mirror_link_LinkClicked);
            this.lnk_cancel_update.AutoSize = true;
            this.lnk_cancel_update.Location = new Point(0x83, 0x34);
            this.lnk_cancel_update.Name = "lnk_cancel_update";
            this.lnk_cancel_update.Size = new Size(120, 13);
            this.lnk_cancel_update.TabIndex = 10;
            this.lnk_cancel_update.TabStop = true;
            this.lnk_cancel_update.Text = "Отменить обновление";
            this.lnk_cancel_update.LinkClicked += new LinkLabelLinkClickedEventHandler(this.lnk_cancel_update_LinkClicked);
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x233, 0x113);
            base.Controls.Add(this.mirror_link);
            base.Controls.Add(this.selected_server);
            base.Controls.Add(this.flush_settings_btn);
            base.Controls.Add(this.gui_gc_status);
            base.Controls.Add(this.btn_logout);
            base.Controls.Add(this.btn_auth);
            base.Controls.Add(this.gui_auth_status);
            base.Controls.Add(this.gui_alterserver);
            base.Controls.Add(this.btn_news);
            base.Controls.Add(this.btn_support);
            base.Controls.Add(this.btn_register);
            base.Controls.Add(this.btn_settings);
            base.Controls.Add(this.btn_play);
            base.Controls.Add(this.servers_group);
            base.Controls.Add(this.combo_language);
            base.Controls.Add(this.gui_setlang);
            base.Controls.Add(this.update_group);
            base.Icon = (Icon) manager.GetObject("$this.Icon");
            this.MaximumSize = new Size(0x243, 0x13a);
            this.MinimumSize = new Size(0x243, 0x13a);
            base.Name = "MainForm";
            this.Text = "P&N Launcher";
            base.Load += new EventHandler(this.MainForm_Load);
            this.update_group.ResumeLayout(false);
            this.update_group.PerformLayout();
            this.servers_group.ResumeLayout(false);
            this.servers_group.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        public void InitLanguagesList()
        {
            this.combo_language.Items.Clear();
            string[] avaibleLanguages = LangController.GetAvaibleLanguages();
            int length = avaibleLanguages.Length;
            for (int i = 0; i < length; i++)
            {
                string str = avaibleLanguages[i];
                if (!string.IsNullOrEmpty(str))
                {
                    this.combo_language.Items.Add(str);
                }
            }
            this.combo_language.Text = LangController.CurrentLanguage;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string aOPTsPath;
            if (!UpdateIsProcess)
            {
                new SelectRuntime(this).ShowDialog();
                if (mRunTime.mRunTimeType != mRunTime.mRunTimeType)
                {
                    if ((!mRunTime.IsDirSelected() || !mRunTime.CheckAccessDir(null)) && !mRunTime.SelectDirectory())
                    {
                        return;
                    }
                    aOPTsPath = Path.Combine(Environment.CurrentDirectory, "pts");
                    if ((mRunTime.mRunTimeType == ERunTime.PTS) && Directory.Exists(aOPTsPath))
                    {
                        try
                        {
                            DirectoryInfo info = new DirectoryInfo(mRunTime.GetDirectory());
                            if (info.GetFiles("*").Length == 0)
                            {
                                info.Delete();
                                string directory = mRunTime.GetDirectory();
                                if (Path.GetPathRoot(aOPTsPath) != Path.GetPathRoot(directory))
                                {
                                    <>c__DisplayClass91_0 class_;
                                    this.SetUpdateInfo(UpdateStatus.CopyFiles, "Starting...");
                                    new Thread(delegate (object obj) {
                                        try
                                        {
                                            Tuple<string, string> tuple = (Tuple<string, string>) obj;
                                            UpdateIsProcess = true;
                                            this.Invoke(() => class_.btn_play.Enabled = false);
                                            this.CopyFilesRecursively(tuple.Item1, tuple.Item2);
                                            this.Invoke(delegate {
                                                mRunTime.LoadCurrentVersion();
                                                FileCache.FlushCache(true);
                                                FileCache.LoadCache();
                                                class_.GetFilesList();
                                                class_.UpdateGuiLanguage();
                                            });
                                            try
                                            {
                                                if (Directory.Exists(aOPTsPath))
                                                {
                                                    Directory.Delete(aOPTsPath, true);
                                                }
                                            }
                                            catch
                                            {
                                            }
                                            UpdateIsProcess = false;
                                        }
                                        catch (Exception exception)
                                        {
                                            MessageBox.Show(LangController.GetTranslate("error_copy_old_pts").Replace("%exp", exception.Message));
                                        }
                                        finally
                                        {
                                            UpdateIsProcess = false;
                                            this.Invoke(() => class_.btn_play.Enabled = true);
                                        }
                                    }).Start(new Tuple<string, string>(aOPTsPath, directory));
                                    return;
                                }
                                else
                                {
                                    Directory.Move(aOPTsPath, directory);
                                }
                            }
                            goto TR_0004;
                        }
                        catch
                        {
                            goto TR_0004;
                        }
                    }
                    else
                    {
                        goto TR_0001;
                    }
                }
            }
            return;
        TR_0001:
            mRunTime.LoadCurrentVersion();
            FileCache.FlushCache(true);
            FileCache.LoadCache();
            this.GetFilesList();
            this.UpdateGuiLanguage();
            return;
        TR_0004:
            try
            {
                if (Directory.Exists(aOPTsPath))
                {
                    Directory.Delete(aOPTsPath, true);
                }
            }
            catch
            {
            }
            goto TR_0001;
        }

        private void lnk_cancel_update_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (MessageBox.Show(LangController.GetTranslate("rollback_update_warning").Replace("%new_line%", Environment.NewLine), "P&N Launcher", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                if (System.IO.File.Exists("Launcher.backup"))
                {
                    System.IO.File.WriteAllText("update.bat", "\r\n                echo Updateing launcher... Wait 2 second\r\n\r\n                @echo off\r\n\r\n                timeout 1 > nul\r\n\r\n                del Launcher.exe\r\n                ren Launcher.backup Launcher.exe\r\n                \r\n                start Launcher.exe\r\n                del update.bat\r\n                ");
                    Process.Start("update.bat");
                    Environment.Exit(0);
                }
                else
                {
                    MessageBox.Show(LangController.GetTranslate("error_rollback"));
                }
            }
        }

        private string loadLastUsedServer()
        {
            try
            {
                return (!System.IO.File.Exists("server.last") ? "P&N Triumph" : System.IO.File.ReadAllText("server.last"));
            }
            catch
            {
                return "P&N Triumph";
            }
        }

        private void loadServers()
        {
            try
            {
                if (!System.IO.File.Exists("servers.list"))
                {
                    this.ServersHash = new byte[0];
                }
                else
                {
                    string s = System.IO.File.ReadAllText("servers.list");
                    using (MD5 md = MD5.Create())
                    {
                        this.ServersHash = md.ComputeHash(Encoding.UTF8.GetBytes(s));
                    }
                    string[] strArray = s.Substrings("<server>", "</server>", 0);
                    int length = strArray.Length;
                    for (int i = 0; i < length; i++)
                    {
                        char[] separator = new char[] { ':' };
                        string[] strArray2 = strArray[i].Split(separator);
                        this.ServersList.Add(strArray2[0], strArray2[1]);
                    }
                }
            }
            catch
            {
            }
            finally
            {
                if ((this.ServersHash.Length == 0) || (this.ServersList.Count == 0))
                {
                    this.ServersList.Add("P&N Triumph", "Rsh");
                    this.ServersList.Add("P&N RP", "Rsh");
                    this.ServersList.Add("P&N Reborn", "Reb");
                }
                if (_isAdminLauncher)
                {
                    this.ServersList.Add("Local (patch)", "admin_patch");
                    this.ServersList.Add("Local (no-patch)", "admin_no_patch");
                }
            }
        }

        public void Logout()
        {
            if (GameCoordinator.Authorized)
            {
                GameCoordinator.Send(new Packet(PacketIds.client_logout, 0));
            }
            GameCoordinator.Authorized = false;
            GameCoordinator.AuthKey = new byte[0];
            GameCoordinator.Login = "";
            if (System.IO.File.Exists("launcher.auth"))
            {
                System.IO.File.Delete("launcher.auth");
            }
            this.UpdateGuiLanguage();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            bool flag;
            bool flag2;
            this._AuthForm = new AuthForm(this);
            this._LKForm = new LKForm(this);
            if (System.IO.File.Exists("update.bat"))
            {
                System.IO.File.Delete("update.bat");
            }
            doLauncherUpdate();
            int num = LangController.LoadLanguages();
            CRunTime time = new CRunTime(Environment.CurrentDirectory, ERunTime.Main);
            CRunTime time2 = new CRunTime(flag ? Config.PTS_PATH : null, ERunTime.PTS);
            CRunTime time3 = new CRunTime(flag2 ? Config.REBORN_PATH : null, ERunTime.Reborn);
            mRunTimeList.Add(ERunTime.Main, time);
            mRunTimeList.Add(ERunTime.PTS, time2);
            mRunTimeList.Add(ERunTime.Reborn, time3);
            mRunTime = mRunTimeList[ERunTime.Main];
            mRunTimeList[ERunTime.Main].LoadCurrentVersion();
            this.selected_server.Text = this.loadLastUsedServer();
            this.loadServers();
            GameCoordinator.Init();
            if (num < 0)
            {
                MessageBox.Show($"Error in load languages [{num}].");
            }
            else
            {
                LangController.InitLanguage();
                this.InitGuiLanguage();
                this.InitLanguagesList();
            }
            if (Config.LoadConfig(out flag, out flag2) == -1)
            {
                this.ShowError(LangController.GetTranslate("msg_confignotfound"));
            }
            GameCoordinator.GCClient.OnConnectSuccess += new SocketEvent(this.GCClient_OnConnectSuccess);
            GameCoordinator.GCClient.OnErrorConnect += new SocketEvent(this.GCClient_OnErrorConnect);
            GameCoordinator.GCClient.OnNewPacket += new ClientEvent(this.GCClient_OnNewPacket);
            GameCoordinator.GCClient.OnDisconnect += new SocketEvent(this.GCClient_OnDisconnect);
            GameCoordinator.Connect();
            this.SetUpdateInfo(UpdateStatus.GettingInfo, "");
            FileCache.LoadCache();
            LoginIP = "185.71.66.50";
            LoginPort = 0x2afc;
            foreach (Process process in Process.GetProcessesByName("r2"))
            {
                if (!process.HasExited)
                {
                    process.Kill();
                }
            }
            this.lnk_cancel_update.Visible = System.IO.File.Exists("Launcher.backup");
        }

        private void mirror_link_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            new SelectMirror().ShowDialog();
        }

        private void saveLastUsedServer(string server)
        {
            try
            {
                System.IO.File.WriteAllText("server.last", server);
            }
            catch
            {
            }
        }

        private void saveNewIP(IPAddress ip)
        {
            try
            {
                string contents = System.IO.File.ReadAllText("launcher.cfg");
                int index = contents.IndexOf("<ip>");
                int num2 = contents.IndexOf("</ip>");
                contents = contents.Remove(index + 4, (num2 - index) - 4).Insert(index + 4, ip.ToString());
                System.IO.File.WriteAllText("launcher.cfg", contents);
            }
            catch
            {
            }
        }

        private void selected_server_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private bool SendCrashReport(ERunTime pRunTime, string pFilePath, string pArgs, int pExitCode, string pServer)
        {
            try
            {
                byte[] buff = new byte[0];
                string s = string.Empty;
                mRunTime.GetDirectory();
                string file = mRunTime.GetFile("CrashReport.dmp");
                string path = mRunTime.GetFile("env/pn.log");
                if (System.IO.File.Exists(file))
                {
                    buff = System.IO.File.ReadAllBytes(file);
                    try
                    {
                        System.IO.File.Delete(file);
                    }
                    catch
                    {
                    }
                }
                if (System.IO.File.Exists(path))
                {
                    string[] pInLog = System.IO.File.ReadAllLines(path);
                    Tuple<int, int> tuple = this.CalcPacketsStat(pInLog);
                    s = $"S {tuple.Item1} / R {tuple.Item2} " + Environment.NewLine;
                    int num2 = 0;
                    int index = 0;
                    while (true)
                    {
                        if ((index >= 0x400) || (index >= pInLog.Length))
                        {
                            int num3 = pInLog.Length - 0x400;
                            if (num3 < num2)
                            {
                                num3 = num2 + 1;
                            }
                            if (num3 != pInLog.Length)
                            {
                                s = s + $"... skipped {(num3 - num2)} lines ..." + Environment.NewLine;
                            }
                            for (int i = num3; i < pInLog.Length; i++)
                            {
                                s = s + pInLog[i] + Environment.NewLine;
                            }
                            break;
                        }
                        s = s + pInLog[index] + Environment.NewLine;
                        num2 = index;
                        index++;
                    }
                }
                string login = GameCoordinator.Login;
                string str5 = $"{mRunTime.mRunTimeType.ToString()}:{mRunTime.mCurrentVersion}";
                string str6 = pServer;
                int getCodeId = LangController.GetCodeId;
                byte[] bytes = Encoding.UTF8.GetBytes(login);
                byte[] buffer3 = Encoding.UTF8.GetBytes(str5);
                byte[] buffer4 = Encoding.UTF8.GetBytes(str6);
                byte[] buffer5 = Encoding.UTF8.GetBytes(pFilePath);
                byte[] buffer6 = Encoding.UTF8.GetBytes(pArgs);
                byte[] buffer7 = Encoding.UTF8.GetBytes(s);
                Packet p = new Packet(PacketIds.server_crush_report, ((((((((((((((1 + Encoding.UTF8.GetByteCount(login)) + 1) + Encoding.UTF8.GetByteCount(str5)) + 1) + Encoding.UTF8.GetByteCount(str6)) + 4) + 2) + Encoding.UTF8.GetByteCount(pFilePath)) + 1) + Encoding.UTF8.GetByteCount(pArgs)) + 4) + 4) + buff.Length) + 4) + Encoding.UTF8.GetByteCount(s));
                p.WriteByte((byte) bytes.Length);
                p.WriteBytes(bytes);
                p.WriteByte((byte) buffer3.Length);
                p.WriteBytes(buffer3);
                p.WriteByte((byte) buffer4.Length);
                p.WriteBytes(buffer4);
                p.WriteInt32(getCodeId);
                p.WriteInt16((short) buffer5.Length);
                p.WriteBytes(buffer5);
                p.WriteByte((byte) buffer6.Length);
                p.WriteBytes(buffer6);
                p.WriteInt32(pExitCode);
                p.WriteInt32(buff.Length);
                p.WriteBytes(buff);
                p.WriteInt32(buffer7.Length);
                p.WriteBytes(buffer7);
                GameCoordinator.Send(p);
                MessageBox.Show("Crash report successfuly sended.");
                return true;
            }
            catch (Exception exception)
            {
                MessageBox.Show("Error send crash report: " + exception.Message);
                return false;
            }
        }

        private void SetUpdateInfo(UpdateStatus status, string info)
        {
            _current_update_status = status;
            this._current_update_info = info;
            if ((status != UpdateStatus.DownloadFiles) || ((DateTime.Now - startUpdate).TotalSeconds > 0.0))
            {
                if (base.InvokeRequired)
                {
                    base.Invoke(delegate {
                        if (status == UpdateStatus.UpdateSuccess)
                        {
                            this.p_update_progress.Value = 100;
                        }
                        if (status != UpdateStatus.DownloadFiles)
                        {
                            this.gui_progress.Text = LangController.GetTranslate(LangController.GetUpdateTagByStatus(_current_update_status)) + ((this._current_update_info.Length != 0) ? (": " + this._current_update_info) : "");
                        }
                        else
                        {
                            long num1 = long.Parse(info);
                            float num = (((((float) num1) / ((float) (DateTime.Now - startUpdate).TotalSeconds)) / 1000f) / 1000f) * 8f;
                            long totalBytes = TotalBytes;
                            double num2 = (((double) num1) / ((double) TotalBytes)) * 100.0;
                            double num3 = (((double) num1) / 1024.0) / 1024.0;
                            double num4 = (((double) TotalBytes) / 1024.0) / 1024.0;
                            this.gui_progress.Text = LangController.GetTranslate(LangController.GetUpdateTagByStatus(_current_update_status)) + $": [{this.currentfile.NetPath}] 
                         [{((int) num3)} MB / {((int) num4)} MB] ({((int) num)} Mbps/sec)";
                            this.p_update_progress.Value = (int) num2;
                        }
                    });
                }
                else
                {
                    if (status == UpdateStatus.UpdateSuccess)
                    {
                        this.p_update_progress.Value = 100;
                    }
                    if (status != UpdateStatus.DownloadFiles)
                    {
                        this.gui_progress.Text = LangController.GetTranslate(LangController.GetUpdateTagByStatus(_current_update_status)) + ((this._current_update_info.Length != 0) ? (": " + this._current_update_info) : "");
                    }
                    else
                    {
                        long num = long.Parse(info);
                        float num2 = (((((float) num) / ((float) (DateTime.Now - startUpdate).TotalSeconds)) / 1000f) / 1000f) * 8f;
                        double num3 = (((double) num) / 1024.0) / 1024.0;
                        double num4 = (((double) TotalBytes) / 1024.0) / 1024.0;
                        this.gui_progress.Text = LangController.GetTranslate(LangController.GetUpdateTagByStatus(_current_update_status)) + $": [{this.currentfile.Name}] 
                         [{((int) num3)} MB / {((int) num4)} MB] ({((int) num2)} Mbps/sec)";
                        this.p_update_progress.Value = (int) ((num / TotalBytes) * 100);
                    }
                }
            }
        }

        public void ShowError(string text)
        {
            MessageBox.Show(text, LangController.GetTranslate("msg_error"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }

        public void ShowInfo(string text)
        {
            MessageBox.Show(text, LangController.GetTranslate("msg_info"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private static void StartOption()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "r2option.exe";
            startInfo.WorkingDirectory = mRunTime.GetDirectory();
            Process.Start(startInfo);
        }

        private void Update_File_Progress_Changed(object sender, DownloadProgressChangedEventArgs e)
        {
            if ((DateTime.Now - lastProgressNotify).TotalSeconds >= 1.0)
            {
                this.SetUpdateInfo(UpdateStatus.DownloadFiles, (DownloadBytes + e.BytesReceived).ToString());
                lastProgressNotify = DateTime.Now;
            }
        }

        public void update_info()
        {
            GameCoordinator.UpdateInfo();
        }

        public void UpdateGuiLanguage()
        {
            if (base.InvokeRequired)
            {
                base.Invoke(() => this.InitGuiLanguage());
            }
            else
            {
                this.InitGuiLanguage();
            }
        }

        public static int LastVersion =>
            mRunTime.mLastVersion;

        public static int CurrentVersion =>
            mRunTime.mCurrentVersion;

        [StructLayout(LayoutKind.Sequential)]
        private struct _startParams
        {
            public string game_name;
            public _startParams(string game_name)
            {
                this.game_name = game_name;
            }
        }

        public class AccountInfo
        {
            [JsonProperty("balance")]
            public int Balance;
            [JsonProperty("is_online")]
            public bool IsOnline;
            [JsonProperty("current_server")]
            public string CurrentServer;
            [JsonProperty("last_ip")]
            public string LastIp;
        }

        public class Activity
        {
            [JsonProperty("enter_date")]
            public DateTime EnterTime;
            [JsonProperty("server")]
            public string ServerName;
            [JsonProperty("ip")]
            public string EnterIp;
        }

        public enum ERunTime
        {
            Main,
            PTS,
            Reborn
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct gameEntrye
        {
            public DateTime date;
            public string server;
        }

        public class ProfileInfo
        {
            [JsonProperty("account_info")]
            public MainForm.AccountInfo account_info;
            [JsonProperty("activity")]
            public MainForm.Activity[] activity;
        }
    }
}

