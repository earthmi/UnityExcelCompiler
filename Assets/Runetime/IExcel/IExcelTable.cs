using System;
using System.Collections.Generic;

namespace ExcelTable
{
    public interface IExcelTable
    {
        public int Id { get; set; }
    }
    [Serializable]
    public class ExcelDataBake
    {
        public string JsonName { get; set; }
        public string CSharpName { get; set; }
        public List<object> Data { get; set; }
    }
}