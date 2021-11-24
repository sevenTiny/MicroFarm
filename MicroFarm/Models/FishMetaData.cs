namespace MicroFarm.Models
{
    /// <summary>
    /// 鱼元属性
    /// 详细介绍见 FishMetaData.xml
    /// </summary>
    public class FishMetaData
    {
        public string Name { get; set; }
        public int Category { get; set; }
        public int BirthAge { get; set; }
        public int AdultAge { get; set; }
        public int MaxAge { get; set; }
        public int DefaultSpeed { get; set; }
        public double MaxSize { get; set; }
        public string Source { get; set; }
        public double NormalDeathRate { get; set; }
        public double MaxAgeDeathRate { get; set; }
    }
}
