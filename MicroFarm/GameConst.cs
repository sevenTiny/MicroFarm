namespace MicroFarm.Configs
{
    internal class GameConst
    {
        /// <summary>
        /// 最小Left
        /// </summary>
        public const int MinLeft = 50;
        /// <summary>
        /// 最小Top
        /// </summary>
        public const int MinTop = 50;
        /// <summary>
        /// 最大的Left
        /// </summary>
        public static int MaxLeft { get; set; }
        /// <summary>
        /// 最大Top
        /// </summary>
        public static int MaxTop { get; set; }
        /// <summary>
        /// 刷新目标几率
        /// </summary>
        public const double RefereshAimRate = 0.1;
        /// <summary>
        /// 刷新频率（秒）
        /// </summary>
        public const int RefereshIntervalSeconds = 10;
        /// <summary>
        /// 一个周期（秒）
        /// </summary>
        public const int CycleIntervalSeconds = 60;
    }
}
