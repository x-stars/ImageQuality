using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace XstarS.ImageQuality.Evaluation
{
    /// <summary>
    /// 提供以比较方式评估 8 位 RGB 位图图像质量的方法。
    /// </summary>
    [CLSCompliant(false)]
    public abstract class Rgb8BitmapEvaluator : IBitmapEvaluator
    {
        /// <summary>
        /// 表示位图中单个像素的通道数量。
        /// </summary>
        protected readonly int Channels;

        /// <summary>
        /// 表示位图中单个图像通道的最大值。
        /// </summary>
        protected readonly int PeakValue;

        /// <summary>
        /// 表示加载位图图像所用的像素格式。
        /// </summary>
        protected readonly PixelFormat PxFormat;

        /// <summary>
        /// 初始化 <see cref="Rgb8BitmapEvaluator"/> 类的新实例。
        /// </summary>
        protected Rgb8BitmapEvaluator()
        {
            this.Channels = 3;
            this.PeakValue = byte.MaxValue;
            this.PxFormat = PixelFormat.Format24bppRgb;
        }

        /// <summary>
        /// 创建计算指定评估指标的 <see cref="Rgb8BitmapEvaluator"/> 类的实例。
        /// </summary>
        /// <param name="indicator">要使用的位图图像质量评估指标。</param>
        /// <returns>计算 <paramref name="indicator"/> 表示的评估指标的
        /// <see cref="Rgb8BitmapEvaluator"/> 类的实例。</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="indicator"/> 不为有效的枚举值。</exception>
        public static Rgb8BitmapEvaluator Create(EvaluationIndicator indicator) =>
            indicator switch
            {
                EvaluationIndicator.PSNR => new Rgb8BitmapPsnrEvaluator(),
                EvaluationIndicator.SSIM => new Rgb8BitmapSsimEvaluator(),
                _ => throw new ArgumentOutOfRangeException(nameof(indicator))
            };

        /// <summary>
        /// 以比较方式评估指定位图图像的质量，并返回图像质量指标。
        /// </summary>
        /// <param name="source">作为参考的位图图像。</param>
        /// <param name="target">要进行比较的位图图像。</param>
        /// <returns>与 <paramref name="source"/> 比较评估得到的
        /// <paramref name="target"/> 的图像质量指标。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/>
        /// 或 <paramref name="target"/> 为 <see langword="null"/>。</exception>
        public unsafe double Evaluate(Bitmap source, Bitmap target)
        {
            if (source is null) { throw new ArgumentNullException(nameof(source)); }
            if (target is null) { throw new ArgumentNullException(nameof(target)); }

            var size = new Size(
                Math.Min(source.Width, target.Width), Math.Min(source.Height, target.Height));
            source = (source.Size != size) ? new Bitmap(source, size) : source;
            target = (target.Size != size) ? new Bitmap(target, size) : target;

            var area = new Rectangle(Point.Empty, size);
            var dSource = source.LockBits(area, ImageLockMode.ReadOnly, this.PxFormat);
            var dTarget = target.LockBits(area, ImageLockMode.ReadOnly, this.PxFormat);
            var result = this.EvaluateCore((byte*)dSource.Scan0, (byte*)dTarget.Scan0, size);
            source.UnlockBits(dSource);
            target.UnlockBits(dTarget);
            return result;
        }

        /// <summary>
        /// 在派生类中重写，用于比较指定位图的字节数据，并返回图像质量指标。
        /// </summary>
        /// <param name="pSource">指向作为参考的位图数据的指针。</param>
        /// <param name="pTarget">指向要进行比较的位图数据的指针。</param>
        /// <param name="size">归一化后的位图图像的大小。</param>
        /// <returns>与 <paramref name="pSource"/> 指向的数据比较评估得到的
        /// <paramref name="pTarget"/> 指向的数据的图像质量指标。</returns>
        protected abstract unsafe double EvaluateCore(byte* pSource, byte* pTarget, Size size);
    }
}
