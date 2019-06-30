using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;

namespace ImageQuality
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 初始化 <see cref="MainWindow"/> 的新实例。
        /// </summary>
        public MainWindow()
        {
            this.ImagePairs = new BindingList<ImagePair>();
            this.InitializeComponent();
        }

        /// <summary>
        /// 用于评估图像质量的图像对列表。
        /// </summary>
        public IList<ImagePair> ImagePairs { get; }

        /// <summary>
        /// 按下添加按钮，打开添加对话框。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var addDialog = new ImagePairAddWindow() { Owner = this };
            if ((bool)addDialog.ShowDialog())
            {
                int length = Math.Min(
                    addDialog.SourceFiles.Count,
                    addDialog.CompareFiles.Count);
                for (int i = 0; i < length; i++)
                {
                    this.ImagePairs.Add(new ImagePair(
                        addDialog.SourceFiles[i].FullName,
                        addDialog.CompareFiles[i].FullName));
                }
            }
        }

        /// <summary>
        /// 按下删除按钮，移除所选图像对。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = this.ImagePairDataGrid.SelectedIndex;
            if ((selectedIndex >= 0) && (selectedIndex < this.ImagePairs.Count))
            {
                this.ImagePairs[selectedIndex].Dispose();
                this.ImagePairs.RemoveAt(selectedIndex);
                this.ImagePairDataGrid.SelectedIndex = selectedIndex - 1;
            }
        }

        /// <summary>
        /// 按下清空按钮，移除所有图像对。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var pair in this.ImagePairs) { pair.Dispose(); }
            this.ImagePairs.Clear();
        }

        /// <summary>
        /// 将图像质量评估结果复制到剪贴板。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            string header = "\"No\",\"SrcImage\",\"CmpImage\",\"PSNR\",\"SSIM\"," + Environment.NewLine;
            var csvBuilder = new StringBuilder(header);
            for (int i = 0; i < this.ImagePairs.Count; i++)
            {
                csvBuilder.Append($"{i + 1},");
                csvBuilder.Append($"\"{this.ImagePairs[i].File1.Name}\",");
                csvBuilder.Append($"\"{this.ImagePairs[i].File2.Name}\",");
                csvBuilder.Append($"{this.ImagePairs[i].Psnr},");
                csvBuilder.Append($"{this.ImagePairs[i].Ssim},");
                csvBuilder.Append(Environment.NewLine);
            }
            Clipboard.SetText(csvBuilder.ToString());
        }
    }
}
