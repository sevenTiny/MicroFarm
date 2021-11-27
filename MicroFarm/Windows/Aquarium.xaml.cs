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
            GameContext.Instance.AquariumWindow = this;
            //绑定上下文
            this.DataContext = GameContext.Instance;
            startViewGrid.DataContext = this;
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
            //延时1s后启动水族箱
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
            foreach (var item in SaveFish.ToFish(loadData.FishData))
            {
                item.Init();

                GameContext.Instance.FishCollection.Add(item);
            }

            #region 追踪周期事件
            //跟踪周期事件到此刻
            var lastSaveData = Convert.ToDateTime(loadData.LastSavaTime);

            if (DateTime.Now > lastSaveData)
            {
                var diff = DateTime.Now - lastSaveData;

                //最大追踪6个小时
                int addMinutes = diff > TimeSpan.FromHours(6) ? 360 : (int)diff.TotalMinutes;

                GameContext.Instance.WriteLog_Aquarium($"距离上次保存将追踪[{addMinutes}]个周期!");
                GameContext.Instance.WriteLog_Aquarium("开始追踪周期...");

                Task.Run(() =>
                {
                    for (int i = 0; i < addMinutes; i++)
                    {
                        //执行周期函数
                        CycleExecute();
                        //进度条
                        StartProgressBarValue = (int)((double)i / addMinutes * 100);

                        GameContext.Instance.WriteLog_Aquarium($"当前追踪到第:{i}周期");
                        Thread.Sleep(5);
                    }

                    this.Dispatcher.Invoke(() =>
                    {
                        GameContext.Instance.WriteLog_Aquarium($"追踪周期完成!");
                        GameContext.Instance.IsAddCycleEventFinished_Fish = true;
                        StartViewVisibility = "Hidden";
                    });
                });
            }
            #endregion
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

                //新增新生鱼
                FishManager.AddReproduction(GameContext.Instance.FishCollection, fish);

                //清理死亡尸体
                FishManager.ClearDeathBody(GameContext.Instance.FishCollection, fish);

            }
        }

        /// <summary>
        /// 周期函数事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="a"></param>
        private void CycleEvent(object sender, EventArgs a)
        {
            if (!GameContext.Instance.IsAddCycleEventFinished_Fish)
            {
                return;
            }

            GameContext.Instance.WriteLog_Aquarium("正在保存...");

            //执行周期函数
            CycleExecute();

            //保存数据
            DataManager.SaveData();

            GameContext.Instance.WriteLog_Aquarium("保存成功!");
        }

        /// <summary>
        /// 执行周期逻辑
        /// </summary>
        private void CycleExecute()
        {
            //执行周期
            foreach (var item in GameContext.Instance.FishCollection)
            {
                //成长事件
                item.GrowUp();
            }
        }

        private void DoubleAnimation_Completed(object sender, EventArgs e)
        {
            startViewGrid.Visibility = Visibility.Hidden;
        }
    }
}