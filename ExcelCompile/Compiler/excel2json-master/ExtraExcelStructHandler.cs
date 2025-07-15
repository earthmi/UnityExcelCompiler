using System.Collections.Generic;
using System.Text;

namespace excel2json
{
    public static class ExtraExcelStructHandler
    {
        private static Dictionary<string,ExtraStructInfo> ExtraInfo = new Dictionary<string, ExtraStructInfo>();
        public static Dictionary<string,ExtraFieldInfo> ExtraFieldInfo = new Dictionary<string, ExtraFieldInfo>();
        public static string Handle(Dictionary<string, ExtraStructInfo> extraInfo)
        {
            StringBuilder builder = new StringBuilder();
            ExtraInfo =extraInfo;
            foreach (var VARIABLE in ExtraInfo)
            {
                var className = VARIABLE.Key;
                var fields = VARIABLE.Value.Fields;
                ExtraFieldInfo extraFieldInfo = new ExtraFieldInfo();
                extraFieldInfo.ClassName =$"DataTable_{className}";
                extraFieldInfo.Fields = new Dictionary<string, string>();
                StringBuilder fieldBuilder = new StringBuilder();

                for (int i = 0; i < fields.Count; i++)
                {
                    var (name,type) = GetFieldNames(fields[i]);
                    if (string.IsNullOrEmpty(name)  || string.IsNullOrEmpty(type))
                    {
                        continue;
                    }
                    extraFieldInfo.Fields.Add(name, type);
                    fieldBuilder.AppendFormat($"\t\t{CommonDefine.FieldCSharp}",type,name);
                    fieldBuilder.AppendLine();
                }
                builder.AppendFormat(CommonDefine.ClassCSharp,extraFieldInfo.ClassName,fieldBuilder);
                builder.AppendLine();
                ExtraFieldInfo[className] = extraFieldInfo;
                
            }
            var finalExtraCSharp = string.Format(CommonDefine.ExtraStructCSharp,builder.ToString());
            return finalExtraCSharp;
        }

        public static (string,string) GetFieldNames(string fieldWithType)
        {
            var split = fieldWithType.Split(':');
            if (split.Length != 2)
            {
                return (null, null);
            }
            return (split[0], split[1]);
        }
    }
}