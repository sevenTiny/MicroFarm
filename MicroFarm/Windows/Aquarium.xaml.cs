﻿using MicroFarm.Configs;
using MicroFarm.Helpers;
using MicroFarm.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MicroFarm.Windows
{
    /// <summary>
    /// Interaction logic for Aquarium.xaml
    /// 水族箱
    /// </summary>
    public partial class Aquarium : Window
    {
        public ObservableCollection<FishBase> FishCollection { get; set; } = new ObservableCollection<FishBase>();
        private DispatcherTimer _timer;

        public Aquarium()
        {
            InitializeComponent();
            //绑定窗体大小改变调整活动范围
            this.SizeChanged += (sender, e) => RefereshActiveRange();
            this.StateChanged += (sender, e) => RefereshActiveRange();
            //刷新活动范围
            RefereshActiveRange();
            //初始化水族数据
            InitFishs();
            //绑定上下文
            fishItems.ItemsSource = FishCollection;
            //绑定新增事件
            FishCollection.CollectionChanged += TimerEvent;
            //启动定时器
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(GameConst.RefereshIntervalSeconds), };
            _timer.Tick += TimerEvent;
            _timer.Start();

            //延时1s后启动水族
            IProgress<int> progress = new Progress<int>(val => TimerEvent(null, null));
            Task.Run(() => { Thread.Sleep(1000); progress.Report(1); });
        }

        public void InitFishs()
        {
            var fishes = Enumerable.Range(0, 10).Select(i => new Fish
            {
                Source = "right"
            })
            .ToList();

            foreach (var item in fishes)
            {
                FishCollection.Add(item);
            }
        }

        /// <summary>
        /// 刷新活动范围
        /// </summary>
        private void RefereshActiveRange()
        {
            GameConst.MaxLeft = (int)(this.ActualWidth > 0 ? this.ActualWidth : this.Width) - 50;
            GameConst.MaxTop = (int)(this.ActualHeight > 0 ? this.ActualHeight : this.Height) - 50;
        }

        private void TimerEvent(object sender, EventArgs a)
        {
            //执行动作
            foreach (var item in VisualHelper.FindVisualChildren<ContentPresenter>(fishItems))
            {
                //https://www.cnblogs.com/hayasi/p/7102451.html

                //拿到数据
                var data = item.DataContext as FishBase;

                if (data == null)
                    continue;

                //刷新状态
                data.Referesh(Canvas.GetLeft(item), Canvas.GetTop(item));

                //如果动画没停，先将动画停止
                if (data.Storyboard != null && data.Storyboard.GetCurrentState() != ClockState.Stopped)
                {
                    data.Storyboard.Stop();
                    data.Storyboard.Children.Clear();
                }

                data.Storyboard = new Storyboard();

                var daLeft = new DoubleAnimation(data.Left, data.AimLeft, new Duration(TimeSpan.FromSeconds(data.RealTimeSpeed)));//实例化double动画处理对象，并取值
                Storyboard.SetTarget(daLeft, item);//将动画处理的对象和控件关联起来
                Storyboard.SetTargetProperty(daLeft, new PropertyPath("(Canvas.Left)"));//设置这个动画的播放位置
                data.Storyboard.Children.Add(daLeft);//将动画处理对象添加到故事板中

                var daTop = new DoubleAnimation(data.Top, data.AimTop, new Duration(TimeSpan.FromSeconds(data.RealTimeSpeed)));//实例化double动画处理对象，并取值
                Storyboard.SetTarget(daTop, item);//将动画处理的对象和控件关联起来
                Storyboard.SetTargetProperty(daTop, new PropertyPath("(Canvas.Top)"));//设置这个动画的播放位置
                data.Storyboard.Children.Add(daTop);//将动画处理对象添加到故事板中

                data.Storyboard.Begin();//开启时间线动画
            }
        }
    }
}
