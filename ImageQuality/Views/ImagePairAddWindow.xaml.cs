using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace XstarS.ImageQuality.Views
{
    /// <summary>
    /// ImagePairAddWindow.xaml 的交互逻辑。
    /// </summary>
    public partial class ImagePairAddWindow : Window
    {
        /// <summary>
        /// 初始化 <see cref="ImagePairAddWindow"/> 类。
        /// </summary>
        static ImagePairAddWindow()
        {
            ImagePairAddWindow.InitializeCommandBindings();
        }

        /// <summary>
        /// 初始化 <see cref="ImagePairAddWindow"/> 类的新实例。
        /// </summary>
        public ImagePairAddWindow()
        {
            this.DataContext = this;
            this.InitializeComponent();
            this.SourceFiles = new ObservableCollection<FileInfo>();
            this.CompareFiles = new ObservableCollection<FileInfo>();
        }

        /// <summary>
        /// 获取当前窗口包含的参考图像文件的集合。
        /// </summary>
        public ObservableCollection<FileInfo> SourceFiles { get; }

        /// <summary>
        /// 获取当前窗口包含的对比图像文件的集合。
        /// </summary>
        public ObservableCollection<FileInfo> CompareFiles { get; }

        /// <summary>
        /// 获取表示“对话框确定”的命令的值。
        /// 默认键笔势：<see cref="Key.Enter"/>。
        /// </summary>
        public static RoutedUICommand DialogOK { get; } =
            new RoutedUICommand(
                nameof(ImagePairAddWindow.DialogOK),
                nameof(ImagePairAddWindow.DialogOK),
                typeof(ImagePairAddWindow),
                new InputGestureCollection() {
                    new KeyGesture(Key.Enter, ModifierKeys.None, "Enter") });

        /// <summary>
        /// 获取表示“取消”的命令的值。
        /// 默认键笔势：<see cref="Key.Escape"/>。
        /// </summary>
        public static RoutedUICommand DialogCancel { get; } =
            new RoutedUICommand(
                nameof(ImagePairAddWindow.DialogCancel),
                nameof(ImagePairAddWindow.DialogCancel),
                typeof(ImagePairAddWindow),
                new InputGestureCollection() {
                    new KeyGesture(Key.Escape, ModifierKeys.None, "Esc") });

        /// <summary>
        /// 初始化 <see cref="ImagePairAddWindow"/> 的命令绑定。
        /// </summary>
        private static void InitializeCommandBindings()
        {
            var commandBindings = new[]
            {
                new CommandBinding(ImagePairAddWindow.DialogOK,
                    (sender, e) => ((ImagePairAddWindow)sender).CloseWithDialogTrue()),
                new CommandBinding(ImagePairAddWindow.DialogCancel,
                    (sender, e) => ((ImagePairAddWindow)sender).CloseWithDialogFalse()),
            };

            foreach (var commandBinding in commandBindings)
            {
                CommandManager.RegisterClassCommandBinding(typeof(ImagePairAddWindow), commandBinding);
            }
        }

        /// <summary>
        /// 关闭当前窗口，并设定对话框结果为 <see langword="true"/>。
        /// </summary>
        public void CloseWithDialogTrue()
        {
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// 关闭当前窗口，并设定对话框结果为 <see langword="false"/>。
        /// </summary>
        public void CloseWithDialogFalse()
        {
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// 参考图像文件数据表拖放进入的事件处理。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">提供事件数据的对象。</param>
        private void SourceFileDataGrid_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[])
            {
                e.Effects = DragDropEffects.Copy;
            }
        }

        /// <summary>
        /// 参考图像文件数据表拖放放下的事件处理。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">提供事件数据的对象。</param>
        private void SourceFileDataGrid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] paths)
            {
                this.AddFilesFromPaths(this.SourceFiles, paths);
            }
        }

        /// <summary>
        /// 对比图像文件数据表拖放进入的事件处理。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">提供事件数据的对象。</param>
        private void CompareFileDataGrid_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[])
            {
                e.Effects = DragDropEffects.Copy;
            }
        }

        /// <summary>
        /// 对比图像文件数据表拖放放下的事件处理。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">提供事件数据的对象。</param>
        private void CompareFileDataGrid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] paths)
            {
                this.AddFilesFromPaths(this.CompareFiles, paths);
            }
        }

        /// <summary>
        /// 根据指定文件或目录的路径，向指定的文件信息列表添加新项目。
        /// </summary>
        /// <param name="files">要添加新项目的文件信息列表。</param>
        /// <param name="paths">要添加的文件或目录的路径。</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void AddFilesFromPaths(IList<FileInfo> files, string[] paths)
        {
            var filePaths = new List<string>();
            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    filePaths.Add(Path.GetFullPath(path));
                }
                else if (Directory.Exists(path))
                {
                    try { filePaths.AddRange(Directory.GetFiles(path)); }
                    catch (Exception) { }
                }
            }

            foreach (var filePath in filePaths)
            {
                files.Add(new FileInfo(filePath));
            }
        }
    }
}
