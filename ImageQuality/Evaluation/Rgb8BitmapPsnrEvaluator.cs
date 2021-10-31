using System;
using System.Drawing;

namespace XstarS.ImageQuality.Evaluation
{
    /// <summary>
    /// 提供评估 8 位 RGB 位图图像的峰值信噪比 (PSNR) 指标的方法。
    /// </summary>
    internal sealed class Rgb8BitmapPsnrEvaluator : Rgb8BitmapEvaluator
    {
        /// <summary>
        /// 初始化 <see cref="Rgb8BitmapPsnrEvaluator"/> 类的新实例。
        /// </summary>
        public Rgb8BitmapPsnrEvaluator() { }

        /// <summary>
        /// 比较指定位图的字节数据，并返回图像质量 PSNR 指标。
        /// </summary>
        /// <param name="pSource">指向作为参考的位图数据的指针。</param>
        /// <param name="pTarget">指向要进行比较的位图数据的指针。</param>
        /// <param name="size">归一化后的位图图像的大小。</param>
        /// <returns>与 <paramref name="pSource"/> 指向的数据比较评估得到的
        /// <paramref name="pTarget"/> 指向的数据的图像质量 PSNR 指标。</returns>
        protected override unsafe double EvaluateCore(byte* pSource, byte* pTarget, Size size)
        {
            var channels = this.Channels;
            var peakValue = this.PeakValue;

            var sError = 0.0;
            for (int h = 0; h < size.Height; h++)
            {
                for (int w = 0; w < size.Width; w++)
                {
                    for (int ch = 0; ch < channels; ch++)
                    {
                        var error = (double)(*pSource++ - *pTarget++);
                        sError += error * error;
                    }
                }
            }

            var mse = sError / (size.Width * size.Height * channels);
            var psnr = 10 * Math.Log10((peakValue * peakValue) / mse);
            return psnr;
        }
    }
}
