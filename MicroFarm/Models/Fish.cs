using MicroFarm.Configs;
using MicroFarm.Helpers;
using MicroFarm.Managers;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Animation;
using System.Xml.Serialization;

namespace MicroFarm.Models
{
    public class Fish : INotifyPropertyChanged
    {
        /// <summary>
        /// 属性改变事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #region 存储属性
        /// <summary>
        /// 对象Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 种类Id
        /// </summary>
        public int Category { get; set; }
        /// <summary>
        /// 当前年龄
        /// </summary>
        public int Age
        {
            get { return _Age; }
            set
            {
                _Age = value;
                var meta = FishManager.GetFishMeta(Category);
                Size = _Age < meta.AdultAge ? (meta.MaxSize * ((double)_Age / meta.AdultAge)) : meta.MaxSize;
                NotifyPropertyChanged(nameof(Size));
            }
        }
        private int _Age = 1;
        /// <summary>
        /// 出生日期
        /// </summary>
        public string BirthDay { get; set; }
        #endregion

        #region 临时可变属性
        /// <summary>
        /// 左坐标
        /// </summary>
        [XmlIgnore]
        public double Left
        {
            get
            {
                return _Left;
            }
            set
            {
                _Left = value;
                NotifyPropertyChanged(nameof(Left));
            }
        }
        private double _Left;
        /// <summary>
        /// 上坐标
        /// </summary>
        [XmlIgnore]
        public double Top
        {
            get
            {
                return _Top;
            }
            set
            {
                _Top = value;
                NotifyPropertyChanged(nameof(Top));
            }
        }
        private double _Top;
        /// <summary>
        /// 尺寸
        /// </summary>
        [XmlIgnore]
        public double Size { get; set; }
        /// <summary>
        /// 角度
        /// </summary>
        [XmlIgnore]
        public int Angle
        {
            get { return _Angle; }
            private set { _Angle = value; NotifyPropertyChanged(nameof(Angle)); }
        }
        private int _Angle = 0;
        /// <summary>
        /// 目标左坐标
        /// </summary>
        [XmlIgnore]
        public double AimLeft { get; set; }
        /// <summary>
        /// 目标上坐标
        /// </summary>
        [XmlIgnore]
        public double AimTop { get; set; }
        /// <summary>
        /// 图片
        /// </summary>
        [XmlIgnore]
        public string Source
        {
            get
            {
                return _Source;
            }
            set
            {
                _Source = Path.Combine(Environment.CurrentDirectory, @$"Resources/Images/{value}.png");
                NotifyPropertyChanged(nameof(Source));
            }
        }
        private string _Source;
        /// <summary>
        /// 实时速度
        /// </summary>
        [XmlIgnore]
        public int RealTimeSpeed { get; set; }
        /// <summary>
        /// 动画引用
        /// </summary>
        [XmlIgnore]
        public Storyboard Storyboard { get; set; }
        #endregion

        public Fish()
        {
            //初始化时，附加元属性
            FishManager.AttatchMetaProperty(this);
            //初始化目标位置
            RefereshAimPosition();
        }

        /// <summary>
        /// 刷新机制
        /// </summary>
        public void Referesh(double left, double top)
        {
            //更新当前位置
            Left = left;
            Top = top;

            //刷新目标位置
            RefereshAim();
            //调整角度
            RefereshAngle();
            //刷新运动速度
            RealTimeSpeed = NumberHelper.GetRandomInt(GameConst.RefereshIntervalSeconds, FishManager.GetFishMeta(Category).DefaultSpeed);

            //打印日志
            LogInfo();
        }

        /// <summary>
        /// 刷新目标
        /// 默认有一定几率改变目标
        /// </summary>
        protected void RefereshAim()
        {
            //到达目的点
            if ((Math.Abs(Left - AimLeft) <= 5 && Math.Abs(Top - AimTop) <= 5))
            {
                RefereshAimPosition();
            }
            //命中几率
            else if (NumberHelper.GetRandomInt(0, 100) < GameConst.RefereshAimRate * 100)
            {
                Trace.WriteLine("Hit Probability !!!!!!!!");
                RefereshAimPosition();
            }
        }

        private void RefereshAimPosition()
        {
            AimLeft = NumberHelper.GetRandomInt(GameConst.MinLeft, GameConst.MaxLeft);
            AimTop = NumberHelper.GetRandomInt(GameConst.MinTop, GameConst.MaxTop);
        }

        private void RefereshAngle()
        {
            //调整角度
            Angle = ((int)(Math.Atan2((AimTop - Top), (AimLeft - Left)) * 180 / Math.PI));
        }

        /// <summary>
        /// 成长事件
        /// </summary>
        public void GrowUp()
        {
            Age++;
        }

        private void LogInfo()
        {
            Trace.WriteLine("--------------------------------------------");
            Trace.WriteLine($"Id={Id}");
            Trace.WriteLine($"AimLeft={AimLeft}\tAimTop={AimTop}");
            Trace.WriteLine($"Left={Left}\tTop={Top}");
            Trace.WriteLine($"Angle={Angle}");
            Trace.WriteLine($"RealTimeSpeed={RealTimeSpeed}");
            Trace.WriteLine($"Size={Size}");
            Trace.WriteLine($"Age={Age}");
            Trace.WriteLine("--------------------------------------------");
        }
    }
}
