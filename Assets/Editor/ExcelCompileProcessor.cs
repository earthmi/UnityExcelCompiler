using System;
using System.IO;
using System.Reflection;
using ExcelTable;
using MiniGame.Support;
// using Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
// using JObject = Newtonsoft.Json.Linq.JObject;

namespace Editor
{
    public  class ExcelCompileProcessor
    {
        static string ProjectPath
        {
            get
            {
                System.IO.DirectoryInfo parent = System.IO.Directory.GetParent(Application.dataPath);
                return parent.ToString();
            }
        }
        
        private static string JsonWorkKey = "AutoDeserializeJsonAndSave";
        [MenuItem("ExcelCompile/Excel 2 JSON\\C#")]
        public static void ExecuteCompileExcel()
        {
            var compileRootPath = Path.Combine(ProjectPath, "ExcelCompile");
            if (!Directory.Exists(compileRootPath))
            {
                Debug.LogError($"please check the excel Compile root directory is exist!\n{compileRootPath}");
                return;
            }
            var exePath = Path.Combine(compileRootPath,"Compiler","excel2json.exe");
            if (!File.Exists(exePath))
            {
                Debug.LogError($"please check the exe directory is exist!\n{exePath}");
                return;
            }
            var args = $"export {compileRootPath}";
            FilesUtility.ProcessCommand(exePath,args);
            Debug.Log($" successfully execute \"excel 2 JSON/C#\" to source directory.\n{compileRootPath}");
            var jsonSourcePath = Path.Combine(compileRootPath, "Json");
            var csharpSourcePath = Path.Combine(compileRootPath, "CSharp");
            if (!Directory.Exists(jsonSourcePath) || !Directory.Exists(csharpSourcePath))
            {
                Debug.LogError($" unable to find \"Json\" or \"CSharp\" directory in path \n{compileRootPath}");
                return;
            }
            var targetCsharpPath = Path.Combine(Application.dataPath, "Runetime","ExcelDataDefine");
            if (!Directory.Exists(targetCsharpPath))
            {
                Debug.LogError($"unable to find target csharp directory!\n{targetCsharpPath}");
                return;
            }
            EditorPrefs.SetString(JsonWorkKey,jsonSourcePath);
            FilesUtility.Sync2TargetDirectory(csharpSourcePath,targetCsharpPath);
            AssetDatabase.Refresh();
        }

        [DidReloadScripts]
        static void DeSerializeJsonThen2Binary()
        {
            var pathSource = EditorPrefs.GetString(JsonWorkKey);
            if (string.IsNullOrEmpty(pathSource))
                return;

            EditorPrefs.DeleteKey(JsonWorkKey);
            var targetJsonPath = Path.Combine(Application.dataPath, "Resources","ConfigBytes");
            if (!Directory.Exists(targetJsonPath))
            {
                Debug.LogError($"unable to find target json directory!\n{targetJsonPath}");
                return;
            }
            SyncJsonFile2Binary(pathSource,targetJsonPath);
            Debug.Log($"successfully deserializer Json then save with Binary!\n{targetJsonPath}");
        }
        
        //预先烘焙数据，将Json反序列化成具体对象之后，再以二进制的方式存入本地
        public static void SyncJsonFile2Binary(string sourceDir, string targetDir)
        {
            if (sourceDir==targetDir)
            {
                Debug.LogError("源目录与目标目录相同，无法同步");
                return;
            }

            FilesUtility.DeleteAllDirectory(targetDir);
            FilesUtility.DeleteAllFiles(targetDir);
            string[] jsoFiles = Directory.GetFiles(sourceDir, "*.json");
            foreach (var fullPath in jsoFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(fullPath);
                string fullClassName = $"ExcelTable.Config{fileName}";
                Assembly assembly = Assembly.GetAssembly(typeof(ExcelDataBake));
                var type = assembly.GetType(fullClassName);
                if (type == null)
                {
                    Debug.LogError($"无法找到对应的c#类:{fullClassName},无法进行json反序列化");
                    continue;
                }
                string json = File.ReadAllText(fullPath);
                var deserialize = JsonConvert.DeserializeObject(json);
                if (deserialize is JObject jsonObject)
                {
                    ExcelDataBake dataBake = new()
                    {
                        JsonName = fileName,
                        CSharpName = type.FullName,
                        Data = new(),
                    };
                    foreach (var property in jsonObject.Properties())
                    {
                        if (property.First != null)
                        {
                            try
                            {
                                object dataObject = property.First.ToObject(type);
                                dataBake.Data.Add(dataObject);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"配置({fileName})反序列化出现问题：{e}");
                            }
                        }
                    }
                    byte[] bytes = SerializeHelper.Serialize(dataBake);
                    var bakeFileFull =Path.ChangeExtension(Path.Combine(targetDir, fileName), ".bytes") ;
                    FilesUtility.WriteBinaryFile(bakeFileFull, bytes);
                    Debug.Log($"write config (\"{fileName}\") file to binary completed (count:{dataBake.Data.Count}) \n {bakeFileFull}");
                }
                else
                {
                    Debug.LogError($"反序列化的对象不是JObject：{fileName},deserialize:{deserialize}");
                }
            }     
            
        }
    }
}