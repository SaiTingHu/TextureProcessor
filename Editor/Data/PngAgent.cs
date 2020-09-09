using UnityEngine;

namespace HT.TextureProcessor
{
    /// <summary>
    /// Png格式纹理代理
    /// </summary>
    public sealed class PngAgent : TextureAgent
    {
        /// <summary>
        /// Png格式纹理代理
        /// </summary>
        /// <param name="guid">纹理GUID</param>
        public PngAgent(string guid) : base(guid)
        { }

        /// <summary>
        /// 获取纹理的格式
        /// </summary>
        /// <returns>格式</returns>
        public override TextureFormat GetFormat()
        {
            return TextureFormat.RGBA32;
        }

        /// <summary>
        /// 获取纹理的编码字节数组
        /// </summary>
        /// <param name="texture">纹理</param>
        /// <returns>字节数组</returns>
        public override byte[] GetEncodeToBytes(Texture2D texture)
        {
            return texture.EncodeToPNG();
        }
    }
}