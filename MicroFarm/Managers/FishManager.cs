using MicroFarm.Helpers;
using MicroFarm.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace MicroFarm.Managers
{
    internal class FishManager
    {
        /// <summary>
        /// 鱼属性配置
        /// </summary>
        private static string FishMetaDataFile => Path.Combine(Environment.CurrentDirectory, "Resources/Configs/FishMetaData.xml");
        private static Dictionary<int, FishMetaData> _FishMetaDic = new Dictionary<int, FishMetaData>();

        static FishManager()
        {
            _FishMetaDic = SerializeHelper.LoadXml<List<FishMetaData>>(FishMetaDataFile).ToDictionary(k => k.Category);
        }

        /// <summary>
        /// 获取元数据
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public static FishMetaData GetFishMeta(int category)
        {
            return _FishMetaDic[category];
        }

        /// <summary>
        /// 根据‘种’生成一个个体
        /// </summary>
        /// <param name="species">种</param>
        /// <returns>个体</returns>
        /// <exception cref="Exception">元数据未找到</exception>
        public static Fish GenerateFish(int species)
        {
            //在同种间随机选择一个类
            var selectedMeta = _FishMetaDic.Values
                .Where(t => t.Species == species)
                .OrderBy(t => Guid.NewGuid())
                .FirstOrDefault();

            if (selectedMeta == null)
                throw new Exception($"未找到Species={species}的鱼元数据");

            return AttatchMetaProperty(new Fish
            {
                Id = Guid.NewGuid().ToString(),
                Name = RandomHelper.GenerateSurname(),
                Category = selectedMeta.Category,
                Age = selectedMeta.BirthAge,
                Source = selectedMeta.Source,
                BirthDay = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            }.Init());
        }

        /// <summary>
        /// 附加元属性
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Fish AttatchMetaProperty(Fish source)
        {
            var meta = source.CurrentMeta;

            source.Source = meta.Source;

            return source;
        }

        /// <summary>
        /// 添加新生鱼
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="fish"></param>
        public static void AddReproduction(ObservableCollection<Fish> collection, Fish fish)
        {
            if (fish.IsReproduction)
            {
                //新生一个鱼
                collection.Add(GenerateFish(fish.CurrentMeta.Species));
                //标记成未怀孕
                fish.IsReproduction = false;
            }
        }

        /// <summary>
        /// 清理死亡尸体
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="fish"></param>
        public static void ClearDeathBody(ObservableCollection<Fish> collection, Fish fish)
        {
            if (fish.IsDeath)
                collection.Remove(fish);
        }
    }

    /// <summary>
    /// 鱼元属性
    /// 详细介绍见 FishMetaData.xml
    /// </summary>
    public class FishMetaData
    {
        public string Name { get; set; }
        public int Category { get; set; }
        public int Species { get; set; }
        public int BirthAge { get; set; }
        public int AdultAge { get; set; }
        public int MaxAge { get; set; }
        public int DefaultSpeed { get; set; }
        public double MaxSize { get; set; }
        public string Source { get; set; }
        public double NormalDeathRate { get; set; }
        public double MaxAgeDeathRate { get; set; }
        public double ReproductionRate { get; set; }
        public int ReproductionRelationCategory { get; set; }
    }
}