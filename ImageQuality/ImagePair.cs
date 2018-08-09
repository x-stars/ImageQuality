using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace ImageQuality
{
    /// <summary>
    /// 图像对 <see cref="ImagePair"/>，用于评估图像的质量。
    /// </summary>
    public class ImagePair : IEquatable<ImagePair>
    {
        /// <summary>
        /// 图像采用的统一像素格式。
        /// </summary>
        private readonly PixelFormat imagePixelFormat;
        /// <summary>
        /// <see cref="ImagePair.imagePixelFormat"/> 对应的通道数量。
        /// </summary>
        private readonly int channel;
        /// <summary>
        /// <see cref="ImagePair.imagePixelFormat"/> 对应的单通道峰值。
        /// </summary>
        private readonly double peak;
        /// <summary>
        /// 用于对比的第一个图像。
        /// </summary>
        private Bitmap image1;
        /// <summary>
        /// 用于对比的第二个图像。
        /// </summary>
        private Bitmap image2;
        /// <summary>
        /// 当前图像对的 PSNR。
        /// </summary>
        private double? psnr;
        /// <summary>
        /// 当前图像对的 SSIM。
        /// </summary>
        private double? ssim;

        /// <summary>
        /// 使用图像文件名初始化 <see cref="ImagePair"/> 的新实例。
        /// </summary>
        /// <param name="filename1">第一个图像的文件名。</param>
        /// <param name="filename2">第二个图像的文件名。</param>
        public ImagePair(string filename1, string filename2)
        {
            this.imagePixelFormat = PixelFormat.Format32bppArgb;
            this.channel = 4;
            this.peak = 255D;
            
            try { this.ImageFile1 = new FileInfo(filename1); }
            catch (Exception) { this.ImageFile1 = new FileInfo(@"\"); }
            try { this.ImageFile2 = new FileInfo(filename2); }
            catch (Exception) { this.ImageFile2 = new FileInfo(@"\"); }
            this.image1 = null;
            this.image2 = null;

            this.psnr = null;
            this.ssim = null;
        }

        /// <summary>
        /// 用于对比的第一个图像文件。
        /// </summary>
        public FileInfo ImageFile1 { get; }
        /// <summary>
        /// 用于对比的第二个图像文件。
        /// </summary>
        public FileInfo ImageFile2 { get; }
        /// <summary>
        /// 用于对比的第一个图像。
        /// </summary>
        public Bitmap Image1
        {
            get
            {
                if (this.image1 is null)
                {
                    try { this.image1 = new Bitmap(this.ImageFile1.FullName); }
                    catch (Exception) { this.image1 = new Bitmap(1, 1); }
                }
                return this.image1;
            }
        }
        /// <summary>
        /// 用于对比的第二个图像。
        /// </summary>
        public Bitmap Image2
        {
            get
            {
                if (this.image2 is null)
                {
                    try { this.image2 = new Bitmap(this.ImageFile2.FullName); }
                    catch (Exception) { this.image2 = new Bitmap(1, 1); }
                }
                return this.image2;
            }
        }
        /// <summary>
        /// 当前图像对的 PSNR。
        /// </summary>
        public double Psnr => this.psnr ?? this.CalcPsnr();
        /// <summary>
        /// 当前图像对的 SSIM。
        /// </summary>
        public double Ssim => this.ssim ?? this.CalcSsim();

        /// <summary>
        /// 返回一个值，该值指示此实例和指定的 <see cref="ImagePair"/> 对象是否相等。
        /// </summary>
        /// <param name="other">要与此实例比较的 <see cref="ImagePair"/> 对象。</param>
        /// <returns>
        /// 如果此实例和 <paramref name="other"/> 中包含的图像对的文件路径相同，
        /// 则为 <see langword="true"/>；否则为 <see langword="false"/>。
        /// </returns>
        public bool Equals(ImagePair other) =>
            !(other is null) &&
            EqualityComparer<string>.Default.Equals(this.ImageFile1.FullName, other.ImageFile1.FullName) &&
            EqualityComparer<string>.Default.Equals(this.ImageFile2.FullName, other.ImageFile2.FullName);

        /// <summary>
        /// 返回一个值，该值指示此实例和指定的对象是否相等。
        /// </summary>
        /// <param name="obj">要与此实例比较的对象。</param>
        /// <returns>
        /// 如果 <paramref name="obj"/> 是 <see cref="ImagePair"/> 的实例，且包含的图像对的文件路径相同，
        /// 则为 <see langword="true"/>；否则为 <see langword="false"/>。
        /// </returns>
        public override bool Equals(object obj) =>
            this.Equals(obj as ImagePair);

        /// <summary>
        /// 返回此实例的哈希代码。
        /// </summary>
        /// <returns>32 位有符号整数哈希代码。</returns>
        public override int GetHashCode()
        {
            var hashCode = 1182647313;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.ImageFile1.FullName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.ImageFile2.FullName);
            return hashCode;
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
                new Rectangle(Point.Empty, size), ImageLockMode.ReadOnly, this.imagePixelFormat);
            byte* image1ChPtr = (byte*)image1Data.Scan0.ToPointer();
            var image2Data = image2.LockBits(
                new Rectangle(Point.Empty, size), ImageLockMode.ReadOnly, this.imagePixelFormat);
            byte* image2ChPtr = (byte*)image2Data.Scan0.ToPointer();

            // 计算 PSNR。
            double se = 0;
            for (int h = 0; h < size.Height; h++)
            {
                for (int w = 0; w < size.Width; w++)
                {
                    for (int ch = 0; ch < this.channel; ch++)
                    {
                        double e = (double)*(image1ChPtr++) - *(image2ChPtr++);
                        se += e * e;
                    }
                }
            }
            double mse = se / (size.Width * size.Height * this.channel);
            double psnr = 10 * Math.Log10((this.peak * this.peak) / mse);

            // 从内存解锁像素。
            image1.UnlockBits(image1Data);
            image2.UnlockBits(image2Data);

            // 返回结果。
            this.psnr = psnr;
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
                new Rectangle(Point.Empty, size), ImageLockMode.ReadOnly, this.imagePixelFormat);
            byte* image1ChPtr = (byte*)image1Data.Scan0.ToPointer();
            var image2Data = image2.LockBits(
                new Rectangle(Point.Empty, size), ImageLockMode.ReadOnly, this.imagePixelFormat);
            byte* image2ChPtr = (byte*)image2Data.Scan0.ToPointer();

            // 设定参数。
            double k1 = 0.01;
            double k2 = 0.03;
            double c1 = (k1 * this.peak) * (k1 * this.peak);
            double c2 = (k2 * this.peak) * (k2 * this.peak);
            double c3 = c2 / 2;

            // 计算平均值。
            double valSum1 = 0;
            double valSum2 = 0;
            for (int h = 0; h < size.Height; h++)
            {
                for (int w = 0; w < size.Width; w++)
                {
                    for (int ch = 0; ch < this.channel; ch++)
                    {
                        valSum1 += *(image1ChPtr++);
                        valSum2 += *(image2ChPtr++);
                    }
                }
            }
            double miu1 = valSum1 / (size.Width * size.Height * this.channel);
            double miu2 = valSum2 / (size.Width * size.Height * this.channel);

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
                    for (int ch = 0; ch < this.channel; ch++)
                    {
                        double e1 = *(image1ChPtr++) - miu1;
                        double e2 = *(image2ChPtr++) - miu2;
                        varSum1 += e1 * e1;
                        varSum2 += e2 * e2;
                        covSum += e1 * e2;
                    }
                }
            }
            double sigma1 = Math.Sqrt(varSum1 / (size.Width * size.Height * this.channel - 1));
            double sigma2 = Math.Sqrt(varSum2 / (size.Width * size.Height * this.channel - 1));
            double sigma12 = covSum / (size.Width * size.Height * this.channel - 1);

            // 计算 SSIM。
            double l = (2 * miu1 * miu2 + c1) / (miu1 * miu1 + miu2 * miu2 + c1);
            double c = (2 * sigma1 * sigma2 + c2) / (sigma1 * sigma1 + sigma2 * sigma2 + c2);
            double s = (sigma12 + c3) / (sigma1 * sigma2 + c3);
            double ssim = l * c * s;

            // 从内存解锁像素。
            image1.UnlockBits(image1Data);
            image2.UnlockBits(image2Data);

            // 返回结果。
            this.ssim = ssim;
            return ssim;
        }

        /// <summary>
        /// 指示两 <see cref="ImagePair"/> 对象是否相等。
        /// </summary>
        /// <param name="pair1">第一个对象。</param>
        /// <param name="pair2">第二个对象。</param>
        /// <returns>
        /// 如果 <paramref name="pair1"/> 和 <paramref name="pair2"/> 中包含的图像对的文件路径相同，
        /// 则为 <see langword="true"/>；否则为 <see langword="false"/>。
        /// </returns>
        public static bool operator ==(ImagePair pair1, ImagePair pair2) =>
            EqualityComparer<ImagePair>.Default.Equals(pair1, pair2);
        /// <summary>
        /// 指示两 <see cref="ImagePair"/> 对象是否不等。
        /// </summary>
        /// <param name="pair1">第一个对象。</param>
        /// <param name="pair2">第二个对象。</param>
        /// <returns>
        /// 如果 <paramref name="pair1"/> 和 <paramref name="pair2"/> 中包含的图像对的文件路径不同，
        /// 则为 <see langword="true"/>；否则为 <see langword="false"/>。
        /// </returns>
        public static bool operator !=(ImagePair pair1, ImagePair pair2) =>
            !(pair1 == pair2);
    }
}
