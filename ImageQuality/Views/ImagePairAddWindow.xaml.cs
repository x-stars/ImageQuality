using System.Windows;
using System.Windows.Input;
using XstarS.ImageQuality.Models;

namespace XstarS.ImageQuality.Views
{
    /// <summary>
    /// ImagePairAddWindow.xaml 的交互逻辑
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
            this.DataContext = new ImagePairAddWindowModel();
            this.InitializeComponent();
        }

        /// <summary>
        /// 获取当前窗口的数据模型。
        /// </summary>
        public ImagePairAddWindowModel Model => (ImagePairAddWindowModel)this.DataContext;

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
                    (sender, e) => ((ImagePairAddWindow)sender).CloseDialog(true)),
                new CommandBinding(ImagePairAddWindow.DialogCancel,
                    (sender, e) => ((ImagePairAddWindow)sender).CloseDialog(false)),
            };

            foreach (var commandBinding in commandBindings)
            {
                CommandManager.RegisterClassCommandBinding(typeof(ImagePairAddWindow), commandBinding);
            }
        }

        /// <summary>
        /// 关闭当前窗口，并设定对话框结果指定值。
        /// </summary>
        /// <param name="result">要设定的对话框结果。</param>
        public void CloseDialog(bool? result)
        {
            this.DialogResult = result;
            this.Close();
        }

        /// <summary>
        /// 获取当前窗口包含的图像文件对应的 <see cref="ImagePair"/> 的数组。
        /// </summary>
        /// <returns>当前窗口包含的图像文件对应的 <see cref="ImagePair"/> 的数组。</returns>
        public ImagePair[] GetImagePairs() => this.Model.ToImagePairs();

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
                this.Model.AddSourceFiles(paths);
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
                this.Model.AddCompareFiles(paths);
            }
        }
    }
}
