using UnityEngine;

namespace HT.TextureProcessor
{
    /// <summary>
    /// Png格式纹理缩放代理
    /// </summary>
    public sealed class PngResizeAgent : TextureResizeAgent
    {
        /// <summary>
        /// Png格式纹理缩放代理
        /// </summary>
        /// <param name="guid">纹理GUID</param>
        /// <param name="path">纹理路径</param>
        public PngResizeAgent(string guid, string path) : base(guid, path)
        { }

        /// <summary>
        /// 获取纹理的格式
        /// </summary>
        /// <returns>格式</returns>
        protected override TextureFormat GetFormat()
        {
            return TextureFormat.RGBA32;
        }

        /// <summary>
        /// 获取纹理的编码字节数组
        /// </summary>
        /// <param name="texture">纹理</param>
        /// <returns>字节数组</returns>
        protected override byte[] GetEncodeToBytes(Texture2D texture)
        {
            return texture.EncodeToPNG();
        }
    }
}