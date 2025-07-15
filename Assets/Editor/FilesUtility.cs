using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Editor
{
    public static class FilesUtility
    {
        public static void ProcessCommand(string command, string argument)
        {
            // 创建进程启动信息对象
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(command);
            // 设置进程启动参数
            info.Arguments = argument;
            info.CreateNoWindow = false;
            info.ErrorDialog = true;
            // info.UseShellExecute = true;

            // 如果使用shell执行，则不重定向标准输入、输出和错误
            if (info.UseShellExecute)
            {
                info.RedirectStandardOutput = false;
                info.RedirectStandardError = false;
                info.RedirectStandardInput = false;
            }
            else
            {
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;
                info.RedirectStandardInput = true;
                info.StandardOutputEncoding = System.Text.UTF8Encoding.UTF8;
                info.StandardErrorEncoding = System.Text.UTF8Encoding.UTF8;
            }
            // 启动进程
            System.Diagnostics.Process process = System.Diagnostics.Process.Start(info);
            if (!info.UseShellExecute)
            {
                Debug.Log(process.StandardOutput);
                Debug.Log(process.StandardError);
            }
            // 等待进程退出并关闭进程
            process.WaitForExit();
            process.Close();
        }
        
        public static List<string> GetAllFilesByDirectory(string dirPath,string exclusive = "")
        {
            var s = new List<string> ();
            string[] files = Directory.GetFiles(dirPath);
            for (int i = 0; i < files.Length; i++)
            {
                string fullPath = Path.GetFullPath(files[i]);
                if (!string.IsNullOrEmpty(exclusive) && fullPath.Contains(exclusive))
                {
                    continue;
                }
                s.Add(fullPath);
            }

            string[] directories = Directory.GetDirectories(dirPath);
            for (int i = 0; i < directories.Length; i++)
            {
                var s2 = GetAllFilesByDirectory(directories[i],exclusive);
                s.AddRange(s2);
            }
            return s;
        }

        public static void DeleteAllDirectory(string targetDir)
        {
            if (!Directory.Exists(targetDir))
            {
                Debug.LogError($"目录不存在，无法删除，{targetDir}");
                return;
            }
            string[] dirs = Directory.GetDirectories(targetDir);
            foreach (var VARIABLE in dirs)
            {
                Directory.Delete(VARIABLE,true);
            }
        }

        public static void DeleteAllFiles(string targetDir)
        {
            if (!Directory.Exists(targetDir))
            {
                Debug.LogError($"目录不存在，无法删除，{targetDir}");
                return;
            }
            string[] files = Directory.GetFiles(targetDir);
            foreach (var VARIABLE in files)
            {
                File.Delete(VARIABLE);
            }
        }

        public static void Copy2TargetDirectory(string sourceDir, string targetDir)
        {
            string[] sourceFiles = Directory.GetFiles(sourceDir);
            foreach (var sourceFile in sourceFiles)
            {
                var fileName = Path.GetFileName(sourceFile);
                var targetFiles = Path.Combine(targetDir, fileName);
                if (File.Exists(targetFiles))
                {
                    File.Delete(targetFiles);
                }
                File.Copy(sourceFile,targetFiles);
            }
            string[] dirs = Directory.GetDirectories(targetDir);
            if (dirs is {Length:<=0})
            {
                return;
            }

            for (int i = 0; i < dirs.Length; i++)
            {
                var subSourceDir = dirs[i];
                var subDirName = Path.GetDirectoryName(subSourceDir);
                var subTargetDir = Path.Combine(targetDir, subDirName);
                if (!Directory.Exists(subTargetDir))
                {
                    Directory.CreateDirectory(subTargetDir);
                }
                Copy2TargetDirectory(subSourceDir, subTargetDir);
            }
        }

        public static void Sync2TargetDirectory(string sourceDir, string targetDir)
        {
            if (sourceDir==targetDir)
            {
                Debug.LogError("源目录与目标目录相同，无法同步");
                return;
            }

            DeleteAllDirectory(targetDir);
            DeleteAllFiles(targetDir);
            Copy2TargetDirectory(sourceDir, targetDir);
            Debug.Log($"完成目录同步！\n源目录：{sourceDir}\n目标目录：{targetDir}");
        }
        public static void WriteBinaryFile(string fileFullPath, byte[] bytes)
        {
            var fs = new FileStream(fileFullPath, FileMode.OpenOrCreate);
            BinaryWriter writer = new BinaryWriter(fs);
            writer.Write(bytes);
            writer.Close();
            fs.Close();
        }
    }
}