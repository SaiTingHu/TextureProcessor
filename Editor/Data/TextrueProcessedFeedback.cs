using UnityEngine;

namespace HT.TextureProcessor
{
    /// <summary>
    /// 纹理处理反馈信息
    /// </summary>
    public sealed class TextrueProcessedFeedback
    {
        /// <summary>
        /// 纹理名称
        /// </summary>
        public string Name;
        /// <summary>
        /// 纹理
        /// </summary>
        public Texture2D Value;
        /// <summary>
        /// 纹理原始存储内存大小
        /// </summary>
        public string RawStorageMemory;
        /// <summary>
        /// 纹理原始运行内存大小
        /// </summary>
        public string RawRuntimeMemory;
        /// <summary>
        /// 纹理修正存储内存大小
        /// </summary>
        public string StorageMemory;
        /// <summary>
        /// 纹理修正运行内存大小
        /// </summary>
        public string RuntimeMemory;
    }
}