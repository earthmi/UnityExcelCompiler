
using System;
using System.Collections.Generic;
using System.Collections;

namespace ExcelTable
{
    [Serializable]
    public class ConfigHeroSkin:IExcelTable
    {
		public int Id {get; set;} // 编号
		public string Name {get; set;} // 名称
		public string IdleStateAnima {get; set;} // 静止动画
		public string MoveStateAnima {get; set;} // 移动动画
    }

}