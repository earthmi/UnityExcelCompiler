using System;
using System.IO;
using System.Data;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace excel2json
{
    /// <summary>
    /// 将DataTable对象，转换成JSON string，并保存到文件中
    /// </summary>
    class JsonExporter
    {
        string mContext = "";
        int mHeaderRows = 0;

        public string context {
            get {
                return mContext;
            }
        }
        DataRow _typeRow;
        /// <summary>
        /// 构造函数：完成内部数据创建
        /// </summary>
        /// <param name="excel">ExcelLoader Object</param>
        public JsonExporter(ExcelLoader excel, bool lowcase, bool exportArray, string dateFormat, bool forceSheetName, int headerRows, string excludePrefix, bool cellJson, bool allString)
        {
            mHeaderRows = headerRows - 1;
            List<DataTable> validSheets = new List<DataTable>();
            for (int i = 0; i < excel.Sheets.Count; i++)
            {
                DataTable sheet = excel.Sheets[i];

                // 过滤掉包含特定前缀的表单
                string sheetName = sheet.TableName;
                if (excludePrefix.Length > 0 && sheetName.StartsWith(excludePrefix))
                    continue;

                if (sheet.Columns.Count > 0 && sheet.Rows.Count > 0)
                    validSheets.Add(sheet);
            }

            var jsonSettings = new JsonSerializerSettings
            {
                DateFormatString = dateFormat,
                Formatting = Formatting.Indented
            };
            _typeRow = validSheets[0].Rows[0];
            if (!forceSheetName && validSheets.Count == 1)
            {   // single sheet

                //-- convert to object
                object sheetValue = convertSheet(validSheets[0], exportArray, lowcase, excludePrefix, cellJson, allString);
                //-- convert to json string
                mContext = JsonConvert.SerializeObject(sheetValue, jsonSettings);
            }
            else
            { // mutiple sheet

                Dictionary<string, object> data = new Dictionary<string, object>();
                foreach (var sheet in validSheets)
                {
                    object sheetValue = convertSheet(sheet, exportArray, lowcase, excludePrefix, cellJson, allString);
                    data.Add(sheet.TableName, sheetValue);
                }

                //-- convert to json string
                mContext = JsonConvert.SerializeObject(data, jsonSettings);
            }
        }

        private object convertSheet(DataTable sheet, bool exportArray, bool lowcase, string excludePrefix, bool cellJson, bool allString)
        {
            if (exportArray)
                return convertSheetToArray(sheet, lowcase, excludePrefix, cellJson, allString);
            else
                return convertSheetToDict(sheet, lowcase, excludePrefix, cellJson, allString);
        }

        private object convertSheetToArray(DataTable sheet, bool lowcase, string excludePrefix, bool cellJson, bool allString)
        {
            List<object> values = new List<object>();

            int firstDataRow = mHeaderRows;
            for (int i = firstDataRow; i < sheet.Rows.Count; i++)
            {
                DataRow row = sheet.Rows[i];

                values.Add(
                    convertRowToDict(sheet, row, lowcase, firstDataRow, excludePrefix, cellJson, allString)
                    );
            }

            return values;
        }

        /// <summary>
        /// 以第一列为ID，转换成ID->Object的字典对象
        /// </summary>
        private object convertSheetToDict(DataTable sheet, bool lowcase, string excludePrefix, bool cellJson, bool allString)
        {
            Dictionary<string, object> importData =
                new Dictionary<string, object>();
            int firstDataRow = mHeaderRows;
            for (int i = firstDataRow; i < sheet.Rows.Count; i++)
            {
                DataRow row = sheet.Rows[i];
                string ID = row[sheet.Columns[0]].ToString();
                if (ID.Contains("#"))
                {
                    continue;
                }
                if (ID.Length <= 0)
                    ID = $"row_{i}";

                var rowObject = convertRowToDict(sheet, row, lowcase, firstDataRow, excludePrefix, cellJson, allString);
                // 多余的字段
                // rowObject[ID] = ID;
                importData[ID] = rowObject;
            }

            return importData;
        }

        string GetJsonFormat(string source,string realType,bool isArray,ExtraFieldInfo extraFieldInfo=null)
        {
            if (!isArray && extraFieldInfo == null)
            {
                return source;
            }
            StringBuilder sb = new StringBuilder();
            if (isArray)
            {
                
                var array = source.Split(';');
                if (array.Length <= 0)
                {
                    Console.WriteLine($"查询到一个数组类型的字段( {realType} )无法被解析，详细内容：{source}");
                    return null;
                }

                if (extraFieldInfo == null)
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        var fieldValue = array[i];
                        sb.Append(GetJsonValue(fieldValue, realType));
                        if (i!= array.Length-1)
                        {
                            sb.Append(',');
                            sb.AppendLine();
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        //额外定义的类型
                        sb.Append(GetExtraFieldCell(array[i],extraFieldInfo.Fields));
                        if (i!= array.Length-1)
                        {
                            sb.Append(',');
                            sb.AppendLine();
                        }
                    }
                }
                return string.Format(CommonDefine.JsonArrayFormat,sb);
            }
            return GetExtraFieldCell(source, extraFieldInfo.Fields);
        }

        string GetExtraFieldCell(string source,Dictionary<string,string> extraFields)
        {
            var fieldValues = source.Split(',');
            StringBuilder arrayElements = new StringBuilder();
            for (int j = 0; j < extraFields.Count; j++)
            {
                var extraFieldName = extraFields.ElementAt(j).Key;
                var extraFieldType = extraFields.ElementAt(j).Value;
                var fieldValue = GetJsonDefaultValue(extraFieldType);
                if (fieldValues.Length > j)
                {
                    fieldValue = fieldValues[j].Replace("[",string.Empty).Replace("]", string.Empty);
                }

                var value = GetJsonValue(fieldValue,
                    extraFieldType); //extraFieldType == CommonDefine.FieldString ?  $"\"{fieldValue}\"" :fieldValue;
                arrayElements.Append($"\"{extraFieldName}\": {value}");
                if (j != extraFields.Count - 1)
                {
                    arrayElements.Append(',');
                    arrayElements.AppendLine();
                }
            }
            return string.Format(CommonDefine.JsonArrayElementFormat, arrayElements);
        }

        string GetJsonDefaultValue(string type)
        {
            switch (type)
            {
                case CommonDefine.FieldString:
                    return "\"\"";
                case CommonDefine.FieldBool:
                    return bool.FalseString.ToLower();
                default:
                    return 0.ToString();
            }
        }

        string GetJsonValue(string source,string type)
        {
            switch (type)
            {
                case CommonDefine.FieldString:
                    return $"\"{source}\"";
                case CommonDefine.FieldBool:
                    if (bool.TryParse(source, out bool boolResult))
                    {
                        return boolResult.ToString();
                    }
                    if (source == "0")
                    {
                        return bool.FalseString.ToLower();
                    }
                    if (source == "1")
                    {
                        return bool.TrueString.ToLower();
                    }
                    break;
                case CommonDefine.FieldInt:
                    if (int.TryParse(source, out int numResult))
                    {
                        return numResult.ToString();
                    }
                    break;
                case CommonDefine.FieldFloat:
                    if (float.TryParse(source, out float floatResult))
                    {
                        return floatResult.ToString();
                    }
                    break;
            }
            return GetJsonDefaultValue(type);
        }

        /// <summary>
        /// 把一行数据转换成一个对象，每一列是一个属性
        /// </summary>
        private Dictionary<string, object> convertRowToDict(DataTable sheet, DataRow row, bool lowcase, int firstDataRow, string excludePrefix, bool cellJson, bool allString)
        {
            var rowData = new Dictionary<string, object>();
            int col = 0;
            foreach (DataColumn column in sheet.Columns)
            {
                // 过滤掉包含指定前缀的列
                string columnName = column.ToString();
                // Console.WriteLine($"列{columnName}");
                string fieldType = _typeRow[columnName].ToString();

                if (excludePrefix.Length > 0 && columnName.StartsWith(excludePrefix))
                    continue;

                object value = row[column];
                
                var isArray = fieldType.IndexOf(CommonDefine.ArrayType, StringComparison.Ordinal)!=-1;
                var realFieldType =isArray? fieldType.Replace(CommonDefine.ArrayType,string.Empty) : fieldType;
                ExtraFieldInfo extraField = null;
                if (ExtraExcelStructHandler.ExtraFieldInfo.TryGetValue(realFieldType, out var extra))
                {
                    extraField = extra;
                }
                bool isCellJson = false;
                // 尝试将单元格字符串转换成 Json Array 或者 Json Object
                if (cellJson)
                {
                    string cellText = GetJsonFormat(value.ToString(), realFieldType,isArray, extraField);
                    if (cellText.StartsWith("[") || cellText.StartsWith("{"))
                    {
                        try
                        {
                            object cellJsonObj = JsonConvert.DeserializeObject(cellText);
                            isCellJson = true;
                            if (cellJsonObj != null)
                                value = cellJsonObj;
                        }
                        catch (Exception exp)
                        {
                        }
                    }
                }

                if (value is DBNull)
                {
                    value = getColumnDefault(sheet, column, firstDataRow);
                }
                else if (value is double doubleNum)
                { // 去掉数值字段的“.0”
                    int numInt =(int)doubleNum;
                    if (numInt == doubleNum)
                    {
                        value = numInt;
                    }
                    if (fieldType == CommonDefine.FieldBool)
                    {
                        value = numInt == 1;
                    }
                }

                //全部转换为string
                //方便LitJson.JsonMapper.ToObject<List<Dictionary<string, string>>>(textAsset.text)等使用方式 之后根据自己的需求进行解析
                if (allString && !(value is string))
                {
                    value = value.ToString();
                }

                string fieldName = column.ToString();
                var lower = fieldName.ToLower();
                if (lower == CommonDefine.IdFieldName.ToLower())
                {
                    fieldName =  CommonDefine.IdFieldName;
                }
                // 表头自动转换成小写
                if (lowcase)
                    fieldName = lower;

                if (string.IsNullOrEmpty(fieldName))
                    fieldName = $"column_{col}";

                rowData[fieldName] = value;
                col++;
            }

            return rowData;
        }

        /// <summary>
        /// 对于表格中的空值，找到一列中的非空值，并构造一个同类型的默认值
        /// </summary>
        private object getColumnDefault(DataTable sheet, DataColumn column, int firstDataRow)
        {
            for (int i = firstDataRow; i < sheet.Rows.Count; i++)
            {
                object value = sheet.Rows[i][column];
                Type valueType = value.GetType();
                if (valueType != typeof(System.DBNull))
                {
                    if (valueType.IsValueType)
                        return Activator.CreateInstance(valueType);
                    break;
                }
            }
            return "";
        }

        /// <summary>
        /// 将内部数据转换成Json文本，并保存至文件
        /// </summary>
        /// <param name="jsonPath">输出文件路径</param>
        public void SaveToFile(string filePath, Encoding encoding)
        {
            //-- 保存文件
            using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (TextWriter writer = new StreamWriter(file, encoding))
                {
                    writer.Write(mContext);
                    Console.WriteLine($"成功写入json文件：{filePath}");
                }
                    
            }
        }
    }
}
