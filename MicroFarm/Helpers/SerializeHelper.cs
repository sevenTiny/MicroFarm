using System;
using System.IO;
using System.Xml.Serialization;

namespace MicroFarm.Helpers
{
    internal class SerializeHelper
    {
        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="filePath">filePath</param>
        /// <returns></returns>
        public static T LoadXml<T>(string filePath) where T : class
        {
            XmlSerializer serializer = new(typeof(T));
            using FileStream stream = new(filePath, FileMode.Open);
            var data = (T)serializer.Deserialize(stream);

            if (data == null)
                throw new Exception($"加载数据失败，请检查数据文件:{filePath}");

            return data;
        }

        /// <summary>
        /// 保存资源
        /// </summary>
        /// <param name="filePath">filePath</param>
        /// <param name="data"></param>
        public static void SaveXml<T>(string filePath, T data)
        {
            XmlSerializer serializer = new(typeof(T));
            using FileStream stream = new(filePath, FileMode.Create, FileAccess.Write);
            serializer.Serialize(stream, data);
        }
    }
}
