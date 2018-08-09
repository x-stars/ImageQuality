using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;

namespace ImageQuality
{
    /// <summary>
    /// ImagePairAddWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ImagePairAddWindow : Window
    {
        /// <summary>
        /// 初始化 <see cref="ImagePairAddWindow"/> 的新实例。
        /// </summary>
        public ImagePairAddWindow()
        {
            this.SrcImageFileList = new BindingList<FileInfo>();
            this.CmpImageFileList = new BindingList<FileInfo>();
            this.InitializeComponent();
        }

        /// <summary>
        /// 参考图像文件列表。
        /// </summary>
        public IList<FileInfo> SrcImageFileList { get; }

        /// <summary>
        /// 对比图像文件列表。
        /// </summary>
        public IList<FileInfo> CmpImageFileList { get; }

        /// <summary>
        /// 打开一个窗口，并返回而不等待新打开的窗口关闭。
        /// 不支持此操作，请用 <see cref="Window.ShowDialog()"/> 代替。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new void Show() =>
            throw new NotSupportedException("Use ShowDialog() instead.");

        /// <summary>
        /// 确定按钮按下的事件处理。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// 取消按钮按下的事件处理。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// 参考文件数据表文件拖放进入。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SrcImageFileDataGrid_DragEnter(object sender, DragEventArgs e)
        {
            if (!(e.Data.GetData(DataFormats.FileDrop) is null))
            { e.Effects = DragDropEffects.Copy; }
        }

        /// <summary>
        /// 参考文件数据表文件拖放放下，添加文件至参考文件列表。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrImageFileDataGrid_Drop(object sender, DragEventArgs e)
        {
            // 取出文件路径和目录中的文件路径。
            var filePathList = new List<string>();
            foreach (string fileSystemPath in (e.Data.GetData(DataFormats.FileDrop) as string[]))
            {
                if (File.Exists(fileSystemPath))
                { filePathList.Add(Path.GetFullPath(fileSystemPath)); }
                else if (Directory.Exists(fileSystemPath))
                {
                    try { filePathList.AddRange(Directory.GetFiles(fileSystemPath)); }
                    catch (Exception) { }
                }
            }

            // 添加到参考文件列表。
            foreach (string filePath in filePathList)
            { this.SrcImageFileList.Add(new FileInfo(filePath)); }
        }

        /// <summary>
        /// 对比文件数据表文件拖放进入。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CmpImageFileDataGrid_DragEnter(object sender, DragEventArgs e)
        {
            if (!(e.Data.GetData(DataFormats.FileDrop) is null))
            { e.Effects = DragDropEffects.Copy; }
        }

        /// <summary>
        /// 对比文件数据表文件拖放放下，添加文件至对比文件列表。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CmpImageFileDataGrid_Drop(object sender, DragEventArgs e)
        {
            // 取出文件路径和目录中的文件路径。
            var filePathList = new List<string>();
            foreach (string fileSystemPath in (e.Data.GetData(DataFormats.FileDrop) as string[]))
            {
                if (File.Exists(fileSystemPath))
                { filePathList.Add(Path.GetFullPath(fileSystemPath)); }
                else if (Directory.Exists(fileSystemPath))
                {
                    try { filePathList.AddRange(Directory.GetFiles(fileSystemPath)); }
                    catch (Exception) { }
                }
            }

            // 添加到对比文件列表。
            foreach (string filePath in filePathList)
            { this.CmpImageFileList.Add(new FileInfo(filePath)); }
        }
    }
}
