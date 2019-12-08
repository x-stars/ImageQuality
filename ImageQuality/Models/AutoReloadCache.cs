using System;

namespace XstarS.ImageQuality.Models
{
    /// <summary>
    /// 表示一个可自动重载和回收的对象缓存。
    /// </summary>
    /// <typeparam name="T">要缓存的对象的类型，应为引用类型。</typeparam>
    public sealed class AutoReloadCache<T> : IDisposable where T : class
    {
        /// <summary>
        /// 指示当前实例占用的资源是否已经被释放。
        /// </summary>
        private volatile bool IsDisposed = false;

        /// <summary>
        /// 重载对象的方法的委托。
        /// </summary>
        private readonly Func<T> ReloadDelegate;

        /// <summary>
        /// 对象缓存的弱引用。
        /// </summary>
        private readonly WeakReference<T> WeakValueCache;

        /// <summary>
        /// 以创建对象的方法的委托初始化 <see cref="AutoReloadCache{T}"/> 类的新实例。
        /// </summary>
        /// <param name="reloadDelegate">重载对象的方法的委托。</param>
        public AutoReloadCache(Func<T> reloadDelegate)
        {
            this.ReloadDelegate = reloadDelegate ??
                throw new ArgumentNullException(nameof(reloadDelegate));
            this.WeakValueCache = new WeakReference<T>(null);
        }

        /// <summary>
        /// 确定对象的缓存是否存在。
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// 当前实例占用的资源已经被释放。</exception>
        public bool IsCached => this.TryGetCache(out _);

        /// <summary>
        /// 获取对象缓存的值；若缓存失效，将自动重载。
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// 当前实例占用的资源已经被释放。</exception>
        public T Value => this.GetCacheOrReload();

        /// <summary>
        /// 获取对象的缓存；若不存在，将自动重载。
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// 当前实例占用的资源已经被释放。</exception>
        public T GetCacheOrReload()
        {
            this.CheckDisposed();

            if (!this.WeakValueCache.TryGetTarget(out var target))
            {
                lock (this.WeakValueCache)
                {
                    if (!this.WeakValueCache.TryGetTarget(out _))
                    {
                        target = this.ReloadDelegate.Invoke();
                        this.WeakValueCache.SetTarget(target);
                    }
                }
            }
            return target;
        }

        /// <summary>
        /// 尝试获取对象的缓存。
        /// </summary>
        /// <param name="cache">对象的缓存；
        /// 若不存在，则为 <see langword="null"/>。</param>
        /// <returns>若对象的缓存存在，则为 <see langword="true"/>；
        /// 否则为 <see langword="false"/>。</returns>
        /// <exception cref="ObjectDisposedException">
        /// 当前实例占用的资源已经被释放。</exception>
        public bool TryGetCache(out T cache)
        {
            this.CheckDisposed();

            return this.WeakValueCache.TryGetTarget(out cache);
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    if (this.TryGetCache(out var cache))
                    {
                        if (cache is IDisposable disposable)
                        {
                            try { disposable.Dispose(); }
                            catch (Exception) { }
                        }
                    }
                }

                this.IsDisposed = true;
            }
        }

        /// <summary>
        /// 检查当前实例占用的资源是否已经被释放。
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// 当前实例占用的资源已经被释放。</exception>
        private void CheckDisposed()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().ToString());
            }
        }
    }
}
