using MicroFarm.Configs;
using MicroFarm.Helpers;
using MicroFarm.Managers;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        /// 鱼的名字
        /// </summary>
        public string Name { get; set; }
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
        public double Size { get; set; }
        /// <summary>
        /// 角度
        /// </summary>
        public int Angle
        {
            get { return _Angle; }
            private set { _Angle = value; NotifyPropertyChanged(nameof(Angle)); }
        }
        private int _Angle = 0;
        /// <summary>
        /// 目标左坐标
        /// </summary>
        public double AimLeft { get; set; }
        /// <summary>
        /// 目标上坐标
        /// </summary>
        public double AimTop { get; set; }
        /// <summary>
        /// 图片
        /// </summary>
        public string Source
        {
            get
            {
                return _Source;
            }
            set
            {
                _Source = Path.Combine(Environment.CurrentDirectory, @$"Resources/Images/Fish/{value}");
                NotifyPropertyChanged(nameof(Source));
            }
        }
        private string _Source;
        /// <summary>
        /// 实时速度
        /// </summary>
        public int RealTimeSpeed { get; set; }
        /// <summary>
        /// 动画引用
        /// </summary>
        public Storyboard Storyboard { get; set; }
        /// <summary>
        /// 绑定元素
        /// </summary>
        public ContentPresenter ContentPresenter { get; set; }
        /// <summary>
        /// 是否死亡
        /// </summary>
        public bool IsDeath { get; set; }
        /// <summary>
        /// 是否怀孕
        /// </summary>
        public bool IsReproduction { get; set; }
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
            //出发生育事件
            TriggerReproduction();

            //打印日志
            DebugHelper.WriteLine($"------------------ Referesh ----------------");
            DebugHelper.WriteLine($"|\tId={Id}");
            DebugHelper.WriteLine($"|\tAimLeft={AimLeft}\tAimTop={AimTop}");
            DebugHelper.WriteLine($"|\tLeft={Left}\tTop={Top}");
            DebugHelper.WriteLine($"|\tAngle={Angle}");
            DebugHelper.WriteLine($"|\tRealTimeSpeed={RealTimeSpeed}");
            DebugHelper.WriteLine($"|\tSize={Size}");
            DebugHelper.WriteLine($"|\tAge={Age}");
            DebugHelper.WriteLine($"--------------------------------------------");
        }

        /// <summary>
        /// 刷新目标
        /// 默认有一定几率改变目标
        /// </summary>
        protected void RefereshAim()
        {
            var window = GameContext.Instance.AquariumWindow;

            var maxLeft = (int)((window.ActualWidth > 0 ? window.ActualWidth : window.Width) - Size);
            var maxTop = (int)((window.ActualHeight > 0 ? window.ActualHeight : window.Height) - Size);

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
                GameContext.Instance.WriteLog_Aquarium("有一条鱼死亡了...");

                //如果动画没停，先将动画停止
                if (Storyboard != null && Storyboard.GetCurrentState() != ClockState.Stopped)
                {
                    Storyboard.Stop();
                    Storyboard = null;
                }
            }
        }
        /// <summary>
        /// 触发生育
        /// </summary>
        /// <param name="collection"></param>
        public void TriggerReproduction()
        {
            //如果已经怀孕或者已经死亡，则不会触发
            if (IsReproduction || IsDeath)
                return;

            //如果生育率为0，则不处理生育事件
            if (_Meta.ReproductionRate <= 0)
                return;

            //校验处于生育年龄
            if (Age < _Meta.AdultAge || Age >= _Meta.MaxAge)
                return;

            //关联的种类的元数据
            var relaMeta = _Meta.ReproductionRelationCategory == Category ? _Meta : FishManager.GetFishMeta(_Meta.ReproductionRelationCategory);

            //校验是否存在关联到达生育年龄的鱼
            if (!GameContext.Instance.FishCollection.Any(t => relaMeta.AdultAge <= t.Age && t.Age < relaMeta.MaxAge))
                return;

            //命中生育率，则触发生育
            if (RandomHelper.IsHitRate(_Meta.ReproductionRate))
                IsReproduction = true;
        }
        /// <summary>
        /// 成长事件
        /// </summary>
        public void GrowUp()
        {
            Age++;
        }
    }
}
