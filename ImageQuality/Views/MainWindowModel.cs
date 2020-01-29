using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using XstarS.ImageQuality.Models;

namespace XstarS.ImageQuality.Views
{
    /// <summary>
    /// 表示主窗口的数据逻辑的模型。
    /// </summary>
    public class MainWindowModel
    {
        /// <summary>
        /// 初始化 <see cref="MainWindowModel"/> 类的新实例。
        /// </summary>
        public MainWindowModel()
        {
            this.ImagePairs = new ObservableCollection<ImagePair>();
        }

        /// <summary>
        /// 获取当前包含的 <see cref="ImagePair"/> 集合。
        /// </summary>
        public ObservableCollection<ImagePair> ImagePairs { get; }

        /// <summary>
        /// 确定 <see cref="ImagePair"/> 集合中是否包含任何项目。
        /// </summary>
        public bool HasImagePairs => this.ImagePairs.Count != 0;

        /// <summary>
        /// 向 <see cref="ImagePair"/> 集合中添加多个指定文件路径定义的新项目。
        /// </summary>
        /// <param name="sourcePaths">参考图像文件的路径列表。</param>
        /// <param name="comparePaths">对比图像文件的路径列表。</param>
        public void AddImagePairs(IList<string> sourcePaths, IList<string> comparePaths)
        {
            var length = Math.Min(sourcePaths.Count, comparePaths.Count);
            for (int i = 0; i < length; i++)
            {
                this.ImagePairs.Add(new ImagePair(sourcePaths[i], comparePaths[i]));
            }
        }

        /// <summary>
        /// 向 <see cref="ImagePair"/> 集合中添加多个指定文件信息定义的新项目。
        /// </summary>
        /// <param name="sourceFiles">参考图像的文件信息列表。</param>
        /// <param name="compareFiles">对比图像的文件信息列表。</param>
        public void AddImagePairs(IList<FileInfo> sourceFiles, IList<FileInfo> compareFiles)
        {
            var length = Math.Min(sourceFiles.Count, compareFiles.Count);
            for (int i = 0; i < length; i++)
            {
                this.ImagePairs.Add(new ImagePair(sourceFiles[i], compareFiles[i]));
            }
        }

        /// <summary>
        /// 从 <see cref="ImagePair"/> 集合中删除指定索引处的项目。
        /// </summary>
        /// <param name="index">要删除的项目的索引。</param>
        public void DeleteImagePair(int index)
        {
            var imagePair = this.ImagePairs[index];
            this.ImagePairs.RemoveAt(index);
            imagePair.Dispose();
        }

        /// <summary>
        /// 从 <see cref="ImagePair"/> 集合中删除指定的项目。
        /// </summary>
        /// <param name="imagePair">要删除的项目。</param>
        public void DeleteImagePair(ImagePair imagePair)
        {
            this.ImagePairs.Remove(imagePair);
            imagePair.Dispose();
        }

        /// <summary>
        /// 清空 <see cref="ImagePair"/> 的集合。
        /// </summary>
        public void ClearImagePairs()
        {
            var imagePairs = new List<ImagePair>(this.ImagePairs);
            this.ImagePairs.Clear();
            foreach (var imagePair in imagePairs)
            {
                imagePair.Dispose();
            }
        }

        /// <summary>
        /// 将对比图像集合的评估结果导出为 CSV 文本。
        /// </summary>
        /// <returns>导出的 CSV 文本。</returns>
        public string ExportResultToCsv()
        {
            var csv = new StringBuilder();
            var header = "No,Source,Comapre,PSNR,SSIM";
            csv.AppendLine(header);
            for (int i = 0; i < this.ImagePairs.Count; i++)
            {
                var line = string.Join(",", new[]
                {
                    (i + 1).ToString(),
                    $@"""{this.ImagePairs[i].SourceFile.Name}""",
                    $@"""{this.ImagePairs[i].CompareFile.Name}""",
                    this.ImagePairs[i].Psnr.ToString(),
                    this.ImagePairs[i].Ssim.ToString(),
                });
                csv.AppendLine(line);
            }
            return csv.ToString();
        }
    }
}
