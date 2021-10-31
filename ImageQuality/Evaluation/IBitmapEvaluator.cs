using System;
using System.Drawing;

namespace XstarS.ImageQuality.Evaluation
{
    /// <summary>
    /// 提供以比较方式评估位图图像质量的方法。
    /// </summary>
    public interface IBitmapEvaluator
    {
        /// <summary>
        /// 以比较方式评估指定位图图像的质量，并返回图像质量指标。
        /// </summary>
        /// <param name="source">作为参考的位图图像。</param>
        /// <param name="target">要进行比较的位图图像。</param>
        /// <returns>与 <paramref name="source"/> 比较评估得到的
        /// <paramref name="target"/> 的图像质量指标。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/>
        /// 或 <paramref name="target"/> 为 <see langword="null"/>。</exception>
        double Evaluate(Bitmap source, Bitmap target);
    }
}
