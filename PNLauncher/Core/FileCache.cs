namespace PNLauncher.Core
{
    using PNLauncher;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class FileCache
    {
        public static Dictionary<string, CacheItem> CacheList = new Dictionary<string, CacheItem>();

        public static void AddInCache(CacheItem itm)
        {
            if (CacheList.ContainsKey(itm.Path))
            {
                CacheList.Remove(itm.Path);
            }
            if (!itm.Path.ToLower().Contains("launcher"))
            {
                CacheList.Add(itm.Path, itm);
            }
        }

        public static string byteArrayToString(byte[] arr, int ln)
        {
            byte[] destinationArray = new byte[ln];
            Array.Copy(arr, 0, destinationArray, 0, ln);
            return BitConverter.ToString(destinationArray).Replace("-", string.Empty).ToLower();
        }

        public static void FlushCache(bool pNoDelete = false)
        {
            CacheList.Clear();
            if (!pNoDelete && File.Exists(MainForm.mRunTime.GetFile("files.sum.cache")))
            {
                File.Delete(MainForm.mRunTime.GetFile("files.sum.cache"));
            }
        }

        public static void LoadCache()
        {
            if (File.Exists(MainForm.mRunTime.GetFile("files.sum.cache")))
            {
                char[] separator = new char[] { '\n' };
                string[] strArray = File.ReadAllText(MainForm.mRunTime.GetFile("files.sum.cache")).Split(separator);
                int length = strArray.Length;
                for (int i = 0; i < length; i++)
                {
                    try
                    {
                        string str = strArray[i];
                        if (!string.IsNullOrEmpty(str))
                        {
                            char[] chArray2 = new char[] { ' ' };
                            string[] strArray2 = str.Split(chArray2);
                            string file = MainForm.mRunTime.GetFile(strArray2[0]);
                            CacheItem item = new CacheItem {
                                Path = file,
                                MD5 = strArray2[1],
                                Size = int.Parse(strArray2[2]),
                                writeTime = long.Parse(strArray2[3])
                            };
                            if (File.Exists(file) && (new FileInfo(file).LastWriteTime.ToBinary() == item.writeTime))
                            {
                                CacheList.Add(item.Path, item);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        public static void SaveCache()
        {
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, CacheItem> pair in CacheList)
            {
                builder.Append($"{pair.Value.Path} {pair.Value.MD5} {pair.Value.Size} {pair.Value.writeTime}
");
            }
            File.WriteAllText(MainForm.mRunTime.GetFile("files.sum.cache"), builder.ToString());
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CacheItem
        {
            public string Path;
            public string MD5;
            public int Size;
            public long writeTime;
        }
    }
}

