using MicroFarm.Models;
using MicroFarm.Windows;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace MicroFarm
{
    public class GameContext : INotifyPropertyChanged
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

        private GameContext() { }
        public static readonly GameContext Instance = new();

        /// <summary>
        /// 水族窗体
        /// </summary>
        public Aquarium AquariumWindow
        {
            get { return _AquariumWindow; }
            set
            {
                _AquariumWindow = value;
                //绑定水族控件
                _logStoryBoard_Aquarium = new Storyboard();
                var animation = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(GameConst.LogDisplayTimeSeconds)));
                Storyboard.SetTarget(animation, AquariumWindow.logTextBox);
                Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));
                _logStoryBoard_Aquarium.Children.Add(animation);
                //文字消失时，清除历史文字
                _logStoryBoard_Aquarium.Completed += (sender, e) =>
                {
                    //如果太长，则清空
                    if (_AquariumWindow.logTextBox.Text.Length > 10000)
                        _AquariumWindow.logTextBox.Text = string.Empty;
                };
                //绑定鱼数量事件
                FishCollection.CollectionChanged += (sender, e) => { FishCount = FishCollection.Count; };
            }
        }
        private Aquarium _AquariumWindow;
        /// <summary>
        /// 周期数
        /// </summary>
        public long CycleNumber { get; set; }
        /// <summary>
        /// 追踪周期是否完成
        /// </summary>
        public bool IsAddCycleEventFinished_Fish { get; set; } = false;
        /// <summary>
        /// 金币数
        /// </summary>
        public int Gold { get => _Gold; set { _Gold = value; NotifyPropertyChanged(nameof(Gold)); } }
        private int _Gold = 0;
        /// <summary>
        /// 鱼类集合
        /// </summary>
        public ObservableCollection<Fish> FishCollection { get; set; } = new ObservableCollection<Fish>();
        /// <summary>
        /// 鱼数量
        /// </summary>
        public int FishCount { get => _FishCount; set { _FishCount = value; NotifyPropertyChanged(nameof(FishCount)); } }
        private int _FishCount = 0;
        /// <summary>
        /// 是否展示图形界面
        /// </summary>
        public bool IsDisplayGraphics { get; set; } = true;
        /// <summary>
        /// 图形界面可见度
        /// </summary>
        public string DisplayGraphicsVisible { get; set; } = "Visible";

        /// <summary>
        /// 切换图形界面展示
        /// </summary>
        public void ChangeGraphicsVisible()
        {
            if (IsDisplayGraphics)
            {
                IsDisplayGraphics = false;
                DisplayGraphicsVisible = "Collapsed";
            }
            else
            {
                IsDisplayGraphics = true;
                DisplayGraphicsVisible = "Visible";
            }

            NotifyPropertyChanged(nameof(DisplayGraphicsVisible));
            NotifyPropertyChanged(nameof(IsDisplayGraphics));
        }

        #region 水族箱日志
        private Storyboard _logStoryBoard_Aquarium;
        /// <summary>
        /// 展示日志
        /// </summary>
        public void ViewLog()
        {
            AquariumWindow.logTextBox.Opacity = 1;

            //消失动画
            _logStoryBoard_Aquarium.Stop();
            _logStoryBoard_Aquarium.Begin();
        }
        /// <summary>
        /// 记录水族日志
        /// </summary>
        /// <param name="msg"></param>
        public void WriteLog_Aquarium(string msg)
        {
            WriteLog(AquariumWindow, AquariumWindow.logTextBox, _logStoryBoard_Aquarium, msg);
        }

        /// <summary>
        /// 通用日志方法
        /// </summary>
        /// <param name="window"></param>
        /// <param name="textBox"></param>
        /// <param name="storyboard"></param>
        /// <param name="msg"></param>
        private void WriteLog(Window window, TextBox textBox, Storyboard storyboard, string msg)
        {
            if (string.IsNullOrEmpty(msg))
                return;

            window.Dispatcher.Invoke(() =>
            {
                //显示
                textBox.Opacity = 1;
                //追加日志
                textBox.AppendText($"{msg}\n");
                //滚动到最下方
                textBox.ScrollToEnd();

                //消失动画
                storyboard.Stop();
                storyboard.Begin();
            });
        }
        #endregion
    }
}
