using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UObject = UnityEngine.Object;

namespace HT.TextureProcessor
{
    /// <summary>
    /// 纹理处理器实用工具
    /// </summary>
    public static class Utility
    {
        private static MethodInfo _getStorageMemorySize;

        /// <summary>
        /// 缩放纹理尺寸为4的倍数（所有纹理）
        /// </summary>
        [MenuItem("HT/Texture Processor/Resize To Multiple Of 4 All")]
        public static void ResizeToMultipleOf4All()
        {
            if (EditorUtility.DisplayDialog("Prompt", "Are you sure you want to resize to multiple of 4 at all texture2d? this is maybe time consuming!", "Yes", "No"))
            {
                List<TextrueResizeFeedback> feedbacks = null;
                TimeSpan timeSpan = ExecutionInTimeMonitor(() =>
                {
                    TextureResizer resizer = new TextureResizer();
                    feedbacks = resizer.ResizeToMultipleOf4();
                    resizer.Dispose();
                });
                TextrueResizeFeedbackWindow.OpenWindow(feedbacks, timeSpan);
            }
        }

        /// <summary>
        /// 打开纹理缩放器
        /// </summary>
        [MenuItem("HT/Texture Processor/Texture Resizer")]
        public static void Resizer()
        {
            TextureResizerWindow.OpenWindow();
        }

        /// <summary>
        /// 打开纹理绘画器
        /// </summary>
        [MenuItem("HT/Texture Processor/Texture Painter")]
        public static void Painter()
        {
            TexturePainterWindow.OpenWindow();
        }

        /// <summary>
        /// 查找纹理
        /// </summary>
        /// <param name="paths">查找路径</param>
        /// <returns>所有纹理GUID</returns>
        internal static string[] FindTextures(string[] paths)
        {
            return AssetDatabase.FindAssets("t:Texture2D", paths);
        }

        /// <summary>
        /// 创建纹理缩放代理
        /// </summary>
        /// <param name="guids">纹理GUID</param>
        /// <returns>纹理缩放代理集合</returns>
        internal static List<TextureResizeAgent> CreataResizeAgents(string[] guids)
        {
            List<TextureResizeAgent> agents = new List<TextureResizeAgent>();
            for (int i = 0; i < guids.Length; i++)
            {
                TextureResizeAgent agent = CreataResizeAgent(guids[i]);
                if (agent != null)
                {
                    agents.Add(agent);
                }
            }
            return agents;
        }

        /// <summary>
        /// 创建纹理缩放代理
        /// </summary>
        /// <param name="guid">纹理GUID</param>
        /// <returns>纹理缩放代理</returns>
        internal static TextureResizeAgent CreataResizeAgent(string guid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.IsPng()) return new PngResizeAgent(guid, path);
            if (path.IsJpg()) return new JpgResizeAgent(guid, path);
            if (path.IsTga()) return new TgaResizeAgent(guid, path);
            LogError("暂不支持此种文件格式的纹理：" + path);
            return null;
        }
        
        /// <summary>
        /// 获取纹理占用的存储内存大小
        /// </summary>
        /// <param name="texture">纹理</param>
        /// <returns>存储内存大小</returns>
        public static long GetStorageMemorySize(UObject texture)
        {
            if (_getStorageMemorySize == null)
            {
                Type type = Type.GetType("UnityEditor.TextureUtil,UnityEditor");
                _getStorageMemorySize = type.GetMethod("GetStorageMemorySize", BindingFlags.Static | BindingFlags.Public);
            }

            return (int)_getStorageMemorySize.Invoke(null, new object[] { texture });
        }

        /// <summary>
        /// 获取纹理占用的运行内存大小
        /// </summary>
        /// <param name="texture">纹理</param>
        /// <returns>运行内存大小</returns>
        public static long GetRuntimeMemorySize(UObject texture)
        {
            return Profiler.GetRuntimeMemorySizeLong(texture);
        }

        /// <summary>
        /// 将字节流转换为显示文本
        /// </summary>
        /// <param name="bytes">字节流</param>
        /// <returns>显示文本</returns>
        public static string FormatBytes(long bytes)
        {
            return EditorUtility.FormatBytes(bytes);
        }

