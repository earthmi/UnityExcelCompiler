using System;
using System.Collections.Generic;

namespace excel2json
{
    public class ExtraStructInfo
    {
        public string ClassName;
        public List<string> Fields;
    }

    public class ExtraFieldInfo
    {
        public string ClassName;
        public Dictionary<string,string> Fields;
    }
    public static class CommonDefine
    {
        public static readonly string ExcelFolder = "Excel";
        public static readonly string JsonFolder = "Json";
        public static readonly string CSharpDefineFolder = "CSharp";
        public static readonly string ExtraFolder = "Extra";
        public static readonly string ExtraStructJson = "ExtraStruct.json";
        public static readonly string ArrayType = "[]";
        public static readonly string CSharpMappingJsonDicElement = "\t\t\t{{typeof({0}),\"{1}\"}},";
        public static readonly string IdFieldName = "Id";
        
        public const string FieldString = "string";
        public const string FieldInt = "int";
        public const string FieldFloat = "float";
        public const string FieldBool = "bool";

        public static readonly string JsonArrayFormat = "[{0}]";
        public static readonly string JsonArrayElementFormat = @"{{{0}}}";
        public static readonly string ExcelHeaderDefineCSharp = @"
using System;
using System.Collections.Generic;
using System.Collections;

namespace ExcelTable
{{
    [Serializable]
    public class {0}:IExcelTable
    {{
{1}
    }}

}}";
        public static readonly string TableDefineCSharp = @"
using System;
using System.Collections.Generic;

namespace ExcelTable
{{
    public class TableDefine
    {{
        public static Dictionary<Type, string> BindingJson = new()
        {{
{0}
        }};
    }}
}}";
        public static readonly string ExtraStructCSharp = @"
using System;
using System.Collections.Generic;

namespace ExcelTable
{{
{0}
}}";
        
        public static readonly string ClassCSharp = @"
    [Serializable]
    public class {0}
    {{
{1}
    }}";
        public static readonly string FieldCSharp = "public {0} {1} {{ get; set; }}";
    }
    

}