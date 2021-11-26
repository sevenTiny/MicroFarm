using System.Windows;

namespace MicroFarm
{
    internal class GameContext
    {
        private GameContext() { }
        /// <summary>
        /// 水族窗体
        /// </summary>
        public static Window AquariumWindow { get; set; }
        /// <summary>
        /// 追踪周期是否完成
        /// </summary>
        public static bool IsAddCycleEventFinished_Fish { get; set; } = false;
    }
}
