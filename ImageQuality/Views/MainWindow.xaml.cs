using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;

namespace XstarS.ImageQuality.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 初始化 <see cref="MainWindow"/> 类。
        /// </summary>
        static MainWindow()
        {
            MainWindow.InitializeCommandBindings();
        }

        /// <summary>
        /// 初始化 <see cref="MainWindow"/> 类的新实例。
        /// </summary>
        public MainWindow()
        {
            this.DataContext = new MainWindowModel();
            this.InitializeComponent();
            this.Model.ImagePairs.CollectionChanged += this.ImagePairs_CollectionChanged;
        }

        /// <summary>
        /// 获取当前窗口的数据模型。
        /// </summary>
        public MainWindowModel Model => (MainWindowModel)this.DataContext;

        /// <summary>
        /// 初始化 <see cref="MainWindow"/> 的命令绑定。
        /// </summary>
        private static void InitializeCommandBindings()
        {
            var commandBindings = new[]
            {
                new CommandBinding(MainWindow.AddImagePairsCommand,
                    (sender, e) => ((MainWindow)sender).AddImagePairs()),
                new CommandBinding(MainWindow.ClearImagePairsCommand,
                    (sender, e) => ((MainWindow)sender).Model.ClearImagePairs(),
                    (sender, e) => e.CanExecute = ((MainWindow)sender).Model.HasImagePairs),
                new CommandBinding(MainWindow.DeleteImagePairCommand,
                    (sender, e) => ((MainWindow)sender).Model.DeleteImagePair((int)e.Parameter),
                    (sender, e) => e.CanExecute = ((MainWindow)sender).ImagePairDataGrid.SelectedIndex >= 0),
                new CommandBinding(MainWindow.CopyImagePairsResultCommand,
                    (sender, e) => Clipboard.SetText(((MainWindow)sender).Model.ExportResultToCsv()),
                    (sender, e) => e.CanExecute = ((MainWindow)sender).Model.HasImagePairs),
            };

            foreach (var commandBinding in commandBindings)
            {
                CommandManager.RegisterClassCommandBinding(typeof(MainWindow), commandBinding);
            }
        }

        /// <summary>
        /// 获取表示“添加图像对”的命令的值。
        /// 默认键笔势：未定义。
        /// </summary>
        public static RoutedUICommand AddImagePairsCommand { get; } =
            new RoutedUICommand(
                nameof(MainWindow.AddImagePairsCommand),
                nameof(MainWindow.AddImagePairsCommand),
                typeof(MainWindow));

        /// <summary>
        /// 获取表示“清除图像对”的命令的值。
        /// 默认键笔势：未定义。
        /// </summary>
        public static RoutedUICommand ClearImagePairsCommand { get; } =
            new RoutedUICommand(
                nameof(MainWindow.ClearImagePairsCommand),
                nameof(MainWindow.ClearImagePairsCommand),
                typeof(MainWindow));

        /// <summary>
        /// 获取表示“删除图像对”的命令的值。
        /// 默认键笔势：<see cref="Key.Delete"/>。
        /// </summary>
        public static RoutedUICommand DeleteImagePairCommand { get; } =
            new RoutedUICommand(
                nameof(MainWindow.DeleteImagePairCommand),
                nameof(MainWindow.DeleteImagePairCommand),
                typeof(MainWindow),
                new InputGestureCollection() {
                    new KeyGesture(Key.Delete, ModifierKeys.None, "Delete") });

        /// <summary>
        /// 获取表示“复制图像对结果”的命令的值。
        /// 默认键笔势：未定义。
        /// </summary>
        public static RoutedUICommand CopyImagePairsResultCommand { get; } =
            new RoutedUICommand(
                nameof(MainWindow.CopyImagePairsResultCommand),
                nameof(MainWindow.CopyImagePairsResultCommand),
                typeof(MainWindow));

        /// <summary>
        /// 向当前窗口添加图像对。
        /// </summary>
        private void AddImagePairs()
        {
            var addWindow = new ImagePairAddWindow() { Owner = this };
            if (addWindow.ShowDialog() == true)
            {
                this.Model.AddImagePairs(addWindow.GetImagePairs());
            }
        }

        /// <summary>
        /// <see cref="MainWindowModel.ImagePairs"/> 的集合发生更改的事件处理。 
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">提供事件数据的对象。</param>
        private void ImagePairs_CollectionChanged(
            object sender, NotifyCollectionChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() => CommandManager.InvalidateRequerySuggested());
        }
    }
}
