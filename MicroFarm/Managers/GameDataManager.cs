using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace MicroFarm.Managers
{
    /// <summary>
    /// 游戏数据管理器
    /// </summary>
    internal class GameDataManager
    {
        private static string GameDataFile => Path.Combine(Environment.CurrentDirectory, "Resources/Configs/GameData.xml");

        private static Dictionary<string, string> _GameDataDic;

        static GameDataManager()
        {
            XElement xml = XElement.Load(GameDataFile);
            _GameDataDic = xml.Elements().ToDictionary(k => k.Name.ToString(), v => v.Value);
        }

        /// <summary>
        /// 获取Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Get(string key)
        {
            return _GameDataDic[key];
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Set(string key, string value)
        {
            _GameDataDic[key] = value;

            XElement xele = XElement.Load(GameDataFile);
            xele.SetElementValue(key, value);
            xele.Save(GameDataFile);
        }
    }
}
