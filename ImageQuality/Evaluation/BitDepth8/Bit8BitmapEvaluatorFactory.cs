using System;
using System.Drawing.Imaging;

namespace XstarS.ImageQuality.Evaluation.BitDepth8
{
    /// <summary>
    /// 提供创建 <see cref="Bit8BitmapEvaluator"/> 类型实例的方法。
    /// </summary>
    internal class Bit8BitmapEvaluatorFactory : BitmapEvaluatorFactory
    {
        /// <summary>
        /// 表示加载位图图像所用的像素格式。
        /// </summary>
        private readonly PixelFormat PxFormat;

        /// <summary>
        /// 使用指定的位图像素格式初始化 <see cref="Bit8BitmapEvaluatorFactory"/> 类的新实例。
        /// </summary>
        /// <param name="format">位图像素模式。应为 8 位深度的像素格式。</param>
        public Bit8BitmapEvaluatorFactory(PixelFormat format)
        {
            this.PxFormat = format;
        }

        /// <summary>
        /// 创建计算指定评估指标的 <see cref="Bit8BitmapEvaluator"/> 类的实例。
        /// </summary>
        /// <param name="indicator">要使用的位图图像质量评估指标。</param>
        /// <returns>计算 <paramref name="indicator"/> 表示的评估指标的
        /// <see cref="Bit8BitmapEvaluator"/> 类的实例。</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="indicator"/> 不为有效的枚举值。</exception>
        public override IBitmapEvaluator CreateEvaluator(EvaluationIndicator indicator) => indicator switch
        {
            EvaluationIndicator.PSNR => new Bit8BitmapPsnrEvaluator(this.PxFormat),
            EvaluationIndicator.SSIM => new Bit8BitmapSsimEvaluator(this.PxFormat),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
