using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            this.ImagePairList = new BindingList<ImagePair>();
            this.InitializeComponent();
        }

        /// <summary>
        /// 用于评估图像质量的图像对列表。
        /// </summary>
        public IList<ImagePair> ImagePairList { get; }

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
                    addDialog.SrcImageFileList.Count,
                    addDialog.CmpImageFileList.Count);
                for (int i = 0; i < length; i++)
                {
                    this.ImagePairList.Add(new ImagePair(
                        addDialog.SrcImageFileList[i].FullName,
                        addDialog.CmpImageFileList[i].FullName));
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
            int selectedIndex = this.imagePairDataGrid.SelectedIndex;
            if ((selectedIndex >= 0) && (selectedIndex < this.ImagePairList.Count))
            {
                this.ImagePairList.RemoveAt(selectedIndex);
                this.imagePairDataGrid.SelectedIndex = selectedIndex - 1;
            }
        }

        /// <summary>
        /// 按下清空按钮，移除所有图像对。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            this.ImagePairList.Clear();
        }

        /// <summary>
        /// 将图像质量评估结果复制到剪贴板。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            string header = "\"No\",\"SrcImage\",\"CmpImage\",\"PSNR\",\"SSIM\"," + Environment.NewLine;
            var stringBuilder = new StringBuilder(header);
            for (int i = 0; i < this.ImagePairList.Count; i++)
            {
                stringBuilder.Append($"{i + 1},");
                stringBuilder.Append($"\"{this.ImagePairList[i].ImageFile1.Name}\",");
                stringBuilder.Append($"\"{this.ImagePairList[i].ImageFile2.Name}\",");
                stringBuilder.Append($"{this.ImagePairList[i].Psnr},");
                stringBuilder.Append($"{this.ImagePairList[i].Ssim},");
                stringBuilder.Append(Environment.NewLine);
            }
            Clipboard.SetText(stringBuilder.ToString());
        }
    }
}
