using MicroFarm.Configs;
using MicroFarm.Helpers;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Animation;

namespace MicroFarm.Models
{
    public abstract class FishBase : INotifyPropertyChanged
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

        /// <summary>
        /// 对象Id
        /// </summary>
        public string Id { get; set; }

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
                _Source = Path.Combine(Environment.CurrentDirectory, @$"Resources/Images/{value}.png");
                NotifyPropertyChanged(nameof(Source));
            }
        }
        private string _Source;

        /// <summary>
        /// 移动速度,最快为10，值越大，越慢
        /// </summary>
        public abstract int Speed { get; }
        /// <summary>
        /// 实时速度
        /// </summary>
        public int RealTimeSpeed { get; set; }

        /// <summary>
        /// 动画引用
        /// </summary>
        public Storyboard Storyboard { get; set; }

        protected FishBase()
        {
            //初始化位置
            //Left = new Random(DateTime.Now.Millisecond).Next(GameConst.MinLeft, GameConst.MaxLeft);
            //Top = new Random(DateTime.Now.Millisecond).Next(GameConst.MinTop, GameConst.MaxTop);

            //初始化目标位置
            RefereshAimPosition();
        }

        /// <summary>
        /// 刷新机制
        /// </summary>
        public void Referesh(double left, double top)
        {
            Left = left;
            Top = top;

            //刷新目标位置
            RefereshAim();
            //调整角度
            RefereshAngle();
            //刷新运动速度
            RealTimeSpeed = NumberHelper.GetRandomInt(GameConst.RefereshIntervalSeconds, Speed);

            //打印日志
            LogInfo();
        }

        /// <summary>
        /// 刷新目标
        /// 默认有一定几率改变目标
        /// </summary>
        protected virtual void RefereshAim()
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

        ///// <summary>
        ///// 刷新位置
        ///// </summary>
        //private void RefereshPosition()
        //{
        //    var horizontal = AimLeft - Left;
        //    var horizontalStep = 0;

        //    if (horizontal > 0)
        //        horizontalStep = 1;
        //    else if (horizontal < 0)
        //        horizontalStep = -1;
        //    else
        //        RefereshAngle();

        //    var vertical = AimTop - Top;
        //    var verticalStep = 0;

        //    if (vertical > 0)
        //        verticalStep = 1;
        //    else if (vertical < 0)
        //        verticalStep = -1;
        //    else
        //        RefereshAngle();

        //    //移动
        //    Left = Left + horizontalStep;
        //    Top = Top + verticalStep;
        //}

        private void RefereshAngle()
        {
            //调整角度
            Angle = ((int)(Math.Atan2((AimTop - Top), (AimLeft - Left)) * 180 / Math.PI));
        }

        private void LogInfo()
        {
            Trace.WriteLine("------------------------------------------");
            Trace.WriteLine($"AimLeft={AimLeft}\tAimTop={AimTop}");
            Trace.WriteLine($"Left={Left}\tTop={Top}");
            Trace.WriteLine($"Angle={Angle}");
            Trace.WriteLine($"RealTimeSpeed={RealTimeSpeed}");
        }
    }
}
