namespace PNLauncher.Core
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    public static class Auth
    {
        public const string AUTH_FILE = "launcher.auth";
        public const int AUTH_KEY_SIZE = 0x1000;

        public static bool ReadKey(out byte[] bf)
        {
            bf = File.ReadAllBytes("launcher.auth");
            return true;
        }

        public static void SaveKey(byte[] bf)
        {
            File.WriteAllBytes("launcher.auth", bf);
        }
    }
}

