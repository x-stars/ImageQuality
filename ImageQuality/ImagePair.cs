using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ImageQuality
{
    /// <summary>
    /// 图像对 <see cref="ImagePair"/>，用于评估图像的质量。
    /// </summary>
    public class ImagePair : IDisposable
    {
        /// <summary>
        /// 指示此实例是否已经被释放。
        /// </summary>
        private bool IsDisposed = false;
        /// <summary>
        /// 图像采用的统一像素格式。
        /// </summary>
        private readonly PixelFormat PixelFormat;
        /// <summary>
        /// <see cref="ImagePair.PixelFormat"/> 对应的通道数量。
        /// </summary>
        private readonly int Count;
        /// <summary>
        /// <see cref="ImagePair.PixelFormat"/> 对应的单通道峰值。
        /// </summary>
        private readonly double Peak;
        /// <summary>
        /// 用于对比的第一个图像文件。
        /// </summary>
        private readonly Lazy<FileInfo> LazyFile1;
        /// <summary>
        /// 用于对比的第二个图像文件。
        /// </summary>
        private readonly Lazy<FileInfo> LazyFile2;
        /// <summary>
        /// 用于对比的第一个图像。
        /// </summary>
        private readonly Lazy<Bitmap> LazyImage1;
        /// <summary>
        /// 用于对比的第二个图像。
        /// </summary>
        private readonly Lazy<Bitmap> LazyImage2;
        /// <summary>
        /// 当前图像对的 PSNR。
        /// </summary>
        private readonly Lazy<double> LazyPsnr;
        /// <summary>
        /// 当前图像对的 SSIM。
        /// </summary>
        private readonly Lazy<double> LazySsim;

        /// <summary>
        /// 使用图像文件名初始化 <see cref="ImagePair"/> 的新实例。
        /// </summary>
        /// <param name="path1">第一个图像的文件名。</param>
        /// <param name="path2">第二个图像的文件名。</param>
        public ImagePair(string path1, string path2)
        {
            this.PixelFormat = PixelFormat.Format32bppArgb;
            this.Count = 4;
            this.Peak = 255D;

            this.LazyFile1 = new Lazy<FileInfo>(() => this.TryLoadFile(path1));
            this.LazyFile2 = new Lazy<FileInfo>(() => this.TryLoadFile(path2));
            this.LazyImage1 = new Lazy<Bitmap>(
                () => this.TryLoadImage(this.File1.FullName));
            this.LazyImage2 = new Lazy<Bitmap>(
                () => this.TryLoadImage(this.File2.FullName));

            this.LazyPsnr = new Lazy<double>(this.CalcPsnr);
            this.LazySsim = new Lazy<double>(this.CalcSsim);
        }

        /// <summary>
        /// 当前实例的 <see cref="IDisposable"/> 检查对象。
        /// </summary>
        private ImagePair Disposable => this.IsDisposed ?
            throw new ObjectDisposedException(this.GetType().ToString()) : this;
        /// <summary>
        /// 用于对比的第一个图像文件。
        /// </summary>
        public FileInfo File1 => this.Disposable.LazyFile1.Value;
        /// <summary>
        /// 用于对比的第二个图像文件。
        /// </summary>
        public FileInfo File2 => this.Disposable.LazyFile2.Value;
        /// <summary>
        /// 用于对比的第一个图像。
        /// </summary>
        public Bitmap Image1 => this.Disposable.LazyImage1.Value;
        /// <summary>
        /// 用于对比的第二个图像。
        /// </summary>
        public Bitmap Image2 => this.Disposable.LazyImage2.Value;
        /// <summary>
        /// 当前图像对的 PSNR。
        /// </summary>
        public double Psnr => this.Disposable.LazyPsnr.Value;
        /// <summary>
        /// 当前图像对的 SSIM。
        /// </summary>
        public double Ssim => this.Disposable.LazySsim.Value;

        /// <summary>
        /// 释放此实例占用的资源。
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放此实例占用的非托管资源，并根据指示释放托管资源。
        /// </summary>
        /// <param name="disposing">指示是否释放托管资源。</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    this.Image1?.Dispose();
                    this.Image2?.Dispose();
                }

                this.IsDisposed = true;
            }
        }

        /// <summary>
        /// 尝试从文件路径加载文件信息。
        /// </summary>
        /// <param name="path">要加载的文件的路径。</param>
        /// <returns>加载完成的文件信息，若无法成功加载，则加载当前目录。</returns>
        private FileInfo TryLoadFile(string path)
        {
            try { return new FileInfo(path); }
            catch (Exception) { return new FileInfo("."); }
        }

        /// <summary>
        /// 尝试从文件路径加载位图对象。
        /// </summary>
        /// <param name="path">要加载的文件的路径。</param>
        /// <returns>加载完成的位图，若无法成功加载，则返回一个 1 * 1 的位图。</returns>
        private Bitmap TryLoadImage(string path)
        {
            try { return new Bitmap(path); }
            catch (Exception) { return new Bitmap(1, 1); }
        }

        /// <summary>
        /// 计算当前图像对的 PSNR。
        /// </summary>
        /// <returns>
        /// <see cref="ImagePair.Image1"/> 和 <see cref="ImagePair.Image2"/> 之间的 PSNR。
        /// </returns>
        private unsafe double CalcPsnr()
        {
            // 大小不一致则缩放到最小。
            var image1 = this.Image1;
            var image2 = this.Image2;
            var size = new Size(Math.Min(image1.Width, image2.Width), Math.Min(image1.Height, image2.Height));
            image1 = (image1.Size != size) ? new Bitmap(image1, size) : image1;
            image2 = (image2.Size != size) ? new Bitmap(image2, size) : image2;

            // 固定像素到内存。
            var image1Data = image1.LockBits(
                new Rectangle(Point.Empty, size), ImageLockMode.ReadOnly, this.PixelFormat);
            byte* image1ChPtr = (byte*)image1Data.Scan0.ToPointer();
            var image2Data = image2.LockBits(
                new Rectangle(Point.Empty, size), ImageLockMode.ReadOnly, this.PixelFormat);
            byte* image2ChPtr = (byte*)image2Data.Scan0.ToPointer();

            // 计算 PSNR。
            double se = 0;
            for (int h = 0; h < size.Height; h++)
            {
                for (int w = 0; w < size.Width; w++)
                {
                    for (int ch = 0; ch < this.Count; ch++)
                    {
                        double e = (double)*(image1ChPtr++) - *(image2ChPtr++);
                        se += e * e;
                    }
                }
            }
            double mse = se / (size.Width * size.Height * this.Count);
            double psnr = 10 * Math.Log10((this.Peak * this.Peak) / mse);

            // 从内存解锁像素。
            image1.UnlockBits(image1Data);
            image2.UnlockBits(image2Data);

            return psnr;
        }

        /// <summary>
        /// 计算当前图像对的 SSIM。
        /// </summary>
        /// <returns>
        /// <see cref="ImagePair.Image1"/> 和 <see cref="ImagePair.Image2"/> 之间的 SSIM。
        /// </returns>
        private unsafe double CalcSsim()
        {
            // 大小不一致则缩放到最小。
            var image1 = this.Image1;
            var image2 = this.Image2;
            var size = new Size(Math.Min(image1.Width, image2.Width), Math.Min(image1.Height, image2.Height));
            image1 = (image1.Size != size) ? new Bitmap(image1, size) : image1;
            image2 = (image2.Size != size) ? new Bitmap(image2, size) : image2;

            // 固定像素到内存。
            var image1Data = image1.LockBits(
                new Rectangle(Point.Empty, size), ImageLockMode.ReadOnly, this.PixelFormat);
            byte* image1ChPtr = (byte*)image1Data.Scan0.ToPointer();
            var image2Data = image2.LockBits(
                new Rectangle(Point.Empty, size), ImageLockMode.ReadOnly, this.PixelFormat);
            byte* image2ChPtr = (byte*)image2Data.Scan0.ToPointer();

            // 设定参数。
            double k1 = 0.01;
            double k2 = 0.03;
            double c1 = (k1 * this.Peak) * (k1 * this.Peak);
            double c2 = (k2 * this.Peak) * (k2 * this.Peak);
            double c3 = c2 / 2;

            // 计算平均值。
            double valSum1 = 0;
            double valSum2 = 0;
            for (int h = 0; h < size.Height; h++)
            {
                for (int w = 0; w < size.Width; w++)
                {
                    for (int ch = 0; ch < this.Count; ch++)
                    {
                        valSum1 += *(image1ChPtr++);
                        valSum2 += *(image2ChPtr++);
                    }
                }
            }
            double miu1 = valSum1 / (size.Width * size.Height * this.Count);
            double miu2 = valSum2 / (size.Width * size.Height * this.Count);

            // 重设指针。
            image1ChPtr = (byte*)image1Data.Scan0.ToPointer();
            image2ChPtr = (byte*)image2Data.Scan0.ToPointer();

            // 计算方差和协方差。
            double varSum1 = 0;
            double varSum2 = 0;
            double covSum = 0;
            for (int h = 0; h < size.Height; h++)
            {
                for (int w = 0; w < size.Width; w++)
                {
                    for (int ch = 0; ch < this.Count; ch++)
                    {
                        double e1 = *(image1ChPtr++) - miu1;
                        double e2 = *(image2ChPtr++) - miu2;
                        varSum1 += e1 * e1;
                        varSum2 += e2 * e2;
                        covSum += e1 * e2;
                    }
                }
            }
            double sigma1 = Math.Sqrt(varSum1 / (size.Width * size.Height * this.Count - 1));
            double sigma2 = Math.Sqrt(varSum2 / (size.Width * size.Height * this.Count - 1));
            double sigma12 = covSum / (size.Width * size.Height * this.Count - 1);

            // 计算 SSIM。
            double l = (2 * miu1 * miu2 + c1) / (miu1 * miu1 + miu2 * miu2 + c1);
            double c = (2 * sigma1 * sigma2 + c2) / (sigma1 * sigma1 + sigma2 * sigma2 + c2);
            double s = (sigma12 + c3) / (sigma1 * sigma2 + c3);
            double ssim = l * c * s;

            // 从内存解锁像素。
            image1.UnlockBits(image1Data);
            image2.UnlockBits(image2Data);

            return ssim;
        }
    }
}
