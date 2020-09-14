using UnityEngine;

namespace HT.TextureProcessor
{
    /// <summary>
    /// Jpg格式纹理缩放代理
    /// </summary>
    internal sealed class JpgResizeAgent : TextureResizeAgent
    {
        /// <summary>
        /// Jpg格式纹理缩放代理
        /// </summary>
        /// <param name="guid">纹理GUID</param>
        /// <param name="path">纹理路径</param>
        public JpgResizeAgent(string guid, string path) : base(guid, path)
        { }

        /// <summary>
        /// 获取纹理的格式
        /// </summary>
        /// <returns>格式</returns>
        protected override TextureFormat GetFormat()
        {
            return TextureFormat.RGB24;
        }

        /// <summary>
        /// 获取纹理的编码字节数组
        /// </summary>
        /// <param name="texture">纹理</param>
        /// <returns>字节数组</returns>
        protected override byte[] GetEncodeToBytes(Texture2D texture)
        {
            return texture.EncodeToJPG();
        }

        /// <summary>
        /// 设置新纹理的像素
        /// </summary>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <param name="newTexture">新纹理</param>
        /// <param name="oldTexture">旧纹理</param>
        protected override void SetNewTexturePixels(int width, int height, Texture2D newTexture, Texture2D oldTexture)
        {
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    if (h < oldTexture.height && w < oldTexture.width)
                    {
                        newTexture.SetPixel(w, h, oldTexture.GetPixel(w, h));
                    }
                    else
                    {
                        bool greaterW = w >= oldTexture.width;
                        bool greaterH = h >= oldTexture.height;
                        if (greaterH && greaterW)
                        {
                            newTexture.SetPixel(w, h, oldTexture.GetPixel(oldTexture.width - 1, oldTexture.height - 1));
                        }
                        else if (greaterH && !greaterW)
                        {
                            newTexture.SetPixel(w, h, oldTexture.GetPixel(w, oldTexture.height - 1));
                        }
                        else if (!greaterH && greaterW)
                        {
                            newTexture.SetPixel(w, h, oldTexture.GetPixel(oldTexture.width - 1, h));
                        }
                        else
                        {
                            newTexture.SetPixel(w, h, Color.clear);
                        }
                    }
                }
            }
            newTexture.Apply();
        }
    }
}