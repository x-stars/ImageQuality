using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using XstarS.ImageQuality.Evaluation;
using XstarS.ImageQuality.Helpers;
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
            this.ImagePairs = new ObservableCollection<PairedImageQuality>();
        }

        /// <summary>
        /// 获取当前包含的 <see cref="PairedImageQuality"/> 集合。
        /// </summary>
        public ObservableCollection<PairedImageQuality> ImagePairs { get; }

        /// <summary>
        /// 确定 <see cref="PairedImageQuality"/> 集合中是否包含任何项目。
        /// </summary>
        public bool HasImagePairs => this.ImagePairs.Count != 0;

        /// <summary>
        /// 向 <see cref="PairedImageQuality"/> 集合中添加指定的新项目。
        /// </summary>
        /// <param name="imagePair">要添加到集合的 <see cref="PairedImageQuality"/>。</param>
        public void AddImagePair(PairedImageQuality imagePair)
        {
            this.ImagePairs.Add(imagePair);
        }

        /// <summary>
        /// 向 <see cref="PairedImageQuality"/> 集合中添加多个指定的新项目。
        /// </summary>
        /// <param name="imagePairs">要添加到集合的多个 <see cref="PairedImageQuality"/>。</param>
        public void AddImagePairs(IEnumerable<PairedImageQuality> imagePairs)
        {
            foreach (var imagePair in imagePairs)
            {
                this.ImagePairs.Add(imagePair);
            }
        }

        /// <summary>
        /// 从 <see cref="PairedImageQuality"/> 集合中删除指定索引处的项目。
        /// </summary>
        /// <param name="index">要删除的项目的索引。</param>
        public void DeleteImagePair(int index)
        {
            this.ImagePairs.RemoveAt(index);
        }

        /// <summary>
        /// 从 <see cref="PairedImageQuality"/> 集合中删除指定的项目。
        /// </summary>
        /// <param name="imagePair">要删除的项目。</param>
        public void DeleteImagePair(PairedImageQuality imagePair)
        {
            this.ImagePairs.Remove(imagePair);
        }

        /// <summary>
        /// 清空 <see cref="PairedImageQuality"/> 的集合。
        /// </summary>
        public void ClearImagePairs()
        {
            this.ImagePairs.Clear();
        }

        /// <summary>
        /// 将对比图像集合的评估结果导出为 CSV 文本。
        /// </summary>
        /// <returns>导出的 CSV 文本。</returns>
        public string ExportResultToCsv()
        {
            const string separator = ",";
            var imagePairs = this.ImagePairs;
            var indicators = EnumHelper.GetValues<EvaluationIndicator>();

            var csv = new StringBuilder();
            var header = string.Join(separator,
                string.Join(separator, "Source", "Target"),
                string.Join(separator, indicators));
            csv.AppendLine(header);

            foreach (var imagePair in imagePairs)
            {
                var line = string.Join(separator,
                    string.Join(separator,
                        $@"""{imagePair.SourceFile.Name}""",
                        $@"""{imagePair.TargetFile.Name}"""),
                    string.Join(separator, Array.ConvertAll(
                        indicators, indicator => imagePair[indicator])));
                csv.AppendLine(line);
            }

            return csv.ToString();
        }
    }
}
