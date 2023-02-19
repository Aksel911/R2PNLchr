namespace Launcher.Help
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public static class PathHelper
    {
        public static List<string> GetAllPathTree(string pTree)
        {
            List<string> list = new List<string>();
            string fileName = Path.GetFileName(pTree);
            string path = (fileName == null) ? pTree : pTree.Replace(fileName, "");
            while (true)
            {
                string directoryName = Path.GetDirectoryName(path);
                if (directoryName == null)
                {
                    list.Add(Path.GetPathRoot(pTree));
                    list.Reverse();
                    return list;
                }
                list.Add(directoryName);
                path = path.Replace(Path.GetFileName(directoryName), "");
            }
        }
    }
}

