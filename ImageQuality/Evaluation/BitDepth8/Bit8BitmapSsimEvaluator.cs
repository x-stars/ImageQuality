using System;
using System.Drawing;
using System.Linq;

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
            var length = size.Width * size.Height * this.Channels;
            var partCount = (int)Math.Ceiling((double)length / this.PartLength);

            var (sSum, tSum) = ParallelEnumerable.Range(0, partCount).Select(
                partIndex =>
                {
                    var partOff = partIndex * this.PartLength;
                    var partLen = partIndex == partCount - 1 ?
                        (length - partOff) : this.PartLength;
                    var pPartSource = pSource + partOff;
                    var pPartTarget = pTarget + partOff;
                    return this.SumAllValues(pPartSource, pPartTarget, partLen);
                }
            ).Aggregate((sum1, sum2) => (sum1.SSum + sum2.SSum, sum1.TSum + sum2.TSum));
            var (sMiu, tMiu) = (sSum / length, tSum / length);

            var (sVar, tVar, covar) = ParallelEnumerable.Range(0, partCount).Select(
                partIndex =>
                {
                    var partOff = partIndex * this.PartLength;
                    var partLen = partIndex == partCount - 1 ?
                        (length - partOff) : this.PartLength;
                    var pPartSource = pSource + partOff;
                    var pPartTarget = pTarget + partOff;
                    return this.CalcVariance(pPartSource, sMiu, pPartTarget, tMiu, partLen);
                }
            ).Aggregate((var1, var2) =>
                (var1.SVar + var2.SVar, var1.TVar + var2.TVar, var1.Covar + var2.Covar));
            var (sSigma, tSigma) = (Math.Sqrt(sVar / (length - 1)), Math.Sqrt(tVar / (length - 1)));
            var stSigma = covar / (length - 1);

            var peakValue = this.PeakValue;
            var k1 = this.K1;
            var k2 = this.K2;
            var c1 = (k1 * peakValue) * (k1 * peakValue);
            var c2 = (k2 * peakValue) * (k2 * peakValue);
            var c3 = c2 / 2;

            var l = ((2 * sMiu * tMiu) + c1) / ((sMiu * sMiu) + (tMiu * tMiu) + c1);
            var c = ((2 * sSigma * tSigma) + c2) / ((sSigma * sSigma) + (tSigma * tSigma) + c2);
            var s = (stSigma + c3) / ((sSigma * tSigma) + c3);
            var ssim = l * c * s;
            return ssim;
        }

        /// <summary>
        /// 计算当前位图分片的所有值之和。
        /// </summary>
        /// <param name="pSource">指向作为参考的位图数据的指针。</param>
        /// <param name="pTarget">指向要进行比较的位图数据的指针。</param>
        /// <param name="length">当前位图分片以字节为单位的大小。</param>
        /// <returns><paramref name="pSource"/> 指向的数据之和以及
        /// <paramref name="pTarget"/> 指向的数据之和组成的二元组。</returns>
        protected virtual unsafe (double SSum, double TSum) SumAllValues(
            byte* pSource, byte* pTarget, int length)
        {
            var (sSum, tSum) = (0.0, 0.0);
            for (int offset = 0; offset < length; offset++)
            {
                sSum += pSource[offset];
                tSum += pTarget[offset];
            }
            return (sSum, tSum);
        }

        /// <summary>
        /// 计算当前位图分片的方差和协方差。
        /// </summary>
        /// <param name="pSource">指向作为参考的位图数据的指针。</param>
        /// <param name="sMiu">作为参考的位图数据的平均值。</param>
        /// <param name="pTarget">指向要进行比较的位图数据的指针。</param>
        /// <param name="tMiu">要进行比较的位图数据的平均值。</param>
        /// <param name="length">当前位图分片以字节为单位的大小。</param>
        /// <returns><paramref name="pSource"/> 指向的数据的方差、<paramref name="pTarget"/> 指向的数据的方差以及
        /// <paramref name="pSource"/> 和 <paramref name="pTarget"/> 指向的数据的协方差组成的三元组。</returns>
        protected virtual unsafe (double SVar, double TVar, double Covar) CalcVariance(
            byte* pSource, double sMiu, byte* pTarget, double tMiu, int length)
        {
            var (sVar, tVar, covar) = (0.0, 0.0, 0.0);
            for (int offset = 0; offset < length; offset++)
            {
                var sDev = pSource[offset] - sMiu;
                var tDev = pTarget[offset] - tMiu;
                sVar += sDev * sDev;
                tVar += tDev * tDev;
                covar += sDev * tDev;
            }
            return (sVar, tVar, covar);
        }
    }
}
