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
        public static readonly GameContext Instance = new GameContext();

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
                var animation = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(10)));
                Storyboard.SetTarget(animation, AquariumWindow.logTextBox);
                Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));
                _logStoryBoard_Aquarium.Children.Add(animation);
                //文字消失时，清除历史文字
                _logStoryBoard_Aquarium.Completed += (sender, e) => { _AquariumWindow.logTextBox.Text = String.Empty; };
                //绑定鱼数量事件
                FishCollection.CollectionChanged += (sender, e) => { FishCount = FishCollection.Count; };
            }
        }
        private Aquarium _AquariumWindow;
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

        #region 水族箱日志
        private Storyboard _logStoryBoard_Aquarium;
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
                //如果太长，则清空
                if (textBox.Text.Length > 1000)
                    textBox.Text = string.Empty;

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
