using System.Diagnostics;

namespace MicroFarm.Helpers
{
    internal class DebugHelper
    {
        public static void WriteLine(string msg)
        {
            //加载完成才打印日志
            if (GameContext.Instance.IsAddCycleEventFinished_Fish)
                Trace.WriteLine(msg);
        }
    }
}
