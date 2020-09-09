using System.IO;
using UnityEditor;
using UnityEngine;

namespace HT.TextureProcessor
{
    /// <summary>
    /// 纹理代理
    /// </summary>
    public sealed class TextureAgent
    {
        /// <summary>
        /// ID
        /// </summary>
        public string GUID;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name;
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
                return (Value.width % 4) == 0 && (Value.height % 4) == 0;
            }
        }

        /// <summary>
        /// 纹理代理
        /// </summary>
        /// <param name="guid">纹理GUID</param>
        public TextureAgent(string guid)
        {
            GUID = guid;
            Path = AssetDatabase.GUIDToAssetPath(guid);
            FullPath = Application.dataPath + Path.Substring(6);
            Value = AssetDatabase.LoadAssetAtPath<Texture2D>(Path);
            Name = Value.name;
            Settings = new TextureImporterSettings();
            TextureImporter importer = AssetImporter.GetAtPath(Path) as TextureImporter;
            importer.ReadTextureSettings(Settings);
            PlatformSettings = importer.GetDefaultPlatformTextureSettings();
        }

        /// <summary>
        /// 修正纹理尺寸为4的倍数
        /// </summary>
        /// <returns>处理结果</returns>
        public TextrueProcessedFeedback ResizeToMultipleOf4()
        {
            if (IsMultipleOf4)
                return null;
            
            Utility.SetReadableEnable(this);

            TextrueProcessedFeedback feedback = new TextrueProcessedFeedback();
            feedback.Name = Path;
            feedback.RawStorageMemory = Utility.GetStorageMemorySize(Value);
            feedback.RawRuntimeMemory = Utility.GetRuntimeMemorySize(Value);

            int widthDiff = Value.width % 4;
            int heightDiff = Value.height % 4;
            int width = widthDiff == 0 ? Value.width : (Value.width + (4 - widthDiff));
            int height = heightDiff == 0 ? Value.height : (Value.height + (4 - heightDiff));
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            SetPixels(width, height, texture, Value);
            WriteToFile(texture);
            ApplySettings();

            Value = AssetDatabase.LoadAssetAtPath<Texture2D>(Path);
            feedback.Value = Value;
            feedback.StorageMemory = Utility.GetStorageMemorySize(Value);
            feedback.RuntimeMemory = Utility.GetRuntimeMemorySize(Value);

            Utility.SetReadableDisabled(this);
            
            return feedback;
        }

        private void SetPixels(int width, int height, Texture2D newTexture, Texture2D oldTexture)
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (i < oldTexture.height && j < oldTexture.width)
                    {
                        newTexture.SetPixel(j, i, oldTexture.GetPixel(j, i));
                    }
                    else
                    {
                        newTexture.SetPixel(j, i, Color.clear);
                    }
                }
            }
            newTexture.Apply();
        }

        private void WriteToFile(Texture2D texture)
        {
            File.WriteAllBytes(FullPath, texture.EncodeToPNG());
            AssetDatabase.Refresh();
        }

        private void ApplySettings()
        {
            TextureImporter importer = AssetImporter.GetAtPath(Path) as TextureImporter;
            importer.SetTextureSettings(Settings);
            importer.SetPlatformTextureSettings(PlatformSettings);
            if (importer.textureType == TextureImporterType.Sprite)
            {
                importer.spritePackingTag = null;
            }
            importer.SaveAndReimport();
        }
    }
}