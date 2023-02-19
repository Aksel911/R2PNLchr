namespace Launcher.Core
{
    using PNLauncher;
    using PNLauncher.Core;
    using PNLauncher.Languages;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public class CRunTime
    {
        private string mEnvDir;
        private string mDownloadURL;
        public List<UpdateFile> mUpdateFiles;
        public List<UpdateFile> mClientFiles;

        public CRunTime(string pEnvDir, MainForm.ERunTime pRunTime)
        {
            this.mEnvDir = pEnvDir;
            this.mRunTimeType = pRunTime;
            this.mUpdateFiles = new List<UpdateFile>();
            this.mClientFiles = new List<UpdateFile>();
        }

        public bool CheckAccessDir(string pEnvDir = null)
        {
            try
            {
                if (!Directory.Exists((pEnvDir == null) ? this.mEnvDir : pEnvDir))
                {
                    Directory.CreateDirectory((pEnvDir == null) ? this.mEnvDir : pEnvDir);
                }
                string path = Path.Combine((pEnvDir == null) ? this.mEnvDir : pEnvDir, "test");
                File.Create(path).Close();
                File.Delete(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GetDirectory() => 
            this.mEnvDir;

        public string GetDownloadFile(string pFilename) => 
            new Uri(new Uri(this.mDownloadURL), pFilename).AbsoluteUri;

        public string GetFile(string pFileName) => 
            Path.Combine(this.mEnvDir, pFileName);

        public string GetFilesPath()
        {
            if (this.mRunTimeType == MainForm.ERunTime.Main)
            {
                return this.GetFile("files.sum");
            }
            if (this.mRunTimeType == MainForm.ERunTime.PTS)
            {
                return this.GetFile("pts_files.sum");
            }
            if (this.mRunTimeType != MainForm.ERunTime.Reborn)
            {
                throw new Exception("Error GetFilesPath()");
            }
            return this.GetFile("reborn_files.sum");
        }

        public string GetFilesSumURL()
        {
            if (this.mRunTimeType == MainForm.ERunTime.Main)
            {
                return this.GetDownloadFile("files.sum");
            }
            if (this.mRunTimeType == MainForm.ERunTime.PTS)
            {
                return this.GetDownloadFile("pts_files.sum");
            }
            if (this.mRunTimeType != MainForm.ERunTime.Reborn)
            {
                throw new Exception("Error GetFilesSumURL()");
            }
            return this.GetDownloadFile("reborn_files.sum");
        }

        public string GetVerPath()
        {
            if (this.mRunTimeType == MainForm.ERunTime.Main)
            {
                return this.GetFile("pn.ver");
            }
            if (this.mRunTimeType == MainForm.ERunTime.PTS)
            {
                return this.GetFile("pn_pts.ver");
            }
            if (this.mRunTimeType != MainForm.ERunTime.Reborn)
            {
                throw new Exception("Error GetVerPath()");
            }
            return this.GetFile("pn_reborn.ver");
        }

        public string GetVerURL()
        {
            if (this.mRunTimeType == MainForm.ERunTime.Main)
            {
                return this.GetDownloadFile("pn.ver");
            }
            if (this.mRunTimeType == MainForm.ERunTime.PTS)
            {
                return this.GetDownloadFile("pn_pts.ver");
            }
            if (this.mRunTimeType != MainForm.ERunTime.Reborn)
            {
                throw new Exception("Error GetVerURL()");
            }
            return this.GetDownloadFile("pn_reborn.ver");
        }

        public bool IsDirSelected() => 
            this.mEnvDir != null;

        public void LoadCurrentVersion()
        {
            string verPath = this.GetVerPath();
            try
            {
                this.mCurrentVersion = BitConverter.ToInt32(File.ReadAllBytes(verPath), 0);
            }
            catch
            {
                this.mCurrentVersion = 0;
            }
        }

        public void LoadLastVersion()
        {
            try
            {
                this.mLastVersion = BitConverter.ToInt32(MainForm.Client.DownloadData(this.GetVerURL()), 0);
            }
            catch
            {
                this.mLastVersion = this.mCurrentVersion;
                MessageBox.Show("[" + this.mRunTimeType.ToString() + "] error load last version");
            }
        }

        public bool SelectDirectory()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            while (true)
            {
                dialog.SelectedPath = MainForm.GetPrevDirectory();
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return false;
                }
                if ((this.mRunTimeType != MainForm.ERunTime.Main) && dialog.SelectedPath.Contains(Environment.CurrentDirectory))
                {
                    MessageBox.Show(LangController.GetTranslate("folder_conflict"));
                    continue;
                }
                if (this.CheckAccessDir(dialog.SelectedPath))
                {
                    this.SetDirectory(dialog.SelectedPath);
                    if (this.mRunTimeType == MainForm.ERunTime.PTS)
                    {
                        Config.SetPTSPath(this.GetDirectory());
                    }
                    else if (this.mRunTimeType == MainForm.ERunTime.Reborn)
                    {
                        Config.SetRebornPath(this.GetDirectory());
                    }
                    return true;
                }
            }
        }

        public void SetDirectory(string pEnvDir)
        {
            this.mEnvDir = pEnvDir;
        }

        public void SetDownloadURL(string pUrl)
        {
            this.mHaveDownloadURL = true;
            this.mDownloadURL = pUrl;
        }

        public void SetParam(string pParam)
        {
            this.mStartParam = pParam;
            this.mHaveParam = true;
        }

        public void SuccessUpdate()
        {
            this.mCurrentVersion = this.mLastVersion;
        }

        public bool mHaveParam { get; private set; }

        public string mStartParam { get; private set; }

        public MainForm.ERunTime mRunTimeType { get; private set; }

        public bool mHaveDownloadURL { get; private set; }

        public int mLastVersion { get; private set; }

        public int mCurrentVersion { get; private set; }
    }
}

