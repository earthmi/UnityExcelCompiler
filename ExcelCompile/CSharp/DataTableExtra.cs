
using System;
using System.Collections.Generic;

namespace ExcelTable
{

    [Serializable]
    public class DataTable_Item
    {
		public int Id { get; set; }
		public int Value { get; set; }

    }

    [Serializable]
    public class DataTable_Position
    {
		public float x { get; set; }
		public float y { get; set; }

    }

}