        /// <summary>
        /// 将字节流转换为增量显示文本
        /// </summary>
        /// <param name="bytes">字节流</param>
        /// <returns>显示文本</returns>
        public static string FormatIncrementBytes(long bytes)
        {
            return bytes > 0 ? ("↓" + EditorUtility.FormatBytes(bytes)) : ("↑" + EditorUtility.FormatBytes(-bytes));
        }

        /// <summary>
        /// 开启纹理的可读、可写模式
        /// </summary>
        /// <param name="importer">纹理导入器</param>
        public static void SetReadableEnable(TextureImporter importer)
        {
            if (!importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }
        }

        /// <summary>
        /// 关闭纹理的可读、可写模式
        /// </summary>
        /// <param name="importer">纹理导入器</param>
        public static void SetReadableDisabled(TextureImporter importer)
        {
            if (importer.isReadable)
            {
                importer.isReadable = false;
                importer.SaveAndReimport();
            }
        }

        /// <summary>
        /// 在时间监控中执行
        /// </summary>
        /// <param name="action">执行的方法</param>
        /// <returns>消耗的时间</returns>
        public static TimeSpan ExecutionInTimeMonitor(Action action)
        {
            DateTime start = DateTime.Now;
            action();
            DateTime end = DateTime.Now;
            return end - start;
        }
        
        /// <summary>
        /// 从纹理的图像源中获取图像颜色
        /// </summary>
        /// <param name="colors">图像源</param>
        /// <param name="width">纹理宽度</param>
        /// <param name="x">横向坐标</param>
        /// <param name="y">纵向坐标</param>
        /// <returns>颜色</returns>
        public static Color GetPixel(this Color[] colors, int width, int x, int y)
        {
            return colors[y * width + x];
        }

        /// <summary>
        /// 从纹理中获取指定区域的图像颜色
        /// </summary>
        /// <param name="texture">纹理</param>
        /// <param name="area">区域</param>
        /// <returns>颜色</returns>
        public static Color[] GetPixels(this Texture2D texture, RectInt area)
        {
            Color[] colors = new Color[area.width * area.height];
            for (int h = 0; h < area.height; h++)
            {
                for (int w = 0; w < area.width; w++)
                {
                    int index = h * area.width + w;
                    colors[index] = texture.GetPixel(area.x + w, area.y + h);
                }
            }
            return colors;
        }

        /// <summary>
        /// 灰度化（饱和度最低的颜色）
        /// </summary>
        /// <param name="color">原始颜色</param>
        /// <returns>灰度化后的颜色</returns>
        public static Color Grayscale(this Color color)
        {
            float grayscale = color.grayscale;
            return new Color(grayscale, grayscale, grayscale, color.a);
        }

        /// <summary>
        /// 此颜色饱和度最高的颜色
        /// </summary>
        /// <param name="color">原始颜色</param>
        /// <returns>饱和度最高的颜色</returns>
        public static Color SaturationMax(this Color color)
        {
            float h, s, v;
            float a = color.a;
            Color.RGBToHSV(color, out h, out s, out v);
            s = 1;
            color = Color.HSVToRGB(h, s, v);
            color.a = a;
            return color;
        }

        /// <summary>
        /// 此颜色明暗度最高的颜色
        /// </summary>
        /// <param name="color">原始颜色</param>
        /// <returns>明暗度最高的颜色</returns>
        public static Color ValueMax(this Color color)
        {
            float h, s, v;
            float a = color.a;
            Color.RGBToHSV(color, out h, out s, out v);
            v = 1;
            color = Color.HSVToRGB(h, s, v);
            color.a = a;
            return color;
        }

