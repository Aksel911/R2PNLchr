namespace PNLauncher.Core
{
    using System;
    using System.Runtime.CompilerServices;

    public class UpdateFile
    {
        public UpdateFile(string name, string md5, int size, string net_path)
        {
            this.NetPath = net_path;
            this.Size = size;
            this.Name = name;
            this.MD5Hash = md5;
        }

        public int Size { get; private set; }

        public string Name { get; private set; }

        public string NetPath { get; private set; }

        public string MD5Hash { get; private set; }
    }
}

