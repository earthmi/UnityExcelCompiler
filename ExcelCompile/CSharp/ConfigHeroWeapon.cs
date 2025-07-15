
using System;
using System.Collections.Generic;
using System.Collections;

namespace ExcelTable
{
    [Serializable]
    public class ConfigHeroWeapon:IExcelTable
    {
		public int Id {get; set;} // 编号
		public string Name {get; set;} // 名称
		public int Damage {get; set;} // 伤害
		public string Prefab {get; set;} // 子弹预制体
		public string Sound {get; set;} // 射击声音
		public float FireInterval {get; set;} // 射击间隔
		public float Speed {get; set;} // 速度
    }

}