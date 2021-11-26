using MicroFarm.Configs;
using MicroFarm.Helpers;
using MicroFarm.Managers;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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
        /// 绑定元素
        /// </summary>
        [XmlIgnore]
        public ContentPresenter ContentPresenter { get; set; }
        /// <summary>
        /// 是否死亡
        /// </summary>
        [XmlIgnore]
        public bool IsDeath { get; set; } = false;
        #endregion

        public Fish()
        {
            //获取元数据
            _Meta = FishManager.GetFishMeta(Category);
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        public void Init()
        {
            //初始化时，附加元属性
            FishManager.AttatchMetaProperty(this);
        }

        /// <summary>
        /// 绑定元素
        /// </summary>
        /// <param name="contentPresenter"></param>
        public void Binding(ContentPresenter contentPresenter)
        {
            //如果已经绑定过了，则跳过
            if (ContentPresenter != null)
                return;

            //绑定
            ContentPresenter = contentPresenter;

            //开始动画
            StartStory();
        }

        /// <summary>
        /// 开始动画
        /// </summary>
        private void StartStory()
        {
            //刷新状态
            Referesh(Canvas.GetLeft(ContentPresenter), Canvas.GetTop(ContentPresenter));

            Storyboard = new Storyboard();

            //完成时，执行下一个动画
            Storyboard.Completed += (sender, evt) => StartStory();

            //https://www.cnblogs.com/hayasi/p/7102451.html

            var daLeft = new DoubleAnimation(Left, AimLeft, new Duration(TimeSpan.FromSeconds(RealTimeSpeed)));//实例化double动画处理对象，并取值
            Storyboard.SetTarget(daLeft, ContentPresenter);//将动画处理的对象和控件关联起来
            Storyboard.SetTargetProperty(daLeft, new PropertyPath("(Canvas.Left)"));//设置这个动画的播放位置
            Storyboard.Children.Add(daLeft);//将动画处理对象添加到故事板中

            var daTop = new DoubleAnimation(Top, AimTop, new Duration(TimeSpan.FromSeconds(RealTimeSpeed)));//实例化double动画处理对象，并取值
            Storyboard.SetTarget(daTop, ContentPresenter);//将动画处理的对象和控件关联起来
            Storyboard.SetTargetProperty(daTop, new PropertyPath("(Canvas.Top)"));//设置这个动画的播放位置
            Storyboard.Children.Add(daTop);//将动画处理对象添加到故事板中

            Storyboard.Begin();//开启时间线动画
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
            var window = Window.GetWindow(ContentPresenter);

            var maxLeft = (int)(window.ActualWidth > 0 ? window.ActualWidth : window.Width) - 50;
            var maxTop = (int)(window.ActualHeight > 0 ? window.ActualHeight : window.Height) - 50;

            AimLeft = RandomHelper.GetRandomInt(GameConst.MinLeft, maxLeft);
            AimTop = RandomHelper.GetRandomInt(GameConst.MinTop, maxTop);
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

            //死亡后，停止移动动画
            if (IsDeath)
            {
                //如果动画没停，先将动画停止
                if (Storyboard != null && Storyboard.GetCurrentState() != ClockState.Stopped)
                {
                    Storyboard.Stop();
                    Storyboard = null;
                }
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
