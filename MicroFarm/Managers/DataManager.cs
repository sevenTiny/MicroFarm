using MicroFarm.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        /// 金币数
        /// </summary>
        public int Gold { get; set; }
        /// <summary>
        /// 鱼数据
        /// </summary>
        [XmlArrayItem(ElementName = "Fish")]
        public List<SaveFish> FishData { get; set; }
    }

    public class SaveFish
    {
        public string Id { get; set; }
        public int Category { get; set; }
        public int Age { get; set; }
        public string BirthDay { get; set; }

        public static List<Fish> ToFish(List<SaveFish> saveFishs)
        {
            if (saveFishs == null || !saveFishs.Any())
                return new List<Fish>();

            return saveFishs.Select(t => new Fish
            {
                Id = t.Id,
                Category = t.Category,
                Age = t.Age,
                BirthDay = t.BirthDay
            }).ToList();
        }

        public static List<SaveFish> ToSaveFish(List<Fish> fishs)
        {
            if (fishs == null || !fishs.Any())
                return new List<SaveFish>();

            return fishs.Select(t => new SaveFish
            {
                Id = t.Id,
                Category = t.Category,
                Age = t.Age,
                BirthDay = t.BirthDay
            }).ToList();
        }
    }
}
