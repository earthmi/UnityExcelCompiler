using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;


namespace excel2json
{
    /// <summary>
    /// 应用程序
    /// </summary>
    sealed partial class Program
    {
        /// <summary>
        /// 应用程序入口
        /// </summary>
        /// <param name="args">命令行参数</param>
        [STAThread]
        static void Main(string[] args)
        {

            // #region Debug专用
            //
            // args = new[] { "--export","E:\\UnityProjects\\T5GZ90U_Squeeze\\T5GZ90U_Squeeze\\ExcelCompile" };
            //
            // #endregion
            if (args!=null && args.Length ==2)
            {
                string excelCompileRoot = args[1];
                ExecuteCompile(excelCompileRoot);
            }
            else
            {
                Console.WriteLine($"参数错误，无法解析");
            }

        }
        

        const int HeaderRow = 3;
        static void ExecuteCompile(string rootPath)
        {
            Console.WriteLine($"检测根路径：{rootPath}");
            var excelPath = Path.Combine(rootPath, CommonDefine.ExcelFolder);
            var jsonPath = Path.Combine(rootPath, CommonDefine.JsonFolder);
            var csharpPath = Path.Combine(rootPath, CommonDefine.CSharpDefineFolder);
            if (!Directory.Exists(excelPath))
            {
                throw new Exception($"无法找到excel文件夹：{excelPath}");
            }

            if (!Directory.Exists(jsonPath))
            {
                throw new Exception($"无法找到Json文件夹：{jsonPath}");
            }
            if (!Directory.Exists(csharpPath))
            {
                throw new Exception($"无法找到CSharp文件夹：{csharpPath}");
            }
            //先删了之前生成的文件
            var startTime = DateTime.Now;
            FileUtility.ClearDirectory(jsonPath);
            FileUtility.ClearDirectory(csharpPath);
            
            Encoding cd = new UTF8Encoding(false);

            var extractPath = Path.Combine(rootPath, CommonDefine.ExtraFolder);
            var extraStructFullPath =Path.Combine(extractPath, CommonDefine.ExtraStructJson);
            var extraInfo = JsonConvert.DeserializeObject<Dictionary<string, ExtraStructInfo>>(File.ReadAllText(extraStructFullPath));
            var extraCSharp = ExtraExcelStructHandler.Handle(extraInfo);
            var extraCSharpFullPath =Path.Combine(csharpPath, "DataTableExtra.cs");
            SaveToFile(extraCSharpFullPath, extraCSharp,cd);
            
            //开始处理excel源文件
            var taskList = new List<Task>();
            var files = FileUtility.GetFilesFull(excelPath);
            StringBuilder csharpJsonMapping = new StringBuilder();

            for (int i = 0; i < files.Count; i++)
            {
                var fullPath = files[i];
                var fileName = Path.GetFileNameWithoutExtension(fullPath);
                var idx = fileName.LastIndexOf('_');
                var configName = fileName;
                if (idx != -1)
                {
                    configName = fileName.Substring(idx+1);
                }

                if (FileUtility.HaveNonEnglish(configName))
                {
                    throw new Exception($"该excel表文件名(” {fileName} “)不符合规范，如需中文命名，请按照 “中文_英文“ 的命名形式");
                }
                char firstChar = char.ToUpperInvariant(configName[0]);
                string rest = configName.Substring(1);
                var realName = firstChar + rest;
                configName = $"Config{realName}";
                // var configName = Path.GetFileNameWithoutExtension(fullPath);
                var jsonFull =Path.ChangeExtension(Path.Combine(jsonPath, realName), ".json") ;
                var csharpFull =Path.ChangeExtension(Path.Combine(csharpPath, configName), ".cs") ;
                csharpJsonMapping.AppendFormat(CommonDefine.CSharpMappingJsonDicElement, configName,realName);
                csharpJsonMapping.AppendLine();
                var task = Task.Run((() =>
                {
                    try
                    {
                        Console.WriteLine($"正在处理表：{configName}");
                        ExcelLoader excel = new ExcelLoader(fullPath, HeaderRow);
                        //-- 生成C#定义文件
                        CSDefineGenerator generator = new CSDefineGenerator(configName, excel, "#");
                        generator.SaveToFile(csharpFull, cd);
                        //-- export
                        JsonExporter exporter = new JsonExporter(excel, false, false, "yyyy/MM/dd", false, HeaderRow, "#", true, false);
                        exporter.SaveToFile(jsonFull, cd);
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"导出表“ {configName} ”时出现问题:\n{e}");
                    }
                }));
                taskList.Add(task);
            }

            Task.WaitAll(taskList.ToArray());
            string tableDefineCsharp = string.Format(CommonDefine.TableDefineCSharp,csharpJsonMapping);
            var defineFilePath = Path.ChangeExtension(Path.Combine(csharpPath, "TableDefine"), ".cs") ;
            SaveToFile(defineFilePath,tableDefineCsharp,cd);
            Console.WriteLine($"完成处理{files.Count.ToString()}个excel表，总耗时：{(DateTime.Now - startTime).TotalSeconds} 秒");

        }
        public static void SaveToFile(string filePath,  string content,Encoding encoding)
        {
            //-- 保存文件
            using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (TextWriter writer = new StreamWriter(file, encoding))
                {
                    writer.Write(content);
                    Console.WriteLine($"成功写入文件：{filePath}");
                }
                    
            }
        }
        /// <summary>
        /// 根据命令行参数，执行Excel数据导出工作
        /// </summary>
        /// <param name="options">命令行参数</param>
        private static void Run(Options options)
        {

            //-- Excel File 
            string excelPath = options.ExcelPath;
            string excelName = Path.GetFileNameWithoutExtension(options.ExcelPath);

            //-- Header
            int header = options.HeaderRows;

            //-- Encoding
            Encoding cd = new UTF8Encoding(false);
            if (options.Encoding != "utf8-nobom")
            {
                foreach (EncodingInfo ei in Encoding.GetEncodings())
                {
                    Encoding e = ei.GetEncoding();
                    if (e.HeaderName == options.Encoding)
                    {
                        cd = e;
                        break;
                    }
                }
            }

            //-- Date Format
            string dateFormat = options.DateFormat;

            //-- Export path
            string exportPath;
            if (options.JsonPath != null && options.JsonPath.Length > 0)
            {
                exportPath = options.JsonPath;
            }
            else
            {
                exportPath = Path.ChangeExtension(excelPath, ".json");
            }

            //-- Load Excel
            ExcelLoader excel = new ExcelLoader(excelPath, header);

            //-- export
            JsonExporter exporter = new JsonExporter(excel, options.Lowcase, options.ExportArray, dateFormat, options.ForceSheetName, header, options.ExcludePrefix, options.CellJson, options.AllString);
            exporter.SaveToFile(exportPath, cd);

            //-- 生成C#定义文件
            if (!string.IsNullOrEmpty(options.CSharpPath))
            {
                CSDefineGenerator generator = new CSDefineGenerator(excelName, excel, options.ExcludePrefix);
                generator.SaveToFile(options.CSharpPath, cd);
            }
        }
    }
}
