using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using XstarS.ImageQuality.Models;

namespace XstarS.ImageQuality.Views
{
    /// <summary>
    /// 表示 <see cref="MainWindow"/> 的数据模型。
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
        /// 向 <see cref="ImagePair"/> 集合中添加指定的新项目。
        /// </summary>
        /// <param name="imagePair">要添加到集合的 <see cref="ImagePair"/>。</param>
        public void AddImagePair(ImagePair imagePair)
        {
            this.ImagePairs.Add(imagePair);
        }

        /// <summary>
        /// 向 <see cref="ImagePair"/> 集合中添加多个指定的新项目。
        /// </summary>
        /// <param name="imagePairs">要添加到集合的多个 <see cref="ImagePair"/>。</param>
        public void AddImagePairs(IEnumerable<ImagePair> imagePairs)
        {
            foreach (var imagePair in imagePairs)
            {
                this.ImagePairs.Add(imagePair);
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
            var imagePairs = this.ImagePairs;
            var csv = new StringBuilder();
            var header = "Source,Traget,PSNR,SSIM";
            csv.AppendLine(header);
            foreach (var imagePair in imagePairs)
            {
                var line = string.Join(",", new[]
                {
                    $@"""{imagePair.SourceFile.Name}""",
                    $@"""{imagePair.TargetFile.Name}""",
                    imagePair.Psnr.ToString(),
                    imagePair.Ssim.ToString(),
                });
                csv.AppendLine(line);
            }
            return csv.ToString();
        }
    }
}
