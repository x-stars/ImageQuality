using System;
using System.Drawing;
using System.Linq;

namespace XstarS.ImageQuality.Evaluation.BitDepth8
{
    /// <summary>
    /// 提供评估 8 位 RGB 位图图像的峰值信噪比 (PSNR) 指标的方法。
    /// </summary>
    internal class Bit8BitmapPsnrEvaluator : Bit8BitmapEvaluator
    {
        /// <summary>
        /// 初始化 <see cref="Bit8BitmapPsnrEvaluator"/> 类的新实例。
        /// </summary>
        public Bit8BitmapPsnrEvaluator() { }

        /// <summary>
        /// 比较指定位图的字节数据，并返回图像质量 PSNR 指标。
        /// </summary>
        /// <param name="pSource">指向作为参考的位图数据的指针。</param>
        /// <param name="pTarget">指向要进行比较的位图数据的指针。</param>
        /// <param name="size">归一化后的位图图像的大小。</param>
        /// <returns>与 <paramref name="pSource"/> 指向的数据比较评估得到的
        /// <paramref name="pTarget"/> 指向的数据的图像质量 PSNR 指标。</returns>
        protected sealed override unsafe double EvaluateCore(byte* pSource, byte* pTarget, Size size)
        {
            var length = size.Width * size.Height * this.Channels;
            var partCount = (int)Math.Ceiling((double)length / this.PartLength);

            var sqError = ParallelEnumerable.Range(0, partCount).Select(
                partIndex =>
                {
                    var partOff = partIndex * this.PartLength;
                    var partLen = partIndex == partCount - 1 ?
                        (length - partOff) : this.PartLength;
                    var pPartSource = pSource + partOff;
                    var pPartTarget = pTarget + partOff;
                    return this.CalcSquareError(pPartSource, pPartTarget, partLen);
                }
            ).Sum();

            var peakValue = this.PeakValue;
            var mse = sqError / length;
            var psnr = 10 * Math.Log10((peakValue * peakValue) / mse);
            return psnr;
        }

        /// <summary>
        /// 计算当前位图分片的平方误差值。
        /// </summary>
        /// <param name="pSource">指向作为参考的位图数据的指针。</param>
        /// <param name="pTarget">指向要进行比较的位图数据的指针。</param>
        /// <param name="length">当前位图分片以字节为单位的大小。</param>
        /// <returns><paramref name="pSource"/> 指向的数据与
        /// <paramref name="pTarget"/> 指向的数据之间的平方误差值。</returns>
        protected virtual unsafe double CalcSquareError(byte* pSource, byte* pTarget, int length)
        {
            var sqError = 0.0;
            for (int offset = 0; offset < length; offset++)
            {
                var error = pSource[offset] - pTarget[offset];
                sqError += (double)error * error;
            }
            return sqError;
        }
    }
}
