using MicroFarm.Models;
using System;
using System.Collections.Generic;
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
        private static Dictionary<int, FishMetaData> _FishMetaDic = null;

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

        public static Fish GenerateFish(int category)
        {
            var meta = GetFishMeta(category);

            if (meta == null)
                throw new Exception($"未找到id={category}的鱼元数据");

            return AttatchMetaProperty(new Fish
            {
                Id = Guid.NewGuid().ToString(),
                Category = category,
                Age = meta.BirthAge,
                Source = meta.Source,
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
    }
}