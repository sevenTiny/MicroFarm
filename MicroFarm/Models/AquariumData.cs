using System;
using System.Collections.Generic;

namespace MicroFarm.Models
{
    /// <summary>
    /// 水族数据
    /// </summary>
    public class AquariumData
    {
        /// <summary>
        /// 上次保存时间
        /// </summary>
        public string LastSavaTime { get; set; }
        /// <summary>
        /// 鱼数据
        /// </summary>
        public List<Fish> FishData { get; set; }
    }
}
