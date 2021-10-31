using System;
using System.Collections.ObjectModel;
using System.IO;
using XstarS.ImageQuality.Helpers;
using XstarS.ImageQuality.Models;

namespace XstarS.ImageQuality.Views
{
    /// <summary>
    /// 表示 <see cref="ImagePairAddWindow"/> 的数据模型。
    /// </summary>
    public class ImagePairAddWindowModel
    {
        /// <summary>
        /// 初始化 <see cref="ImagePairAddWindowModel"/> 类的新实例。
        /// </summary>
        public ImagePairAddWindowModel()
        {
            this.SourceFiles = new ObservableCollection<FileInfo>();
            this.TargetFiles = new ObservableCollection<FileInfo>();
        }

        /// <summary>
        /// 获取当前窗口包含的参考图像文件的集合。
        /// </summary>
        public ObservableCollection<FileInfo> SourceFiles { get; }

        /// <summary>
        /// 获取当前窗口包含的对比图像文件的集合。
        /// </summary>
        public ObservableCollection<FileInfo> TargetFiles { get; }

        /// <summary>
        /// 向参考图像文件的集合中添加指定路径包含的文件。
        /// </summary>
        /// <param name="paths">要添加的文件或目录的路径。</param>
        public void AddSourceFiles(string[] paths)
        {
            var filePaths = PathHelper.GetFilePaths(paths);
            foreach (var filePath in filePaths)
            {
                this.SourceFiles.Add(new FileInfo(filePath));
            }
        }

        /// <summary>
        /// 向对比图像文件的集合中添加指定路径包含的文件。
        /// </summary>
        /// <param name="paths">要添加的文件或目录的路径。</param>
        public void AddTaregtFiles(string[] paths)
        {
            var filePaths = PathHelper.GetFilePaths(paths);
            foreach (var filePath in filePaths)
            {
                this.TargetFiles.Add(new FileInfo(filePath));
            }
        }

        /// <summary>
        /// 将当前实例包含的图像文件的集合转换为 <see cref="ImagePair"/> 的数组。
        /// </summary>
        /// <returns>转换得到的 <see cref="ImagePair"/> 的数组</returns>
        public ImagePair[] ToImagePairs()
        {
            var sourceFiles = this.SourceFiles;
            var targetFiles = this.TargetFiles;
            var length = Math.Min(sourceFiles.Count, targetFiles.Count);
            var imagePairs = new ImagePair[length];
            for (int i = 0; i < length; i++)
            {
                imagePairs[i] = new ImagePair(sourceFiles[i], targetFiles[i]);
            }
            return imagePairs;
        }
    }
}
