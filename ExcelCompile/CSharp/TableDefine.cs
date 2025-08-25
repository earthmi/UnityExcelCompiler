
using System;
using System.Collections.Generic;

namespace ExcelTable
{
    public class TableDefine
    {
        public static Dictionary<Type, string> BindingJson = new()
        {
			{typeof(ConfigEntitySpawn),"EntitySpawn"},

        };
    }
}