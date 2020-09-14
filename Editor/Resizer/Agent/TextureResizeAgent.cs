using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HT.TextureProcessor
{
    /// <summary>
    /// 纹理缩放代理
    /// </summary>
    internal abstract class TextureResizeAgent : IDisposable
    {
        /// <summary>
        /// ID
        /// </summary>
        public string GUID;
        /// <summary>
        /// 路径
        /// </summary>
        public string Path;
        /// <summary>
        /// 全路径
        /// </summary>
        public string FullPath;
        /// <summary>
        /// 纹理
        /// </summary>
        public Texture2D Value;
        /// <summary>
        /// 纹理导入器
        /// </summary>
        public TextureImporter Importer;
        /// <summary>
        /// 纹理导入设置
        /// </summary>
        public TextureImporterSettings Settings;
        /// <summary>
        /// 纹理导入平台设置
        /// </summary>
        public TextureImporterPlatformSettings PlatformSettings;
        
        /// <summary>
        /// 尺寸是否为4的倍数
        /// </summary>
        public bool IsMultipleOf4
        {
            get
            {
                return Value != null && (Value.width % 4) == 0 && (Value.height % 4) == 0;
            }
        }

        /// <summary>
        /// 纹理的尺寸（格式：宽x高）
        /// </summary>
        public string Size
        {
            get
            {
                if (Value == null)
                {
                    return "0x0";
                }
                else
                {
                    return Value.width + "x" + Value.height;
                }
            }
        }

        /// <summary>
        /// 纹理缩放代理
        /// </summary>
        /// <param name="guid">纹理GUID</param>
        /// <param name="path">纹理路径</param>
        public TextureResizeAgent(string guid, string path)
        {
            GUID = guid;
            Path = path;
            FullPath = Application.dataPath + Path.Substring(6);
        }
        
        /// <summary>
        /// 缩放纹理尺寸为4的倍数
        /// </summary>
        /// <returns>处理结果</returns>
        public virtual TextrueResizeFeedback ResizeToMultipleOf4()
        {
            //加载原纹理
            LoadValue();

            if (IsMultipleOf4)
                return null;

            //读取原纹理设置
            ReadSettings();

            //开启纹理的可读、可写模式
            Utility.SetReadableEnable(Importer);

            //反馈信息：记录原纹理信息
            TextrueResizeFeedback feedback = new TextrueResizeFeedback();
            feedback.Name = Path;
            feedback.RawStorageMemory = Utility.GetStorageMemorySize(Value);
            feedback.RawRuntimeMemory = Utility.GetRuntimeMemorySize(Value);
            feedback.RawWidth = Value.width;
            feedback.RawHeight = Value.height;

            //计算新纹理尺寸
            int widthDiff = Value.width % 4;
            int heightDiff = Value.height % 4;
            int width = widthDiff == 0 ? Value.width : (Value.width + (4 - widthDiff));
            int height = heightDiff == 0 ? Value.height : (Value.height + (4 - heightDiff));

            //构造新纹理
            Texture2D texture = new Texture2D(width, height, GetFormat(), false);
            SetNewTexturePixels(width, height, texture, Value);
            WriteToFile(texture);

            //应用新纹理设置
            ApplySettings();

            //加载新纹理
            LoadValue(true);

            //反馈信息：记录新纹理信息
            feedback.Value = Value;
            feedback.StorageMemory = Utility.GetStorageMemorySize(Value);
            feedback.RuntimeMemory = Utility.GetRuntimeMemorySize(Value);
            feedback.Width = Value.width;
            feedback.Height = Value.height;

            //关闭纹理的可读、可写模式
            Utility.SetReadableDisabled(Importer);

            return feedback;
        }

        /// <summary>
        /// 获取纹理的格式
        /// </summary>
        /// <returns>格式</returns>
        protected abstract TextureFormat GetFormat();

        /// <summary>
        /// 获取纹理的编码字节数组
        /// </summary>
        /// <param name="texture">纹理</param>
        /// <returns>字节数组</returns>
        protected abstract byte[] GetEncodeToBytes(Texture2D texture);

        /// <summary>
        /// 设置新纹理的像素
        /// </summary>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <param name="newTexture">新纹理</param>
        /// <param name="oldTexture">旧纹理</param>
        protected virtual void SetNewTexturePixels(int width, int height, Texture2D newTexture, Texture2D oldTexture)
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
                        newTexture.SetPixel(w, h, Color.clear);
                    }
                }
            }
            newTexture.Apply();
        }

        /// <summary>
        /// 读取原纹理设置
        /// </summary>
        private void ReadSettings()
        {
            Importer = AssetImporter.GetAtPath(Path) as TextureImporter;
            Settings = new TextureImporterSettings();
            Importer.ReadTextureSettings(Settings);
            PlatformSettings = Importer.GetDefaultPlatformTextureSettings();
        }

        /// <summary>
        /// 写入到文件
        /// </summary>
        /// <param name="texture">纹理</param>
        private void WriteToFile(Texture2D texture)
        {
            File.WriteAllBytes(FullPath, GetEncodeToBytes(texture));
            AssetDatabase.ImportAsset(Path);
        }

        /// <summary>
        /// 应用新纹理设置
        /// </summary>
        private void ApplySettings()
        {
            Importer = AssetImporter.GetAtPath(Path) as TextureImporter;
            Importer.SetTextureSettings(Settings);
            Importer.SetPlatformTextureSettings(PlatformSettings);
            Importer.crunchedCompression = true;
            if (Importer.textureType == TextureImporterType.Sprite)
            {
                Importer.spritePackingTag = null;
                Importer.mipmapEnabled = false;
            }
            Importer.SaveAndReimport();
        }

        /// <summary>
        /// 加载纹理
        /// </summary>
        /// <param name="isReload">当纹理已加载时，是否重新加载</param>
        public void LoadValue(bool isReload = false)
        {
            if (isReload)
            {
                Value = AssetDatabase.LoadAssetAtPath<Texture2D>(Path);
            }
            else
            {
                if (Value == null)
                {
                    Value = AssetDatabase.LoadAssetAtPath<Texture2D>(Path);
                }
            }
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public void Dispose()
        {
            Value = null;
            Importer = null;
            Settings = null;
            PlatformSettings = null;
        }
    }
}