using MicroFarm.Configs;
using MicroFarm.Helpers;
using MicroFarm.Managers;
using MicroFarm.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace MicroFarm.Windows
{
    /// <summary>
    /// Interaction logic for Aquarium.xaml
    /// 水族箱
    /// </summary>
    public partial class Aquarium : Window
    {
        /// <summary>
        /// 鱼类集合
        /// </summary>
        public ObservableCollection<Fish> FishCollection { get; set; } = new ObservableCollection<Fish>();
        private DispatcherTimer _timer, _cycleTimer;

        public Aquarium()
        {
            InitializeComponent();
            //绑定窗体大小改变调整活动范围
            this.SizeChanged += (sender, e) => RefereshActiveRange();
            this.StateChanged += (sender, e) => RefereshActiveRange();
            //刷新活动范围
            RefereshActiveRange();
            //初始化水族数据
            InitAquarium();
            //绑定上下文
            this.DataContext = this;
            //绑定新增事件
            FishCollection.CollectionChanged += RefereshEvent;
            //启动定时器
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(GameConst.RefereshIntervalSeconds), };
            _timer.Tick += RefereshEvent;
            _timer.Start();

            _cycleTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(GameConst.CycleIntervalSeconds) };
            _cycleTimer.Tick += CycleEvent;
            _cycleTimer.Start();

            //延时1s后启动水族
            IProgress<int> progress = new Progress<int>(val => RefereshEvent(null, null));
            Task.Run(() => { Thread.Sleep(1000); progress.Report(1); });
        }

        /// <summary>
        /// 初始化水族
        /// </summary>
        public void InitAquarium()
        {
            var loadData = DataManager.LoadData();

            //加载数据
            foreach (var item in loadData.FishData ?? new List<Fish>())
            {
                item.Init();

                FishCollection.Add(item);
            }

            //跟踪周期事件到此刻
            var lastSaveData = Convert.ToDateTime(loadData.LastSavaTime);

            if (DateTime.Now > lastSaveData)
            {
                for (int i = 0; i < (DateTime.Now - lastSaveData).Minutes; i++)
                {
                    CycleExecute();
                }
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

        /// <summary>
        /// 刷新函数事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="a"></param>
        private void RefereshEvent(object sender, EventArgs a)
        {
            // TODO: 现在每个元素用的全局定时器，同一时刻改变游动状态，有点生硬，最好改成每个元素独立事件线
            //执行动作
            foreach (var item in VisualHelper.FindVisualChildren<ContentPresenter>(fishItems))
            {
                //https://www.cnblogs.com/hayasi/p/7102451.html

                //拿到数据
                var data = item.DataContext as Fish;

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

                //如果已经死亡，就不触发移动动画了
                if (data.IsDeath)
                    continue;

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

            //清理死亡尸体
            FishManager.ClearDeathBody(FishCollection);
        }

        /// <summary>
        /// 周期函数事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="a"></param>
        private void CycleEvent(object sender, EventArgs a)
        {
            Trace.WriteLine("正在保存...");

            //执行周期函数
            CycleExecute();

            //保存数据
            DataManager.SaveData(new AquariumData
            {
                LastSavaTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                FishData = FishCollection.ToList()
            });

            Trace.WriteLine("保存成功!");
        }

        /// <summary>
        /// 执行周期逻辑
        /// </summary>
        private void CycleExecute()
        {
            //执行周期
            foreach (var item in FishCollection)
            {
                //成长事件
                item.GrowUp();
            }

            //触发生育事件
            FishManager.Reproduction(FishCollection);
            //清理死亡尸体
            FishManager.ClearDeathBody(FishCollection);
        }
    }
}