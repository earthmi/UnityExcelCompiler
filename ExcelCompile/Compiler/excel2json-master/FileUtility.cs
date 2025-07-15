using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace excel2json
{
    public static class FileUtility
    {
        public static List<string> GetFilesFull(string dirPath, string filter = "")
        {
            var s = new List<string> ();
            string[] files = Directory.GetFiles(dirPath);
            for (int i = 0; i < files.Length; i++)
            {
                string fileName = Path.GetFullPath(files[i]);
                if (fileName.Contains(filter) && !fileName.Contains("$") )
                {
                    s.Add(fileName);
                }
            }

            string[] directories = Directory.GetDirectories(dirPath);
            for (int i = 0; i < directories.Length; i++)
            {
                var s2 = GetFilesFull(directories[i], filter);
                s.AddRange(s2);
            }
            return s;
        }
        public static void DeleteAllDirectory(string targetDir)
        {
            if (!Directory.Exists(targetDir))
            {
                Console.WriteLine($"目录不存在，无法删除，{targetDir}");
                return;
            }
            string[] dirs = Directory.GetDirectories(targetDir);
            foreach (var VARIABLE in dirs)
            {
                Directory.Delete(VARIABLE,true);
            }
        }

        public static void ClearDirectory(string targetDir)
        {
            DeleteAllDirectory(targetDir);
            foreach (var VARIABLE in Directory.GetFiles(targetDir))
            {
                if (File.Exists(VARIABLE))
                {
                    File.Delete(VARIABLE);
                }
            }
        }

        public static bool HaveNonEnglish(string name)
        {
            string pattern = @"[^a-zA-Z0-9_]+";
            return Regex.IsMatch(name,pattern);
        }
        // public static List<string> GetFiles(string dirPath, string filter = "")
        // {
        //     var fullPath = 
        // }
    }
}