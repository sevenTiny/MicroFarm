using MicroFarm.Configs;
using MicroFarm.Helpers;
using MicroFarm.Managers;
using MicroFarm.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public partial class Aquarium : Window, INotifyPropertyChanged
    {
        #region 界面控制属性
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
        /// 鱼类集合
        /// </summary>
        public ObservableCollection<Fish> FishCollection { get; set; } = new ObservableCollection<Fish>();
        /// <summary>
        /// 启动进度
        /// </summary>
        public int StartProgressBarValue
        {
            get { return _StartProgressBarValue; }
            set { _StartProgressBarValue = value; NotifyPropertyChanged(nameof(StartProgressBarValue)); }
        }
        private int _StartProgressBarValue;
        /// <summary>
        /// 启动界面显示
        /// </summary>
        public string StartViewVisibility
        {
            get { return _StartViewVisibility; }
            set { _StartViewVisibility = value; NotifyPropertyChanged(nameof(StartViewVisibility)); }
        }
        private string _StartViewVisibility = "Visible";
        #endregion

        private DispatcherTimer _timer, _cycleTimer;

        public Aquarium()
        {
            InitializeComponent();
            //绑定窗体
            GameContext.AquariumWindow = this;
            //绑定上下文
            this.DataContext = this;
            //启动定时器
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(GameConst.RefereshIntervalSeconds) };
            _timer.Tick += RefereshEvent;
            _timer.Start();

            _cycleTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(GameConst.CycleIntervalSeconds) };
            _cycleTimer.Tick += CycleEvent;
            _cycleTimer.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //初始化水族数据
            InitAquarium();
            //延时1s后启动水族
            IProgress<int> progress = new Progress<int>(val => RefereshEvent(null, null));
            Task.Run(() => { Thread.Sleep(1000); progress.Report(1); });
        }

        /// <summary>
        /// 初始化水族数据
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
                var diff = DateTime.Now - lastSaveData;

                //最大追踪6个小时
                int addMinutes = diff > TimeSpan.FromHours(6) ? 360 : (int)diff.TotalMilliseconds;

                OutPutHelper.WriteLine($"距离上次保存将追踪[{addMinutes}]个周期!");
                OutPutHelper.WriteLine("开始追踪周期...");

                Task.Run(() =>
                {
                    for (int i = 0; i < addMinutes; i++)
                    {
                        //执行周期函数
                        CycleExecute();
                        //进度条
                        StartProgressBarValue = (int)((double)i / addMinutes * 100);
                        Thread.Sleep(10);
                    }

                    this.Dispatcher.Invoke(() =>
                    {
                        OutPutHelper.WriteLine($"追踪周期完成!");
                        GameContext.IsAddCycleEventFinished_Fish = true;
                        StartViewVisibility = "Hidden";
                    });
                });
            }
        }

        /// <summary>
        /// 刷新函数事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="a"></param>
        private void RefereshEvent(object sender, EventArgs a)
        {
            foreach (var item in VisualHelper.FindVisualChildren<ContentPresenter>(fishItems))
            {
                //获取实例
                if (item.DataContext is not Fish fish)
                    continue;

                //绑定元素到对象
                fish.Binding(item);

                //清理死亡尸体
                FishManager.ClearDeathBody(FishCollection, fish);
            }
        }

        /// <summary>
        /// 周期函数事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="a"></param>
        private void CycleEvent(object sender, EventArgs a)
        {
            if (!GameContext.IsAddCycleEventFinished_Fish)
            {
                OutPutHelper.WriteLine($"周期追踪还未完成，暂时无法触发新的周期！");
                return;
            }

            OutPutHelper.WriteLine("正在保存...");

            //执行周期函数
            CycleExecute();

            //保存数据
            DataManager.SaveData(new AquariumData
            {
                LastSavaTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                FishData = FishCollection.ToList()
            });

            OutPutHelper.WriteLine("保存成功!");
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

            #region 生育事件
            //IProgress<List<Fish>> reproduction = new Progress<List<Fish>>(fishes =>
            //{
            //    this.Dispatcher.Invoke(() => { fishes.ForEach(item => FishCollection.Add(item)); });
            //});

            //Task.Run(() =>
            //{
            var newFishes = FishManager.Reproduction(FishCollection);

            if (newFishes.Any())
                this.Dispatcher.Invoke(() => { newFishes.ForEach(item => FishCollection.Add(item)); });
            //reproduction.Report(newFishes);
            //});
            #endregion
        }
    }
}