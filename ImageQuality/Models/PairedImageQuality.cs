using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using XstarS.ImageQuality.Evaluation;
using XstarS.ImageQuality.Evaluation.BitDepth8;
using XstarS.ImageQuality.Helpers;

namespace XstarS.ImageQuality.Models
{
    /// <summary>
    /// 提供由参考图像和对比图像对比得到的图像质量评估结果。
    /// </summary>
    public class PairedImageQuality
    {
        /// <summary>
        /// 表示延迟加载的图像质量评估指标的 <see cref="Dictionary{TKey, TValue}"/> 对象。
        /// </summary>
        private readonly Dictionary<EvaluationIndicator, Lazy<double>> LazyImageQuality;

        /// <summary>
        /// 使用参考图像和对比图像的路径初始化 <see cref="PairedImageQuality"/> 类的新实例。
        /// </summary>
        /// <param name="sourcePath">参考图像文件的路径。</param>
        /// <param name="targetPath">对比图像文件的路径。</param>
        public PairedImageQuality(string sourcePath, string targetPath)
            : this(new FileInfo(sourcePath), new FileInfo(targetPath))
        {
        }

        /// <summary>
        /// 使用参考图像和对比图像的文件信息初始化 <see cref="PairedImageQuality"/> 类的新实例。
        /// </summary>
        /// <param name="sourceFile">参考图像的文件信息。</param>
        /// <param name="targetFile">对比图像的文件信息。</param>
        public PairedImageQuality(FileInfo sourceFile, FileInfo targetFile)
        {
            this.SourceFile = sourceFile;
            this.TargetFile = targetFile;
            this.LazyImageQuality = this.CreateLazyImageQuality();
        }

        /// <summary>
        /// 获取以指定评估指标对比当前图像得到的图像质量。
        /// </summary>
        /// <param name="indicator">图像质量的评估指标。</param>
        /// <returns>以 <paramref name="indicator"/> 指标对比当前图像得到的图像质量。</returns>
        public double this[EvaluationIndicator indicator] =>
            this.LazyImageQuality[indicator].Value;

        /// <summary>
        /// 获取当前参考图像的文件信息。
        /// </summary>
        public FileInfo SourceFile { get; }

        /// <summary>
        /// 获取当前对比图像的文件信息。
        /// </summary>
        public FileInfo TargetFile { get; }

        /// <summary>
        /// 尝试从指定文件路径加载位图对象。
        /// </summary>
        /// <param name="path">要加载的文件的路径。</param>
        /// <returns>加载完成的位图；若无法加载，则为一个 1 * 1 的位图。</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static Bitmap TryLoadBitmap(string path)
        {
            try { return new Bitmap(path); }
            catch (Exception) { return new Bitmap(1, 1); }
        }

        /// <summary>
        /// 创建延迟加载的图像质量评估指标的 <see cref="Dictionary{TKey, TValue}"/> 对象。
        /// </summary>
        /// <returns>延迟加载的图像质量评估指标的 <see cref="Dictionary{TKey, TValue}"/> 对象。</returns>
        private Dictionary<EvaluationIndicator, Lazy<double>> CreateLazyImageQuality()
        {
            var indicators = EnumHelper.GetValues<EvaluationIndicator>();
            return indicators.ToDictionary(indicator => indicator,
                indicator => new Lazy<double>(() => this.CalculateImageQuality(indicator)));
        }

        /// <summary>
        /// 以指定评估指标比较当前图像并计算图像质量。
        /// </summary>
        /// <param name="indicator">图像质量的评估指标。</param>
        /// <returns>以 <paramref name="indicator"/> 指标对比当前图像得到的图像质量。</returns>
        private double CalculateImageQuality(EvaluationIndicator indicator)
        {
            using var sourceBitmap = PairedImageQuality.TryLoadBitmap(this.SourceFile.FullName);
            using var targetBitmap = PairedImageQuality.TryLoadBitmap(this.TargetFile.FullName);
            return Bit8BitmapEvaluator.Create(indicator).Evaluate(sourceBitmap, targetBitmap);
        }
    }
}
