using System;

namespace XstarS.ImageQuality.Helpers
{
    /// <summary>
    /// 提供枚举相关的帮助方法。
    /// </summary>
    internal static class EnumHelper
    {
        /// <summary>
        /// 检索指定枚举中常数值的数组。
        /// </summary>
        /// <typeparam name="TEnum">枚举类型。</typeparam>
        /// <returns>一个数组，其中包含 <typeparamref name="TEnum"/> 中常数的值。</returns>
        internal static TEnum[] GetValues<TEnum>() where TEnum : struct, Enum =>
            (TEnum[])Enum.GetValues(typeof(TEnum));
    }
}
