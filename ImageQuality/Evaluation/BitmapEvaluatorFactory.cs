using System;
using System.Collections.Concurrent;
using System.Drawing.Imaging;
using XstarS.ImageQuality.Evaluation.BitDepth8;

namespace XstarS.ImageQuality.Evaluation
{
    /// <summary>
    /// 提供创建 <see cref="IBitmapEvaluator"/> 接口实例的方法。
    /// </summary>
    public abstract class BitmapEvaluatorFactory
    {
        /// <summary>
        /// 表示位图像素模式对应的 <see cref="BitmapEvaluatorFactory"/> 类的默认实例。
        /// </summary>
        private static readonly ConcurrentDictionary<PixelFormat, BitmapEvaluatorFactory> DefaultFactories =
            new ConcurrentDictionary<PixelFormat, BitmapEvaluatorFactory>();

        /// <summary>
        /// 初始化 <see cref="BitmapEvaluatorFactory"/> 类的新实例。
        /// </summary>
        protected BitmapEvaluatorFactory() { }

        /// <summary>
        /// 获取指定位图像素模式对应的 <see cref="BitmapEvaluatorFactory"/> 类的默认实例。
        /// </summary>
        /// <param name="format">指定图像中的每个像素的颜色数据格式。</param>
        /// <returns><paramref name="format"/> 对应的
        /// <see cref="BitmapEvaluatorFactory"/> 类的默认实例。</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="format"/> 不表示有效的枚举值。</exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="format"/> 表示的像素模式当前不受支持。</exception>
        public static BitmapEvaluatorFactory GetDefault(PixelFormat format) =>
            BitmapEvaluatorFactory.DefaultFactories.GetOrAdd(format,
                newFormat => BitmapEvaluatorFactory.CreateDefault(newFormat));

        /// <summary>
        /// 创建指定位图像素模式对应的 <see cref="BitmapEvaluatorFactory"/> 类的默认实例。
        /// </summary>
        /// <param name="format">指定图像中的每个像素的颜色数据格式。</param>
        /// <returns><paramref name="format"/> 对应的
        /// <see cref="BitmapEvaluatorFactory"/> 类的默认实例。</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="format"/> 不表示有效的枚举值。</exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="format"/> 表示的像素模式当前不受支持。</exception>
        private static BitmapEvaluatorFactory CreateDefault(PixelFormat format) => format switch
        {
            PixelFormat.Format8bppIndexed or PixelFormat.Format24bppRgb or
            PixelFormat.Format32bppRgb or PixelFormat.Format32bppArgb or
            PixelFormat.Format32bppPArgb =>
                new Bit8BitmapEvaluatorFactory(format),
            PixelFormat.Indexed or PixelFormat.Gdi or
            PixelFormat.Alpha or PixelFormat.PAlpha or
            PixelFormat.Extended or PixelFormat.Canonical or PixelFormat.Undefined or
            PixelFormat.Format1bppIndexed or PixelFormat.Format4bppIndexed or
            PixelFormat.Format16bppGrayScale or PixelFormat.Format16bppRgb555 or
            PixelFormat.Format16bppRgb565 or PixelFormat.Format16bppArgb1555 or
            PixelFormat.Format48bppRgb or PixelFormat.Format64bppArgb or
            PixelFormat.Format64bppPArgb or PixelFormat.Max =>
                throw new NotSupportedException(),
            _ => throw new ArgumentOutOfRangeException(nameof(format)),
        };

        /// <summary>
        /// 创建计算指定评估指标的 <see cref="IBitmapEvaluator"/> 接口的实例。
        /// </summary>
        /// <param name="indicator">要使用的位图图像质量评估指标。</param>
        /// <returns>计算 <paramref name="indicator"/> 表示的评估指标的
        /// <see cref="IBitmapEvaluator"/> 接口的实例。</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="indicator"/> 不为有效的枚举值。</exception>
        public abstract IBitmapEvaluator CreateEvaluator(EvaluationIndicator indicator);
    }
}
