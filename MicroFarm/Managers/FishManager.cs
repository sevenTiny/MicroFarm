﻿using MicroFarm.Helpers;
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
            XmlSerializer serializer = new(typeof(List<FishMetaData>));
            using FileStream stream = new(FishMetaDataFile, FileMode.Open);
            var metadata = (List<FishMetaData>)serializer.Deserialize(stream);

            if (metadata == null)
                throw new Exception("加载鱼类元数据失败");

            _FishMetaDic = metadata.ToDictionary(k => k.Category);
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
                Category = selectedMeta.Category,
                Age = selectedMeta.BirthAge,
                Source = selectedMeta.Source,
                BirthDay = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        /// <summary>
        /// 附加元属性
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Fish AttatchMetaProperty(Fish source)
        {
            var meta = GetFishMeta(source.Category);

            source.Source = meta.Source;

            return source;
        }

        /// <summary>
        /// 生育事件
        /// </summary>
        /// <param name="collection"></param>
        public static void Reproduction(ObservableCollection<Fish> collection)
        {
            var newFishes = new Lazy<List<Fish>>();

            foreach (var fish in collection)
            {
                //鱼元数据
                var meta = GetFishMeta(fish.Category);

                //如果生育率为0，则不处理生育事件
                if (meta.ReproductionRate <= 0)
                    continue;

                //校验处于生育年龄
                if (fish.Age < meta.AdultAge || fish.Age >= meta.MaxAge)
                    continue;

                //关联的种类的元数据
                var relaMeta = meta.ReproductionRelationCategory == fish.Category ? meta : GetFishMeta(meta.ReproductionRelationCategory);

                //校验是否存在关联到达生育年龄的鱼
                if (!collection.Any(t => relaMeta.AdultAge <= t.Age && t.Age < relaMeta.MaxAge))
                    continue;

                //命中生育率，则触发生育
                if (RandomHelper.IsHitRate(meta.ReproductionRate))
                    newFishes.Value.Add(GenerateFish(meta.Species));
            }

            if (newFishes.IsValueCreated)
            {
                foreach (var item in newFishes.Value)
                {
                    collection.Add(item);
                }
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