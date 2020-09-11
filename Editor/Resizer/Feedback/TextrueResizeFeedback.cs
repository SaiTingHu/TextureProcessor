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
        public long RawStorageMemory;
        /// <summary>
        /// 纹理原始运行内存大小
        /// </summary>
        public long RawRuntimeMemory;
        /// <summary>
        /// 纹理缩放后存储内存大小
        /// </summary>
        public long StorageMemory;
        /// <summary>
        /// 纹理缩放后运行内存大小
        /// </summary>
        public long RuntimeMemory;
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
        
        /// <summary>
        /// 节约的存储内存大小
        /// </summary>
        public long SavedStorageMemory
        {
            get
            {
                return RawStorageMemory - StorageMemory;
            }
        }
        /// <summary>
        /// 节约的运行内存大小
        /// </summary>
        public long SavedRuntimeMemory
        {
            get
            {
                return RawRuntimeMemory - RuntimeMemory;
            }
        }
        /// <summary>
        /// 纹理的原始尺寸（格式：宽x高）
        /// </summary>
        public string RawSize
        {
            get
            {
                return RawWidth + "x" + RawHeight;
            }
        }
        /// <summary>
        /// 纹理的缩放后尺寸（格式：宽x高）
        /// </summary>
        public string Size
        {
            get
            {
                return Width + "x" + Height;
            }
        }

        public override string ToString()
        {
            string message = string.Format("{0}, Saved Storage Memory [{1}], Saved Runtime Memory [{2}], Raw Size [{3}], Resized Size [{4}]"
                , Name, Utility.FormatBytes(SavedStorageMemory), Utility.FormatBytes(SavedRuntimeMemory), RawSize, Size);
            return message;
        }
    }
}