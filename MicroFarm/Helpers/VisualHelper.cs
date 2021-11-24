using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace MicroFarm.Helpers
{
    internal static class VisualHelper
    {
        /// <summary>
        /// 获取到子节点
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="depObj"></param>
        /// <returns></returns>
        public static List<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null)
                return new List<T>();

            List<T> list = new();

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                if (child == null)
                    continue;

                if (child is T t)
                    list.Add(t);

                List<T> childItems = FindVisualChildren<T>(child);

                if (childItems != null)
                    list.AddRange(childItems);
            }

            return list;
        }
    }
}