        /// <summary>
        /// 纹理的指定行是否为空白像素
        /// </summary>
        /// <param name="texture">纹理</param>
        /// <param name="row">行号</param>
        /// <returns>是否为空白像素</returns>
        public static bool IsBlankRow(this Texture2D texture, int row)
        {
            for (int i = 0; i < texture.width; i++)
            {
                if (!Mathf.Approximately(texture.GetPixel(i, row).a, 0f))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 纹理的指定列是否为空白像素
        /// </summary>
        /// <param name="texture">纹理</param>
        /// <param name="column">列号</param>
        /// <returns>是否为空白像素</returns>
        public static bool IsBlankColumn(this Texture2D texture, int column)
        {
            for (int i = 0; i < texture.height; i++)
            {
                if (!Mathf.Approximately(texture.GetPixel(column, i).a, 0f))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 纹理是否支持绘画器
        /// </summary>
        /// <param name="texture">纹理</param>
        /// <returns>是否支持</returns>
        public static bool IsSupportPaint(this Texture2D texture)
        {
            switch (texture.format)
            {
                case TextureFormat.ARGB32:
                case TextureFormat.RGBA32:
                case TextureFormat.BGRA32:
                case TextureFormat.RGB24:
                case TextureFormat.Alpha8:
                case TextureFormat.RGBAFloat:
                case TextureFormat.RGBAHalf:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 获取纹理文件格式
        /// </summary>
        /// <param name="path">纹理路径</param>
        /// <returns>文件格式</returns>
        public static FileFormat GetFileFormat(this string path)
        {
            if (path.IsJpg())
                return FileFormat.JPG;
            if (path.IsPng())
                return FileFormat.PNG;
            if (path.IsTga())
                return FileFormat.TGA;
            return FileFormat.Unknown;
        }

        /// <summary>
        /// 是否为Png格式纹理
        /// </summary>
        /// <param name="path">纹理路径</param>
        /// <returns>是、否</returns>
        public static bool IsPng(this string path)
        {
            return path.EndsWith(".png") || path.EndsWith(".PNG");
        }

        /// <summary>
        /// 是否为Jpg格式纹理
        /// </summary>
        /// <param name="path">纹理路径</param>
        /// <returns>是、否</returns>
        public static bool IsJpg(this string path)
        {
            return path.EndsWith(".jpg") || path.EndsWith(".JPG") || path.EndsWith(".jpeg") || path.EndsWith(".JPEG");
        }

        /// <summary>
        /// 是否为Tga格式纹理
        /// </summary>
        /// <param name="path">纹理路径</param>
        /// <returns>是、否</returns>
        public static bool IsTga(this string path)
        {
            return path.EndsWith(".tga") || path.EndsWith(".TGA");
        }

        /// <summary>
        /// 打印普通日志
        /// </summary>
        /// <param name="message">日志信息</param>
        /// <param name="context">日志目标</param>
        public static void Log(object message, UObject context)
        {
            Debug.Log("<color=cyan><b>[Texture Processor]</b></color> " + message, context);
        }

        /// <summary>
        /// 打印普通日志
        /// </summary>
        /// <param name="message">日志信息</param>
        public static void Log(object message)
        {
            Debug.Log("<color=cyan><b>[Texture Processor]</b></color> " + message);
        }

        /// <summary>
        /// 打印警告日志
        /// </summary>
        /// <param name="message">日志信息</param>
        /// <param name="context">日志目标</param>
        public static void LogWarning(object message, UObject context)
        {
            Debug.LogWarning("<color=yellow><b>[Texture Processor]</b></color> " + message, context);
        }

        /// <summary>
        /// 打印警告日志
        /// </summary>
        /// <param name="message">日志信息</param>
        public static void LogWarning(object message)
        {
            Debug.LogWarning("<color=yellow><b>[Texture Processor]</b></color> " + message);
        }

        /// <summary>
        /// 打印错误日志
        /// </summary>
        /// <param name="message">日志信息</param>
        /// <param name="context">日志目标</param>
        public static void LogError(object message, UObject context)
        {
            Debug.LogError("<color=red><b>[Texture Processor]</b></color> " + message, context);
        }

        /// <summary>
        /// 打印错误日志
        /// </summary>
        /// <param name="message">日志信息</param>
        public static void LogError(object message)
        {
            Debug.LogError("<color=red><b>[Texture Processor]</b></color> " + message);
        }
    }
}