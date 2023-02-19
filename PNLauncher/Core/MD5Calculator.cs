namespace PNLauncher.Core
{
    using Launcher.Help;
    using PNLauncher;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;
    using System.Threading;

    public static class MD5Calculator
    {
        public static int Threads;
        public static UpdateFile[] FilesToUpdate;
        public static int currentIndex;
        public static int lostIndex;
        public static object syncObject;
        public static int ThreadsSuccess;
        public static int CheckedFiles;
        public static long TotalBytes;
        public static List<UpdateFile> WrongFiles;

        private static string calcMD5(UpdateFile current)
        {
            FileStream inputStream = new FileStream(current.Name, FileMode.Open, FileAccess.Read);
            if (inputStream == null)
            {
                return "";
            }
            inputStream.Close();
            return BitConverter.ToString(MD5.Create().ComputeHash(inputStream)).Replace("-", string.Empty).ToLower();
        }

        private static void create_path(string path)
        {
            List<string> allPathTree = Launcher.Help.PathHelper.GetAllPathTree(path);
            int count = allPathTree.Count;
            for (int i = 0; i < count; i++)
            {
                if (!Directory.Exists(allPathTree[i]))
                {
                    Directory.CreateDirectory(allPathTree[i]);
                }
            }
        }

        private static void process()
        {
            while (true)
            {
                UpdateFile file = null;
                object syncObject = MD5Calculator.syncObject;
                lock (syncObject)
                {
                    if (currentIndex == lostIndex)
                    {
                        Interlocked.Increment(ref ThreadsSuccess);
                        break;
                    }
                    currentIndex++;
                    file = FilesToUpdate[currentIndex];
                    if (file == null)
                    {
                        Interlocked.Increment(ref CheckedFiles);
                        continue;
                    }
                }
                if (!File.Exists(MainForm.mRunTime.GetFile(file.Name)))
                {
                    create_path(MainForm.mRunTime.GetFile(file.Name));
                    syncObject = MD5Calculator.syncObject;
                    lock (syncObject)
                    {
                        WrongFiles.Add(file);
                        TotalBytes += file.Size;
                    }
                    Interlocked.Increment(ref CheckedFiles);
                }
                else
                {
                    string str = "";
                    if (FileCache.CacheList.ContainsKey(file.Name))
                    {
                        FileCache.CacheItem item = FileCache.CacheList[file.Name];
                        DateTime lastWriteTime = new FileInfo(item.Path).LastWriteTime;
                        if (lastWriteTime.ToBinary() == item.writeTime)
                        {
                            str = item.MD5;
                        }
                        else
                        {
                            FileCache.CacheList.Remove(item.Path);
                        }
                    }
                    if (string.IsNullOrEmpty(str))
                    {
                        str = calcMD5(file);
                    }
                    if (((str != file.MD5Hash) && !file.Name.Contains("R2.cfg")) && ((MainForm.mIsIgnoreLauncherUpdate && !file.Name.Contains("Launcher.exe")) || !MainForm.mIsIgnoreLauncherUpdate))
                    {
                        syncObject = MD5Calculator.syncObject;
                        lock (syncObject)
                        {
                            WrongFiles.Add(file);
                            TotalBytes += file.Size;
                        }
                    }
                    else
                    {
                        syncObject = MD5Calculator.syncObject;
                        lock (syncObject)
                        {
                            FileCache.CacheItem itm = new FileCache.CacheItem {
                                MD5 = file.MD5Hash,
                                Path = file.Name,
                                Size = file.Size,
                                writeTime = new FileInfo(file.Name).LastWriteTime.ToBinary()
                            };
                            FileCache.AddInCache(itm);
                        }
                    }
                    Interlocked.Increment(ref CheckedFiles);
                }
            }
        }

        public static void Start()
        {
            int length = FilesToUpdate.Length;
            lostIndex = length;
            int num2 = 0;
            while (num2 < length)
            {
                int index = 0;
                while (true)
                {
                    if (index >= ((length - 1) - num2))
                    {
                        num2++;
                        break;
                    }
                    if (FilesToUpdate[index].Size < FilesToUpdate[index + 1].Size)
                    {
                        FilesToUpdate[index + 1] = FilesToUpdate[index];
                        FilesToUpdate[index] = FilesToUpdate[index + 1];
                    }
                    index++;
                }
            }
            for (int i = 0; i < Threads; i++)
            {
                Thread thread1 = new Thread(new ThreadStart(MD5Calculator.process));
                thread1.IsBackground = true;
                thread1.Start();
            }
        }

        public static bool Success() => 
            ThreadsSuccess == Threads;
    }
}

