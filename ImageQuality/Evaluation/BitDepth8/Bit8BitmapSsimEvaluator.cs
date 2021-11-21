using System;
using System.Drawing;

namespace XstarS.ImageQuality.Evaluation.BitDepth8
{
    /// <summary>
    /// 提供评估 8 位 RGB 位图图像的结构相似度 (SSIM) 指标的方法。
    /// </summary>
    internal class Bit8BitmapSsimEvaluator : Bit8BitmapEvaluator
    {
        /// <summary>
        /// 表示 SSIM 的 <see langword="k1"/> 参数。
        /// </summary>
        private readonly double K1;

        /// <summary>
        /// 表示 SSIM 的 <see langword="k2"/> 参数。
        /// </summary>
        private readonly double K2;

        /// <summary>
        /// 初始化 <see cref="Bit8BitmapSsimEvaluator"/> 类的新实例。
        /// </summary>
        public Bit8BitmapSsimEvaluator() : this(0.01, 0.03) { }

        /// <summary>
        /// 以指定的参数初始化 <see cref="Bit8BitmapSsimEvaluator"/> 类的新实例。
        /// </summary>
        /// <param name="k1">SSIM 的 <see langword="k1"/> 参数。</param>
        /// <param name="k2">SSIM 的 <see langword="k2"/> 参数。</param>
        public Bit8BitmapSsimEvaluator(double k1, double k2)
        {
            this.K1 = k1;
            this.K2 = k2;
        }

        /// <summary>
        /// 比较指定位图的字节数据，并返回图像质量 SSIM 指标。
        /// </summary>
        /// <param name="pSource">指向作为参考的位图数据的指针。</param>
        /// <param name="pTarget">指向要进行比较的位图数据的指针。</param>
        /// <param name="size">归一化后的位图图像的大小。</param>
        /// <returns>与 <paramref name="pSource"/> 指向的数据比较评估得到的
        /// <paramref name="pTarget"/> 指向的数据的图像质量 SSIM 指标。</returns>
        protected sealed override unsafe double EvaluateCore(byte* pSource, byte* pTarget, Size size)
        {
            var channels = this.Channels;
            var peakValue = this.PeakValue;

            var k1 = this.K1;
            var k2 = this.K2;
            var c1 = (k1 * peakValue) * (k1 * peakValue);
            var c2 = (k2 * peakValue) * (k2 * peakValue);
            var c3 = c2 / 2;

            var sSum = 0.0;
            var tSum = 0.0;
            var p0Source = pSource;
            var p0Target = pTarget;
            for (int h = 0; h < size.Height; h++)
            {
                for (int w = 0; w < size.Width; w++)
                {
                    for (int ch = 0; ch < channels; ch++)
                    {
                        sSum += *p0Source++;
                        tSum += *p0Target++;
                    }
                }
            }
            var sMiu = sSum / (size.Width * size.Height * channels);
            var tMiu = tSum / (size.Width * size.Height * channels);

            var sVarSum = 0.0;
            var tVarSum = 0.0;
            var covSum = 0.0;
            for (int h = 0; h < size.Height; h++)
            {
                for (int w = 0; w < size.Width; w++)
                {
                    for (int ch = 0; ch < channels; ch++)
                    {
                        var sError = *pSource++ - sMiu;
                        var tError = *pTarget++ - tMiu;
                        sVarSum += sError * sError;
                        tVarSum += tError * tError;
                        covSum += sError * tError;
                    }
                }
            }
            var sSigma = Math.Sqrt(sVarSum / ((size.Width * size.Height * channels) - 1));
            var tSigma = Math.Sqrt(tVarSum / ((size.Width * size.Height * channels) - 1));
            var stSigma = covSum / ((size.Width * size.Height * channels) - 1);

            var l = ((2 * sMiu * tMiu) + c1) / ((sMiu * sMiu) + (tMiu * tMiu) + c1);
            var c = ((2 * sSigma * tSigma) + c2) / ((sSigma * sSigma) + (tSigma * tSigma) + c2);
            var s = (stSigma + c3) / ((sSigma * tSigma) + c3);
            var ssim = l * c * s;
            return ssim;
        }
    }
}
