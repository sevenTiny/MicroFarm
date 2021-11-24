using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFarm.Models
{
    /// <summary>
    /// 标准鱼
    /// </summary>
    public class Fish : FishBase
    {
        public Fish() : base()
        {
        }

        /// <summary>
        /// 标准鱼的速度
        /// </summary>
        public override int Speed => 20;
    }
}
