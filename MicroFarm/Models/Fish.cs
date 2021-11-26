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

        private FishMetaData _Meta = null;

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
                Size = _Age < _Meta.AdultAge ? (_Meta.MaxSize * ((double)_Age / _Meta.AdultAge)) : _Meta.MaxSize;
                NotifyPropertyChanged(nameof(Size));
            }
        }
        private int _Age;
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
                _Source = Path.Combine(Environment.CurrentDirectory, @$"Resources/Images/{value}");
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
        /// <summary>
        /// 是否死亡
        /// </summary>
        [XmlIgnore]
        public bool IsDeath { get; set; } = false;
        #endregion

        /// <summary>
        /// 初始化数据
        /// </summary>
        public void Init()
        {
            //获取元数据
            _Meta = FishManager.GetFishMeta(Category);

            //初始化时，附加元属性
            FishManager.AttatchMetaProperty(this);
        }

        /// <summary>
        /// 刷新机制
        /// </summary>
        public void Referesh(double left, double top)
        {
            //更新当前位置
            Left = left; Top = top;

            //刷新目标位置
            RefereshAim();
            //调整角度
            RefereshAngle();
            //刷新运动速度
            RefereshSpeed();
            //触发死亡事件
            TriggerDeath();

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
            else if (RandomHelper.IsHitRate(GameConst.RefereshAimRate))
            {
                Trace.WriteLine("Hit Probability !!!!!!!!");
                RefereshAimPosition();
            }

            void RefereshAimPosition()
            {
                AimLeft = RandomHelper.GetRandomInt(GameConst.MinLeft, GameConst.MaxLeft);
                AimTop = RandomHelper.GetRandomInt(GameConst.MinTop, GameConst.MaxTop);
            }
        }
        /// <summary>
        /// 调整角度
        /// </summary>
        private void RefereshAngle()
        {
            //调整角度
            Angle = ((int)(Math.Atan2((AimTop - Top), (AimLeft - Left)) * 180 / Math.PI));
        }
        /// <summary>
        /// 刷新运动速度
        /// </summary>
        private void RefereshSpeed()
        {
            RealTimeSpeed = RandomHelper.GetRandomInt(GameConst.RefereshIntervalSeconds, _Meta.DefaultSpeed);
        }
        /// <summary>
        /// 触发死亡
        /// </summary>
        private void TriggerDeath()
        {
            if (Age >= _Meta.MaxAge)
            {
                if (RandomHelper.IsHitRate(_Meta.MaxAgeDeathRate))
                    IsDeath = true;
            }
            else if (RandomHelper.IsHitRate(_Meta.NormalDeathRate))
            {
                IsDeath = true;
            }
        }

        /// <summary>
        /// 成长事件
        /// </summary>
        public void GrowUp()
        {
            Age++;

            Trace.WriteLine($"----------------- GrowUp -------------------");
            Trace.WriteLine($"|\tId={Id}");
            Trace.WriteLine($"|\tAge={Age}");
            Trace.WriteLine($"--------------------------------------------");
        }

        private void LogInfo()
        {
            Trace.WriteLine($"------------------ Referesh ----------------");
            Trace.WriteLine($"|\tId={Id}");
            Trace.WriteLine($"|\tAimLeft={AimLeft}\tAimTop={AimTop}");
            Trace.WriteLine($"|\tLeft={Left}\tTop={Top}");
            Trace.WriteLine($"|\tAngle={Angle}");
            Trace.WriteLine($"|\tRealTimeSpeed={RealTimeSpeed}");
            Trace.WriteLine($"|\tSize={Size}");
            Trace.WriteLine($"|\tAge={Age}");
            Trace.WriteLine($"--------------------------------------------");
        }
    }
}
