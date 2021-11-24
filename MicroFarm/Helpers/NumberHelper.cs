using System.Security.Cryptography;

namespace MicroFarm.Helpers
{
    public class NumberHelper
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
    }
}
