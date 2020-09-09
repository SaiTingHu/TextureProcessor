using UnityEngine;

namespace HT.TextureProcessor
{
    /// <summary>
    /// Jpg格式纹理代理
    /// </summary>
    public sealed class JpgAgent : TextureAgent
    {
        /// <summary>
        /// Jpg格式纹理代理
        /// </summary>
        /// <param name="guid">纹理GUID</param>
        public JpgAgent(string guid) : base(guid)
        { }

        /// <summary>
        /// 获取纹理的格式
        /// </summary>
        /// <returns>格式</returns>
        public override TextureFormat GetFormat()
        {
            return TextureFormat.RGB24;
        }

        /// <summary>
        /// 获取纹理的编码字节数组
        /// </summary>
        /// <param name="texture">纹理</param>
        /// <returns>字节数组</returns>
        public override byte[] GetEncodeToBytes(Texture2D texture)
        {
            return texture.EncodeToJPG();
        }
    }
}