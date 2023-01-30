using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace HT.TextureProcessor
{
    /// <summary>
    /// 纹理绘画器
    /// </summary>
    public sealed class TexturePainter : IDisposable
    {
        /// <summary>
        /// 纹理
        /// </summary>
        internal Texture2D Value;
        /// <summary>
        /// 纹理路径
        /// </summary>
        internal string Path;
        /// <summary>
        /// 纹理全路径
        /// </summary>
        internal string FullPath;
        /// <summary>
        /// 纹理的文件格式
        /// </summary>
        internal FileFormat Format;
        /// <summary>
        /// 绘画锚点
        /// </summary>
        internal Vector2 Anchor;
        /// <summary>
        /// 纹理导入器
        /// </summary>
        internal TextureImporter Importer;
        /// <summary>
        /// 纹理导入设置
        /// </summary>
        internal TextureImporterSettings Settings;
        /// <summary>
        /// 纹理导入平台设置
        /// </summary>
        internal TextureImporterPlatformSettings PlatformSettings;
        /// <summary>
        /// 绘画纹理
        /// </summary>
        internal Texture2D PaintValue;

        private Color[] _rawPixels;
        private Color[] _targetPixels;

        /// <summary>
        /// 是否是一个空的绘画器
        /// </summary>
        public bool IsNull
        {
            get
            {
                return Value == null || PaintValue == null;
            }
        }

        /// <summary>
        /// 尺寸是否为4的倍数
        /// </summary>
        public bool IsMultipleOf4
        {
            get
            {
                return PaintValue != null && (PaintValue.width % 4) == 0 && (PaintValue.height % 4) == 0;
            }
        }

        /// <summary>
        /// 生成一个空的纹理绘画器
        /// </summary>
        public TexturePainter()
        {
            
        }

        /// <summary>
        /// 生成一个纹理绘画器
        /// </summary>
        /// <param name="texture">纹理</param>
        /// <param name="anchor">锚点</param>
        public TexturePainter(Texture2D texture, Vector2 anchor)
        {
            OpenTexture(texture, anchor);
        }

        /// <summary>
        /// 打开新的纹理
        /// </summary>
        /// <param name="texture">纹理</param>
        /// <param name="anchor">锚点</param>
        public void OpenTexture(Texture2D texture, Vector2 anchor)
        {
            Value = texture;
            Path = AssetDatabase.GetAssetPath(Value);
            FullPath = Application.dataPath + Path.Substring(6);
            Format = Path.GetFileFormat();
            Anchor = anchor;
            Importer = AssetImporter.GetAtPath(Path) as TextureImporter;
            Settings = new TextureImporterSettings();
            Importer.ReadTextureSettings(Settings);
            PlatformSettings = Importer.GetDefaultPlatformTextureSettings();

            //开启纹理的可读、可写模式
            Utility.SetReadableEnable(Importer);

            if (PaintValue != null)
            {
                UObject.DestroyImmediate(PaintValue);
                PaintValue = null;
            }
            PaintValue = UObject.Instantiate(Value);

            //关闭纹理的可读、可写模式
            Utility.SetReadableDisabled(Importer);
        }

        /// <summary>
        /// 修正纹理尺寸为4的倍数
        /// </summary>
        public void FixMultipleOf4()
        {
            if (IsMultipleOf4)
                return;
            
            //计算新纹理尺寸
            int widthDiff = PaintValue.width % 4;
            int heightDiff = PaintValue.height % 4;
            int width = widthDiff == 0 ? PaintValue.width : (PaintValue.width + (4 - widthDiff));
            int height = heightDiff == 0 ? PaintValue.height : (PaintValue.height + (4 - heightDiff));

            //记录旧纹理数据
            int oldWidth = PaintValue.width;
            int oldHeight = PaintValue.height;
            Color[] oldColors = PaintValue.GetPixels();

            //构造新纹理
            PaintValue.Reinitialize(width, height);
            if (Format == FileFormat.JPG)
            {
                for (int h = 0; h < height; h++)
                {
                    for (int w = 0; w < width; w++)
                    {
                        if (h < oldHeight && w < oldWidth)
                        {
                            PaintValue.SetPixel(w, h, oldColors.GetPixel(oldWidth, w, h));
                        }
                        else
                        {
                            bool greaterW = w >= oldWidth;
                            bool greaterH = h >= oldHeight;
                            if (greaterH && greaterW)
                            {
                                PaintValue.SetPixel(w, h, oldColors[oldHeight * oldWidth - 1]);
                            }
                            else if (greaterH && !greaterW)
                            {
                                PaintValue.SetPixel(w, h, oldColors.GetPixel(oldWidth, w, oldHeight - 1));
                            }
                            else if (!greaterH && greaterW)
                            {
                                PaintValue.SetPixel(w, h, oldColors.GetPixel(oldWidth, oldWidth - 1, h));
                            }
                            else
                            {
                                PaintValue.SetPixel(w, h, Color.clear);
                            }
                        }
                    }
                }
            }
            else
            {
                for (int h = 0; h < height; h++)
                {
                    for (int w = 0; w < width; w++)
                    {
                        if (h < oldHeight && w < oldWidth)
                        {
                            PaintValue.SetPixel(w, h, oldColors.GetPixel(oldWidth, w, h));
                        }
                        else
                        {
                            PaintValue.SetPixel(w, h, Color.clear);
                        }
                    }
                }
            }
            PaintValue.Apply();
        }

        /// <summary>
        /// 剪切掉边缘空白像素
        /// </summary>
        public void CutBlankPixels()
        {
            if (Format == FileFormat.JPG)
                return;
            
            //计算非空白像素的区域
            int startRow = 0;
            for (int i = 0; i < PaintValue.height; i++)
            {
                if (PaintValue.IsBlankRow(i))
                {
                    startRow = i + 1;
                }
                else
                {
                    break;
                }
            }
            int endRow = PaintValue.height - 1;
            for (int i = PaintValue.height - 1; i >= 0; i--)
            {
                if (PaintValue.IsBlankRow(i))
                {
                    endRow = i - 1;
                }
                else
                {
                    break;
                }
            }
            int startColumn = 0;
            for (int i = 0; i < PaintValue.width; i++)
            {
                if (PaintValue.IsBlankColumn(i))
                {
                    startColumn = i + 1;
                }
                else
                {
                    break;
                }
            }
            int endColumn = PaintValue.width - 1;
            for (int i = PaintValue.width - 1; i >= 0; i--)
            {
                if (PaintValue.IsBlankColumn(i))
                {
                    endColumn = i - 1;
                }
                else
                {
                    break;
                }
            }
            
            //构造新纹理
            RectInt area = new RectInt(startColumn, startRow, endColumn - startColumn + 1, endRow - startRow + 1);
            Color[] newColors = PaintValue.GetPixels(area);
            PaintValue.Reinitialize(area.width, area.height);
            PaintValue.SetPixels(newColors);
            PaintValue.Apply();
        }

        /// <summary>
        /// 灰度化
        /// </summary>
        public void Grayscale()
        {
            for (int h = 0; h < PaintValue.height; h++)
            {
                for (int w = 0; w < PaintValue.width; w++)
                {
                    PaintValue.SetPixel(w, h, PaintValue.GetPixel(w, h).Grayscale());
                }
            }
            PaintValue.Apply();
        }

        /// <summary>
        /// 亮度调节
        /// </summary>
        /// <param name="brightness">亮度</param>
        public void AdjustBrightness(float brightness)
        {
            if (_rawPixels == null || _rawPixels.Length != (PaintValue.width * PaintValue.height))
            {
                Color[] rawColors = PaintValue.GetPixels();
                _rawPixels = new Color[rawColors.Length];
                for (int i = 0; i < rawColors.Length; i++)
                {
                    _rawPixels[i] = rawColors[i];
                }
            }

            Color[] colors = PaintValue.GetPixels();
            for (int i = 0; i < colors.Length; i++)
            {
                Color color = _rawPixels[i] * brightness;
                color.a = _rawPixels[i].a;
                colors[i] = color;
            }
            PaintValue.SetPixels(colors);
            PaintValue.Apply();
        }

        /// <summary>
        /// 亮度调节保存
        /// </summary>
        public void AdjustBrightnessSave()
        {
            _rawPixels = null;
        }

        /// <summary>
        /// 亮度调节还原
        /// </summary>
        public void AdjustBrightnessRestore()
        {
            if (_rawPixels == null || _rawPixels.Length != (PaintValue.width * PaintValue.height))
            {
                _rawPixels = null;
                return;
            }

            Color[] colors = PaintValue.GetPixels();
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = _rawPixels[i];
            }
            PaintValue.SetPixels(colors);
            PaintValue.Apply();
            _rawPixels = null;
        }

        /// <summary>
        /// 饱和度调节
        /// </summary>
        /// <param name="saturation">饱和度</param>
        public void AdjustSaturation(float saturation)
        {
            if (_rawPixels == null || _rawPixels.Length != (PaintValue.width * PaintValue.height))
            {
                Color[] rawColors = PaintValue.GetPixels();
                _rawPixels = new Color[rawColors.Length];
                for (int i = 0; i < rawColors.Length; i++)
                {
                    _rawPixels[i] = rawColors[i];
                }
            }
            if (_targetPixels == null || _targetPixels.Length != (PaintValue.width * PaintValue.height))
            {
                Color[] rawColors = PaintValue.GetPixels();
                _targetPixels = new Color[rawColors.Length];
                for (int i = 0; i < rawColors.Length; i++)
                {
                    _targetPixels[i] = rawColors[i].SaturationMax();
                }
            }

            Color[] colors = PaintValue.GetPixels();
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.Lerp(_rawPixels[i], _targetPixels[i], saturation);
            }
            PaintValue.SetPixels(colors);
            PaintValue.Apply();
        }

        /// <summary>
        /// 饱和度调节保存
        /// </summary>
        public void AdjustSaturationSave()
        {
            _rawPixels = null;
            _targetPixels = null;
        }

        /// <summary>
        /// 饱和度调节还原
        /// </summary>
        public void AdjustSaturationRestore()
        {
            if (_rawPixels == null || _rawPixels.Length != (PaintValue.width * PaintValue.height)
                || _targetPixels == null || _targetPixels.Length != (PaintValue.width * PaintValue.height))
            {
                _rawPixels = null;
                _targetPixels = null;
                return;
            }

            Color[] colors = PaintValue.GetPixels();
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = _rawPixels[i];
            }
            PaintValue.SetPixels(colors);
            PaintValue.Apply();
            _rawPixels = null;
            _targetPixels = null;
        }

        /// <summary>
        /// 明暗度调节
        /// </summary>
        /// <param name="value">明暗度</param>
        public void AdjustValue(float value)
        {
            if (_rawPixels == null || _rawPixels.Length != (PaintValue.width * PaintValue.height))
            {
                Color[] rawColors = PaintValue.GetPixels();
                _rawPixels = new Color[rawColors.Length];
                for (int i = 0; i < rawColors.Length; i++)
                {
                    _rawPixels[i] = rawColors[i];
                }
            }
            if (_targetPixels == null || _targetPixels.Length != (PaintValue.width * PaintValue.height))
            {
                Color[] rawColors = PaintValue.GetPixels();
                _targetPixels = new Color[rawColors.Length];
                for (int i = 0; i < rawColors.Length; i++)
                {
                    _targetPixels[i] = rawColors[i].ValueMax();
                }
            }

            Color[] colors = PaintValue.GetPixels();
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.Lerp(_rawPixels[i], _targetPixels[i], value);
            }
            PaintValue.SetPixels(colors);
            PaintValue.Apply();
        }

        /// <summary>
        /// 明暗度调节保存
        /// </summary>
        public void AdjustValueSave()
        {
            _rawPixels = null;
            _targetPixels = null;
        }

        /// <summary>
        /// 明暗度调节还原
        /// </summary>
        public void AdjustValueRestore()
        {
            if (_rawPixels == null || _rawPixels.Length != (PaintValue.width * PaintValue.height)
                || _targetPixels == null || _targetPixels.Length != (PaintValue.width * PaintValue.height))
            {
                _rawPixels = null;
                _targetPixels = null;
                return;
            }

            Color[] colors = PaintValue.GetPixels();
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = _rawPixels[i];
            }
            PaintValue.SetPixels(colors);
            PaintValue.Apply();
            _rawPixels = null;
            _targetPixels = null;
        }

        /// <summary>
        /// 左右镜像
        /// </summary>
        public void LeftRightMirror()
        {
            int widthHalf = PaintValue.width / 2;
            for (int h = 0; h < PaintValue.height; h++)
            {
                for (int w = 0; w < widthHalf; w++)
                {
                    int mirrorIndex = PaintValue.width - 1 - w;
                    Color color = PaintValue.GetPixel(w, h);
                    PaintValue.SetPixel(w, h, PaintValue.GetPixel(mirrorIndex, h));
                    PaintValue.SetPixel(mirrorIndex, h, color);
                }
            }
            PaintValue.Apply();
        }

        /// <summary>
        /// 上下镜像
        /// </summary>
        public void TopBottomMirror()
        {
            int heightHalf = PaintValue.height / 2;
            for (int w = 0; w < PaintValue.width; w++)
            {
                for (int h = 0; h < heightHalf; h++)
                {
                    int mirrorIndex = PaintValue.height - 1 - h;
                    Color color = PaintValue.GetPixel(w, h);
                    PaintValue.SetPixel(w, h, PaintValue.GetPixel(w, mirrorIndex));
                    PaintValue.SetPixel(w, mirrorIndex, color);
                }
            }
            PaintValue.Apply();
        }

        /// <summary>
        /// 保存
        /// </summary>
        public void Save()
        {
            WriteToFile(PaintValue, FullPath, Path);
            ApplySettings(Path);
        }

        /// <summary>
        /// 另存
        /// </summary>
        /// <param name="fullPath">保存的完整路径</param>
        public void SaveAs(string fullPath)
        {
            string path = "Assets" + fullPath.Replace(Application.dataPath, "");
            WriteToFile(PaintValue, fullPath, path);
            ApplySettings(path);
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

            if (PaintValue != null)
            {
                UObject.DestroyImmediate(PaintValue);
                PaintValue = null;
            }

            _rawPixels = null;
            _targetPixels = null;
        }

        /// <summary>
        /// 写入到文件
        /// </summary>
        /// <param name="texture">纹理</param>
        /// <param name="fullPath">完整路径</param>
        /// <param name="path">资源路径</param>
        private void WriteToFile(Texture2D texture, string fullPath, string path)
        {
            byte[] bytes = null;
            if (Format == FileFormat.JPG) bytes = texture.EncodeToJPG();
            else if (Format == FileFormat.PNG) bytes = texture.EncodeToPNG();
            else if (Format == FileFormat.TGA) bytes = texture.EncodeToTGA();
            else return;

            File.WriteAllBytes(fullPath, bytes);
            AssetDatabase.ImportAsset(path);
        }

        /// <summary>
        /// 应用纹理设置
        /// </summary>
        /// <param name="path">纹理路径</param>
        private void ApplySettings(string path)
        {
            Importer = AssetImporter.GetAtPath(path) as TextureImporter;
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
    }

    /// <summary>
    /// 纹理文件格式
    /// </summary>
    public enum FileFormat
    {
        JPG,
        PNG,
        TGA,
        Unknown
    }
}