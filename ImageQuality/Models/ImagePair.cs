using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace XstarS.ImageQuality.Models
{
    /// <summary>
    /// 表示由参考图像和对比图像构成的图像对，可用于图像质量评估。
    /// </summary>
    public class ImagePair : IDisposable
    {
        /// <summary>
        /// 单个像素的通道数量。
        /// </summary>
        private const int ChCount = 3;

        /// <summary>
        /// 单个图像通道的最大值。
        /// </summary>
        private const int ChMaxValue = 255;

        /// <summary>
        /// 加载图像所用的像素格式。
        /// </summary>
        private const PixelFormat PxFormat = PixelFormat.Format24bppRgb;

        /// <summary>
        /// 指示当前实例占用的资源是否已经被释放。
        /// </summary>
        private volatile bool IsDisposed = false;

        /// <summary>
        /// <see cref="ImagePair.SourceBitmap"/> 的缓存值。
        /// </summary>
        private readonly AutoReloadCache<Bitmap> SourceBitmapCache;

        /// <summary>
        /// <see cref="ImagePair.CompareBitmap"/> 的缓存值。
        /// </summary>
        private readonly AutoReloadCache<Bitmap> CompareBitmapCache;

        /// <summary>
        /// <see cref="ImagePair.Psnr"/> 的延迟初始化值。
        /// </summary>
        private readonly Lazy<double> LazyPsnr;

        /// <summary>
        /// <see cref="ImagePair.Ssim"/> 的延迟初始化值。
        /// </summary>
        private readonly Lazy<double> LazySsim;

        /// <summary>
        /// 使用参考图像和对比图像的路径初始化 <see cref="ImagePair"/> 类的新实例。
        /// </summary>
        /// <param name="sourcePath">参考图像文件的路径。</param>
        /// <param name="comparePath">对比图像文件的路径。</param>
        public ImagePair(string sourcePath, string comparePath)
        {
            this.SourceFile = this.TryLoadFile(sourcePath);
            this.CompareFile = this.TryLoadFile(comparePath);
            this.SourceBitmapCache = new AutoReloadCache<Bitmap>(
                () => this.TryLoadBitmap(this.SourceFile.FullName));
            this.CompareBitmapCache = new AutoReloadCache<Bitmap>(
                () => this.TryLoadBitmap(this.CompareFile.FullName));
            this.LazyPsnr = new Lazy<double>(this.ComputePsnr);
            this.LazySsim = new Lazy<double>(this.ComputeSsim);
        }

        /// <summary>
        /// 使用参考图像和对比图像的文件信息初始化 <see cref="ImagePair"/> 类的新实例。
        /// </summary>
        /// <param name="sourceFile">参考图像的文件信息。</param>
        /// <param name="compareFile">对比图像的文件信息。</param>
        public ImagePair(FileInfo sourceFile, FileInfo compareFile)
        {
            this.SourceFile = sourceFile;
            this.CompareFile = compareFile;
            this.SourceBitmapCache = new AutoReloadCache<Bitmap>(
                () => this.TryLoadBitmap(this.SourceFile.FullName));
            this.CompareBitmapCache = new AutoReloadCache<Bitmap>(
                () => this.TryLoadBitmap(this.CompareFile.FullName));
            this.LazyPsnr = new Lazy<double>(this.ComputePsnr);
            this.LazySsim = new Lazy<double>(this.ComputeSsim);
        }

        /// <summary>
        /// 获取当前图像对中参考图像的文件信息。
        /// </summary>
        public FileInfo SourceFile { get; }

        /// <summary>
        /// 获取当前图像对中对比图像的文件信息。
        /// </summary>
        public FileInfo CompareFile { get; }

        /// <summary>
        /// 获取当前图像对中参考图像的位图对象。
        /// </summary>
        public Bitmap SourceBitmap => this.SourceBitmapCache.Value;

        /// <summary>
        /// 获取当前图像对中对比图像的位图对象。
        /// </summary>
        public Bitmap CompareBitmap => this.CompareBitmapCache.Value;

        /// <summary>
        /// 获取当前图像对的 PSNR。
        /// </summary>
        public double Psnr => this.LazyPsnr.Value;

        /// <summary>
        /// 获取当前图像对的 SSIM。
        /// </summary>
        public double Ssim => this.LazySsim.Value;

        /// <summary>
        /// 尝试从指定文件路径加载文件信息。
        /// </summary>
        /// <param name="path">要加载的文件的路径。</param>
        /// <returns>加载完成的文件信息；若无法加载，则为当前目录。</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private FileInfo TryLoadFile(string path)
        {
            try { return new FileInfo(path); }
            catch (Exception) { return new FileInfo("."); }
        }

        /// <summary>
        /// 尝试从指定文件路径加载位图对象。
        /// </summary>
        /// <param name="path">要加载的文件的路径。</param>
        /// <returns>加载完成的位图；若无法加载，则为一个 1 * 1 的位图。</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private Bitmap TryLoadBitmap(string path)
        {
            try { return new Bitmap(path); }
            catch (Exception) { return new Bitmap(1, 1); }
        }

        /// <summary>
        /// 计算当前图像对的 PSNR。
        /// </summary>
        /// <returns> <see cref="ImagePair.SourceBitmap"/> 和
        /// <see cref="ImagePair.CompareBitmap"/> 之间的 PSNR。 </returns>
        private unsafe double ComputePsnr()
        {
            var chCount = ImagePair.ChCount;
            var chMax = ImagePair.ChMaxValue;
            var pxFormat = ImagePair.PxFormat;
            var source = this.SourceBitmap;
            var compare = this.CompareBitmap;

            var size = new Size(
                Math.Min(source.Width, compare.Width), Math.Min(source.Height, compare.Height));
            source = (source.Size != size) ? new Bitmap(source, size) : source;
            compare = (compare.Size != size) ? new Bitmap(compare, size) : compare;

            var sourceData = source.LockBits(
                new Rectangle(Point.Empty, size), ImageLockMode.ReadOnly, pxFormat);
            var compareData = compare.LockBits(
                new Rectangle(Point.Empty, size), ImageLockMode.ReadOnly, pxFormat);

            var sourceChPtr = (byte*)sourceData.Scan0.ToPointer();
            var compareChPtr = (byte*)compareData.Scan0.ToPointer();

            var sError = 0.0;
            for (int h = 0; h < size.Height; h++)
            {
                for (int w = 0; w < size.Width; w++)
                {
                    for (int ch = 0; ch < chCount; ch++)
                    {
                        var error = (double)(*(sourceChPtr++) - *(compareChPtr++));
                        sError += error * error;
                    }
                }
            }
            var mse = sError / (size.Width * size.Height * chCount);
            var psnr = 10 * Math.Log10((chMax * chMax) / mse);

            source.UnlockBits(sourceData);
            compare.UnlockBits(compareData);

            return psnr;
        }

        /// <summary>
        /// 计算当前图像对的 SSIM。
        /// </summary>
        /// <returns> <see cref="ImagePair.SourceBitmap"/> 和
        /// <see cref="ImagePair.CompareBitmap"/> 之间的 SSIM。 </returns>
        private unsafe double ComputeSsim()
        {
            var chCount = ImagePair.ChCount;
            var chMax = ImagePair.ChMaxValue;
            var pxFormat = ImagePair.PxFormat;
            var source = this.SourceBitmap;
            var compare = this.CompareBitmap;

            var size = new Size(
                Math.Min(source.Width, compare.Width), Math.Min(source.Height, compare.Height));
            source = (source.Size != size) ? new Bitmap(source, size) : source;
            compare = (compare.Size != size) ? new Bitmap(compare, size) : compare;

            var sourceData = source.LockBits(
                new Rectangle(Point.Empty, size), ImageLockMode.ReadOnly, pxFormat);
            var compareData = compare.LockBits(
                new Rectangle(Point.Empty, size), ImageLockMode.ReadOnly, pxFormat);

            var sourceChPtr = (byte*)sourceData.Scan0.ToPointer();
            var compareChPtr = (byte*)compareData.Scan0.ToPointer();

            var k1 = 0.01;
            var k2 = 0.03;
            var c1 = (k1 * chMax) * (k1 * chMax);
            var c2 = (k2 * chMax) * (k2 * chMax);
            var c3 = c2 / 2;

            var sSum = 0.0;
            var cSum = 0.0;
            for (int h = 0; h < size.Height; h++)
            {
                for (int w = 0; w < size.Width; w++)
                {
                    for (int ch = 0; ch < chCount; ch++)
                    {
                        sSum += *(sourceChPtr++);
                        cSum += *(compareChPtr++);
                    }
                }
            }
            var sMiu = sSum / (size.Width * size.Height * chCount);
            var cMiu = cSum / (size.Width * size.Height * chCount);

            sourceChPtr = (byte*)sourceData.Scan0.ToPointer();
            compareChPtr = (byte*)compareData.Scan0.ToPointer();

            var sVarSum = 0.0;
            var cVarSum = 0.0;
            var covSum = 0.0;
            for (int h = 0; h < size.Height; h++)
            {
                for (int w = 0; w < size.Width; w++)
                {
                    for (int ch = 0; ch < chCount; ch++)
                    {
                        var sError = *(sourceChPtr++) - sMiu;
                        var cError = *(compareChPtr++) - cMiu;
                        sVarSum += sError * sError;
                        cVarSum += cError * cError;
                        covSum += sError * cError;
                    }
                }
            }
            var sSigma = Math.Sqrt(sVarSum / (size.Width * size.Height * chCount - 1));
            var cSigma = Math.Sqrt(cVarSum / (size.Width * size.Height * chCount - 1));
            var scSigma = covSum / (size.Width * size.Height * chCount - 1);

            var l = (2 * sMiu * cMiu + c1) / (sMiu * sMiu + cMiu * cMiu + c1);
            var c = (2 * sSigma * cSigma + c2) / (sSigma * sSigma + cSigma * cSigma + c2);
            var s = (scSigma + c3) / (sSigma * cSigma + c3);
            var ssim = l * c * s;

            source.UnlockBits(sourceData);
            compare.UnlockBits(compareData);

            return ssim;
        }

        /// <summary>
        /// 释放当前实例占用的托管资源和非托管资源。
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// 释放当前实例占用的非托管资源，并根据指示释放托管资源。
        /// </summary>
        /// <param name="disposing">指示是否应该释放托管资源。</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    this.SourceBitmapCache.Dispose();
                    this.CompareBitmapCache.Dispose();
                }

                this.IsDisposed = true;
            }
        }
    }
}
