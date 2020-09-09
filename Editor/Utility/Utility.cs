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
        /// 修正纹理尺寸为4的倍数（所有纹理）
        /// </summary>
        [MenuItem("Tools/Texture Processor/Resize To Multiple Of 4 All")]
        public static void ResizeToMultipleOf4All()
        {
            TextureProcessor processor = new TextureProcessor();
            List<TextrueProcessedFeedback> feedbacks = processor.ResizeToMultipleOf4();

            Log("已完成纹理修正 " + feedbacks.Count + " 个！");
            for (int i = 0; i < feedbacks.Count; i++)
            {
                string message = string.Format("修正：{0}，原始存储内存：<color=red>{1}</color>，原始运行内存：<color=red>{2}</color>，修正存储内存：<color=green>{3}</color>，修正运行内存：<color=green>{4}</color>"
                    , feedbacks[i].Name, feedbacks[i].RawStorageMemory, feedbacks[i].RawRuntimeMemory, feedbacks[i].StorageMemory, feedbacks[i].RuntimeMemory);
                Log(message, feedbacks[i].Value);
            }
        }

        /// <summary>
        /// 修正纹理尺寸为4的倍数
        /// </summary>
        [MenuItem("Tools/Texture Processor/Resize To Multiple Of 4")]
        public static void ResizeToMultipleOf4()
        {
            
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
        /// 创建纹理代理
        /// </summary>
        /// <param name="guids">纹理GUID</param>
        /// <returns>纹理代理集合</returns>
        public static List<TextureAgent> CreataAgents(string[] guids)
        {
            List<TextureAgent> agents = new List<TextureAgent>();
            for (int i = 0; i < guids.Length; i++)
            {
                TextureAgent agent = CreataAgent(guids[i]);
                if (agent != null)
                {
                    agents.Add(agent);
                }
            }
            return agents;
        }

        /// <summary>
        /// 创建纹理代理
        /// </summary>
        /// <param name="guid">纹理GUID</param>
        /// <returns>纹理代理</returns>
        public static TextureAgent CreataAgent(string guid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.EndsWith(".png") || path.EndsWith(".PNG"))
            {
                return new PngAgent(guid);
            }
            if (path.EndsWith(".jpg") || path.EndsWith(".JPG"))
            {
                return new JpgAgent(guid);
            }
            if (path.EndsWith(".tga") || path.EndsWith(".TGA"))
            {
                return new TgaAgent(guid);
            }
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
        /// <param name="agent">纹理代理</param>
        public static void SetReadableEnable(TextureAgent agent)
        {
            TextureImporter importer = AssetImporter.GetAtPath(agent.Path) as TextureImporter;
            if (!importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }
        }

        /// <summary>
        /// 关闭纹理的可读、可写模式
        /// </summary>
        /// <param name="agent">纹理代理</param>
        public static void SetReadableDisabled(TextureAgent agent)
        {
            TextureImporter importer = AssetImporter.GetAtPath(agent.Path) as TextureImporter;
            if (importer.isReadable)
            {
                importer.isReadable = false;
                importer.SaveAndReimport();
            }
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