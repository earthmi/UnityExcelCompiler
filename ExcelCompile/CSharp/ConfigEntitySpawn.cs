
using System;
using System.Collections.Generic;
using System.Collections;

namespace ExcelTable
{
    [Serializable]
    public class ConfigEntitySpawn:IExcelTable
    {
		public int Id {get; set;} // 生产批号
		public int Amount {get; set;} // 数量
		public int MaxHealth {get; set;} // 生命
		public string Prefab {get; set;} // 资产
		public int HorizontalNum {get; set;} // 横向排布数量
		public int HorizontalSpacing {get; set;} // 横向排布间隔
		public int Height {get; set;} // 高度
		public int VerticalSpacing {get; set;} // 垂直间隔
		public bool IsThreaten {get; set;} // 是敌人？
		public string[] Test1 {get; set;} // 但
		public int[] Test2 {get; set;} // ssss
		public DataTable_Item Test3 {get; set;} // ssss
    }

}