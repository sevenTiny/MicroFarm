using System.Security.Cryptography;

namespace MicroFarm.Helpers
{
    public class RandomHelper
    {
        /// <summary>
        /// 获取随机数
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int GetRandomInt(int min, int max)
        {
            return RandomNumberGenerator.GetInt32(min, max);
        }

        /// <summary>
        /// 是否命中概率
        /// </summary>
        /// <param name="rate">大于0，小于1的小数</param>
        /// <returns></returns>
        public static bool IsHitRate(double rate)
        {
            return GetRandomInt(0, 100) <= rate * 100;
        }
    }
}
