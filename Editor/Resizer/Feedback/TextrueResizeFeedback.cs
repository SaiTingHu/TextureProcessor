using UnityEngine;

namespace HT.TextureProcessor
{
    /// <summary>
    /// 纹理缩放处理反馈信息
    /// </summary>
    public sealed class TextrueResizeFeedback
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
        /// 纹理缩放后存储内存大小
        /// </summary>
        public string StorageMemory;
        /// <summary>
        /// 纹理缩放后运行内存大小
        /// </summary>
        public string RuntimeMemory;
        /// <summary>
        /// 纹理原始宽度
        /// </summary>
        public int RawWidth;
        /// <summary>
        /// 纹理原始高度
        /// </summary>
        public int RawHeight;
        /// <summary>
        /// 纹理缩放后宽度
        /// </summary>
        public int Width;
        /// <summary>
        /// 纹理缩放后高度
        /// </summary>
        public int Height;

        public override string ToString()
        {
            string message = string.Format("{0}，原始尺寸、存储、运行内存：<color=red>{1}、{2}、{3}</color>，缩放后尺寸、存储、运行内存：<color=green>{4}、{5}、{6}</color>"
                    , Name, RawWidth + "x" + RawHeight, RawStorageMemory, RawRuntimeMemory, Width + "x" + Height, StorageMemory, RuntimeMemory);
            return message;
        }
    }
}