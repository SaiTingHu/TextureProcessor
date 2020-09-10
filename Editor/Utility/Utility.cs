﻿using System;
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
        [MenuItem("Tools/Texture Processor/Resize To Multiple Of 4 All")]
        public static void ResizeToMultipleOf4All()
        {
            if (EditorUtility.DisplayDialog("Prompt", "Are you sure you want to resize to multiple of 4 at all texture2d,this is maybe time consuming!", "Yes", "No"))
            {
                List<TextrueResizeFeedback> feedbacks = null;
                TimeSpan timeSpan = ExecutionInTimeMonitor(() =>
                {
                    TextureResizer resizer = new TextureResizer();
                    feedbacks = resizer.ResizeToMultipleOf4();
                    resizer.Dispose();
                });

                Log("已完成纹理缩放 " + feedbacks.Count + " 个！[耗时：" + timeSpan.ToString(@"hh\:mm\:ss") + "]");
                for (int i = 0; i < feedbacks.Count; i++)
                {
                    Log(feedbacks[i].ToString(), feedbacks[i].Value);
                }
            }
        }

        /// <summary>
        /// 打开纹理缩放器
        /// </summary>
        [MenuItem("Tools/Texture Processor/Resizer")]
        public static void Resizer()
        {
            TextureResizerWindow window = EditorWindow.GetWindow<TextureResizerWindow>();
            window.titleContent.image = EditorGUIUtility.IconContent("ContentSizeFitter Icon").image;
            window.titleContent.text = "Texture Resizer";
        }

        /// <summary>
        /// 查找纹理
        /// </summary>
        /// <param name="paths">查找路径</param>
        /// <returns>所有纹理GUID</returns>
        public static string[] FindTextures(string[] paths)
        {
            return AssetDatabase.FindAssets("t:Texture2D", paths);
        }
        
        /// <summary>
        /// 创建纹理缩放代理
        /// </summary>
        /// <param name="guids">纹理GUID</param>
        /// <returns>纹理缩放代理集合</returns>
        public static List<TextureResizeAgent> CreataResizeAgents(string[] guids)
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
        public static TextureResizeAgent CreataResizeAgent(string guid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.IsPng()) return new PngResizeAgent(guid, path);
            if (path.IsJpg()) return new JpgResizeAgent(guid, path);
            if (path.IsTga()) return new TgaResizeAgent(guid, path);
            LogError("暂不支持此纹理格式：" + path);
            return null;
        }

        /// <summary>
        /// 获取纹理占用的存储内存大小
        /// </summary>
        /// <param name="texture">纹理</param>
        /// <returns>存储内存大小</returns>
        public static string GetStorageMemorySize(UObject texture)
        {
            if (_getStorageMemorySize == null)
            {
                Type type = Type.GetType("UnityEditor.TextureUtil,UnityEditor");
                _getStorageMemorySize = type.GetMethod("GetStorageMemorySize", BindingFlags.Static | BindingFlags.Public);
            }

            return EditorUtility.FormatBytes((int)_getStorageMemorySize.Invoke(null, new object[] { texture }));
        }

        /// <summary>
        /// 获取纹理占用的运行内存大小
        /// </summary>
        /// <param name="texture">纹理</param>
        /// <returns>运行内存大小</returns>
        public static string GetRuntimeMemorySize(UObject texture)
        {
            return EditorUtility.FormatBytes(Profiler.GetRuntimeMemorySizeLong(texture));
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
    }
}