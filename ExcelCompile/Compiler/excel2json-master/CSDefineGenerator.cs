using System;
using System.IO;
using System.Data;
using System.Text;
using System.Collections.Generic;

namespace excel2json
{
    /// <summary>
    /// 根据表头，生成C#类定义数据结构
    /// 表头使用三行定义：字段名称、字段类型、注释
    /// </summary>
    class CSDefineGenerator
    {
        struct FieldDef
        {
            public string name;
            public string type;
            public string comment;
        }

        string mCode;

        public string code {
            get {
                return this.mCode;
            }
        }

        private string configName;
        
        public CSDefineGenerator(string excelName, ExcelLoader excel, string excludePrefix)
        {
            configName = excelName;
            //-- 创建代码字符串
            StringBuilder sb = new StringBuilder();
            if (excel.Sheets.Count<=0)
            {
                mCode = null;
                return;
            }
            sb.Append(_exportSheet(excel.Sheets[0], excludePrefix));
            mCode = sb.ToString();
        }

        private string _exportSheet(DataTable sheet, string excludePrefix)
        {
            if (sheet.Columns.Count < 0 || sheet.Rows.Count < 2)
                return "";

            string sheetName = sheet.TableName;
            if (excludePrefix.Length > 0 && sheetName.StartsWith(excludePrefix))
                return "";

            // get field list
            DataRow typeRow = sheet.Rows[0];
            DataRow commentRow = sheet.Rows[1];
            StringBuilder sb = new StringBuilder();
            bool findIdType = false;
            for (int i = 0; i < sheet.Columns.Count; i++)
            {
                DataColumn column = sheet.Columns[i];
                string columnName = column.ToString();
                if (excludePrefix.Length > 0 && columnName.StartsWith(excludePrefix))
                    continue;

                FieldDef field;
                field.name = column.ToString();
                field.type = typeRow[column].ToString();
                if (string.Equals(field.name, CommonDefine.IdFieldName, StringComparison.CurrentCultureIgnoreCase))
                {
                    findIdType = true;
                    field.name = CommonDefine.IdFieldName;
                    if (field.type!= "int")
                    {
                        throw new Exception($"该表( “{configName}” )的Id类型( “{field.type}” )错误，需要设置成int类型");
                    }
                }
                var isArray = field.type.IndexOf(CommonDefine.ArrayType, StringComparison.Ordinal)!=-1;
                var realFieldType =isArray? field.type.Replace(CommonDefine.ArrayType,string.Empty) : field.type;
                
                if (ExtraExcelStructHandler.ExtraFieldInfo.TryGetValue(realFieldType, out var extraField))
                {
                    field.type = extraField.ClassName + (isArray ? CommonDefine.ArrayType :string.Empty);
                }
                field.comment = commentRow[column].ToString();
                sb.AppendFormat("\t\tpublic {0} {1} {{get; set;}} // {2}", field.type, field.name, field.comment);
                if (i!=sheet.Columns.Count-1)
                {
                    sb.AppendLine();
                }
            }

            if (!findIdType)
            {
                throw new Exception($"该表( “{configName}” )找不到Id字段，该表需要一个唯一Id字段");
            }
            var final = string.Format(CommonDefine.ExcelHeaderDefineCSharp, configName, sb);
            return final;
        }

        public void SaveToFile(string filePath, Encoding encoding)
        {
            if (string.IsNullOrEmpty(mCode))
            {
                Console.WriteLine($"无法写入：{configName},因为内容为空");
                return;
            }
            //-- 保存文件
            using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (TextWriter writer = new StreamWriter(file, encoding))
                {
                    writer.Write(mCode);
                    Console.WriteLine($"成功写入c#文件：{filePath}");

                }
            }
        }
    }
}
