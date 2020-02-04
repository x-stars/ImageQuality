using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XstarS.ImageQuality.Helpers
{
    /// <summary>
    /// 提供文件系统路径相关的帮助方法。
    /// </summary>
    internal static class PathHelper
    {
        /// <summary>
        /// 获取指定路径包含的所有文件的完整路径。
        /// </summary>
        /// <param name="path">要获取文件的文件或目录路径。</param>
        /// <returns><paramref name="path"/> 包含的所有文件的完整路径。</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal static string[] GetFilePaths(string path)
        {
            var filePaths = new List<string>();
            if (File.Exists(path))
            {
                filePaths.Add(Path.GetFullPath(path));
            }
            else if (Directory.Exists(path))
            {
                try { filePaths.AddRange(Directory.GetFiles(path)); }
                catch (Exception) { }
            }
            return filePaths.ToArray();
        }

        /// <summary>
        /// 获取指定的多个路径包含的所有文件的完整路径。
        /// </summary>
        /// <param name="paths">要获取文件的多个文件或目录路径。</param>
        /// <returns><paramref name="paths"/> 包含的所有文件的完整路径。</returns>
        internal static string[] GetFilePaths(string[] paths)
        {
            return paths.SelectMany(PathHelper.GetFilePaths).ToArray();
        }
    }
}
