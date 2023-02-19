namespace PNLauncher.Help
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public static class StringHelper
    {
        public static string AsString(this byte[] bf)
        {
            int length = bf.Length;
            char[] chArray = new char[length];
            for (int i = 0; i < length; i++)
            {
                chArray[i] = (char) bf[i];
            }
            return new string(chArray);
        }

        public static string Substring(this string str, string left, string right, int idx = 0)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }
            int index = str.IndexOf(left, idx);
            if (index == -1)
            {
                return string.Empty;
            }
            int startIndex = index + left.Length;
            int num3 = str.IndexOf(right, startIndex);
            return ((num3 != -1) ? str.Substring(startIndex, num3 - startIndex) : string.Empty);
        }

        public static string[] Substrings(this string str, string left, string right, int startIndex = 0)
        {
            int num = startIndex;
            List<string> list = new List<string>();
            while (true)
            {
                int index = str.IndexOf(left, num);
                if (index != -1)
                {
                    int num3 = index + left.Length;
                    int num4 = str.IndexOf(right, num3);
                    if (num4 != -1)
                    {
                        list.Add(str.Substring(num3, num4 - num3));
                        num = num4 + right.Length;
                        continue;
                    }
                }
                return list.ToArray();
            }
        }
    }
}

