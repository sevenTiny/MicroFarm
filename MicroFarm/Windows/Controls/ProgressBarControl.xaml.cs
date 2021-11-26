using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace MicroFarm.Windows.Controls
{
    /// <summary>
    /// Interaction logic for ProgressBarControl.xaml
    /// </summary>
    public partial class ProgressBarControl : UserControl
    {
        public ProgressBarControl()
        {
            InitializeComponent();

            progressBar1.DataContext = this;
        }

        /// <summary>
        /// 进度条值
        /// </summary>
        [Category("Extend Properties")]
        public int ProgressBarValue
        {
            get { return (int)GetValue(ProgressBarValueProperty); }
            set { SetValue(ProgressBarValueProperty, value); }
        }

        public static readonly DependencyProperty ProgressBarValueProperty = DependencyProperty.Register("ProgressBarValue", typeof(int), typeof(ProgressBarControl));
    }
}
