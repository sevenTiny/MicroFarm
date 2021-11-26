﻿using MicroFarm.Configs;
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
        }
    }
}