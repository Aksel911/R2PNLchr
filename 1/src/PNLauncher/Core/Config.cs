namespace PNLauncher.Core
{
    using PNLauncher;
    using PNLauncher.Help;
    using System;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public static class Config
    {
        public const string CFG_FILE = "launcher.cfg";
        public const string PTS_PATH_FILE = "pn_pts.path";
        public const string REBORN_PATH_FILE = "pn_reborn.path";
        public const int PART_SIZE = 0x100000;
        public static int PACKET_BUFFER = 0x100400;
        public static string RegisterURL = "https://vk.com/";
        public static string SupportURL = "https://vk.com/";
        public static string NewsURL = "https://vk.com/";

        public static bool ChangeVersion(int new_ver)
        {
            System.IO.File.WriteAllBytes("pn.ver", BitConverter.GetBytes(new_ver));
            return true;
        }

        public static int LoadConfig(out bool aPTSPathLoaded, out bool aRebornPathLoaded)
        {
            PTS_PATH = Path.Combine(MainForm.GetPrevDirectory(), "pts");
            aPTSPathLoaded = false;
            REBORN_PATH = Path.Combine(MainForm.GetPrevDirectory(), "reborn");
            aRebornPathLoaded = false;
            try
            {
                if (System.IO.File.Exists("launcher.cfg"))
                {
                    string str = System.IO.File.ReadAllText("launcher.cfg");
                    GAMECOORD_IP = IPAddress.Parse(str.Substring("<ip>", "</ip>", 0));
                    GAMECOORD_PORT = ushort.Parse(str.Substring("<port>", "</port>", 0));
                    if (System.IO.File.Exists("pn_pts.path"))
                    {
                        PTS_PATH = System.IO.File.ReadAllText("pn_pts.path");
                        aPTSPathLoaded = true;
                    }
                    if (System.IO.File.Exists("pn_reborn.path"))
                    {
                        REBORN_PATH = System.IO.File.ReadAllText("pn_reborn.path");
                        aRebornPathLoaded = true;
                    }
                }
                else
                {
                    return -1;
                }
            }
            catch
            {
                GAMECOORD_IP = IPAddress.Parse("185.71.66.50");
                GAMECOORD_PORT = 0x34bc;
                MessageBox.Show("Error load launcher.cfg. Please check game files.");
            }
            return 0;
        }

        public static void SetPTSPath(string pNewPath)
        {
            System.IO.File.WriteAllText("pn_pts.path", pNewPath);
        }

        public static void SetRebornPath(string pNewPath)
        {
            System.IO.File.WriteAllText("pn_reborn.path", pNewPath);
        }

        public static IPAddress GAMECOORD_IP { get; private set; }

        public static ushort GAMECOORD_PORT { get; private set; }

        public static string PTS_PATH { get; private set; }

        public static string REBORN_PATH { get; private set; }
    }
}

