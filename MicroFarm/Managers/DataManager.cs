using MicroFarm.Models;
using System;
using System.IO;
using System.Xml.Serialization;

namespace MicroFarm.Managers
{
    internal class DataManager
    {
        /// <summary>
        /// 鱼缸游戏数据
        /// </summary>
        private static string AquariumDataFile => Path.Combine(Environment.CurrentDirectory, "Resources/Configs/AquariumData.xml");

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <returns></returns>
        public static AquariumData LoadData()
        {
            XmlSerializer serializer = new(typeof(AquariumData));
            using FileStream stream = new(AquariumDataFile, FileMode.Open);
            var data = (AquariumData)serializer.Deserialize(stream);

            if (data == null)
                throw new Exception($"加载数据失败，请检查数据文件:{AquariumDataFile}");

            return data;
        }

        /// <summary>
        /// 保存资源
        /// </summary>
        /// <param name="data"></param>
        public static void SaveData(AquariumData data)
        {
            XmlSerializer serializer = new(typeof(AquariumData));
            using FileStream stream = new(AquariumDataFile, FileMode.Create, FileAccess.Write);
            serializer.Serialize(stream, data);
        }
    }
}
